using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.Gathering
{
    public enum ResourceAreaShape
    {
        Box,
        Circle
    }

    /// <summary>
    /// 지정한 범위 안에 자원 노드를 무작위로 생성하고, 고갈되면 설정한 시간 뒤 다시 생성한다.
    /// 어떤 자원을/몇 개까지/얼마 뒤에 재생성할지는 전부 인스펙터(entries + ResourceNodeData)에서 조절한다.
    /// 공유 상태(노드 생성/잔량)는 서버 권위 대상이므로 멀티플레이어 도입 시 이 컴포넌트의 로직은 서버에서만 돌린다
    /// (현재는 EnemySpawner와 동일하게 로컬 권위).
    /// </summary>
    public class ResourceSpawnArea : MonoBehaviour
    {
        private struct PendingRespawn
        {
            public float ReadyTime;
            public int EntryIndex;
        }

        [Header("범위 (이 오브젝트의 Transform 기준, 회전 반영)")]
        [SerializeField] private ResourceAreaShape _shape = ResourceAreaShape.Box;
        [Tooltip("Box: X/Z 크기(m).")]
        [SerializeField] private Vector2 _boxSize = new Vector2(20f, 20f);
        [Tooltip("Circle: 반지름(m).")]
        [SerializeField, Min(0.1f)] private float _radius = 10f;

        [Header("생성할 자원")]
        [SerializeField] private ResourceSpawnEntry[] _entries;
        [Tooltip("이 범위 안에 동시에 존재할 수 있는 전체 노드 수(재생성 대기 중인 것 포함).")]
        [SerializeField, Min(1)] private int _maxTotalCount = 10;

        [Header("초기 생성")]
        [SerializeField] private bool _spawnOnStart = true;
        [Tooltip("시작 시 즉시 생성할 개수. 전체 최대 개수를 넘지 않는다.")]
        [SerializeField, Min(0)] private int _initialSpawnCount = 5;

        [Header("재생성")]
        [Tooltip("끄면 고갈된 노드는 다시 생성되지 않는다.")]
        [SerializeField] private bool _enableRespawn = true;
        [Tooltip("배치할 자리를 못 찾아 재생성에 실패했을 때 다시 시도하기까지의 시간(초).")]
        [SerializeField, Min(0.5f)] private float _retryDelaySeconds = 5f;

        [Header("배치")]
        [Tooltip("지면으로 인식할 레이어. 자원 노드 자신의 레이어는 빼둘 것.")]
        [SerializeField] private LayerMask _groundMask = ~0;
        [Tooltip("켜면 아래로 레이캐스트해 지면 높이에 붙인다. 끄면 이 오브젝트의 높이를 그대로 쓴다.")]
        [SerializeField] private bool _snapToGround = true;
        [SerializeField] private bool _alignToGroundNormal;
        [SerializeField, Min(0.1f)] private float _raycastHeight = 30f;
        [SerializeField, Min(0.1f)] private float _raycastDistance = 60f;
        [Tooltip("노드 하나를 놓을 자리를 찾기 위해 시도하는 최대 횟수.")]
        [SerializeField, Min(1)] private int _maxPlacementAttempts = 20;
        [SerializeField] private Transform _nodeParent;

        private readonly List<ResourceNode> _aliveNodes = new List<ResourceNode>();
        private readonly Dictionary<ResourceNode, int> _entryIndexByNode = new Dictionary<ResourceNode, int>();
        private readonly Dictionary<ResourceNodeData, ResourceNodePool> _poolsByData = new Dictionary<ResourceNodeData, ResourceNodePool>();
        private readonly List<PendingRespawn> _pending = new List<PendingRespawn>();
        private int[] _aliveCounts;
        private int[] _pendingCounts;

        public int AliveCount => _aliveNodes.Count;
        public int PendingRespawnCount => _pending.Count;

        private void Awake()
        {
            int entryCount = _entries != null ? _entries.Length : 0;
            _aliveCounts = new int[entryCount];
            _pendingCounts = new int[entryCount];
        }

        private void Start()
        {
            if (_spawnOnStart)
                SpawnInitial();
        }

        private void Update()
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                PendingRespawn pending = _pending[i];
                if (Time.time < pending.ReadyTime) continue;

                if (TrySpawnNode(pending.EntryIndex))
                {
                    _pendingCounts[pending.EntryIndex]--;
                    _pending.RemoveAt(i);
                }
                else
                {
                    pending.ReadyTime = Time.time + _retryDelaySeconds;
                    _pending[i] = pending;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (ResourceNode node in _aliveNodes)
                node.Depleted -= HandleDepleted;
        }

        /// <summary>초기 생성 개수만큼(전체 최대 개수 이내) 가중치로 자원을 골라 채운다.</summary>
        public void SpawnInitial()
        {
            int target = Mathf.Min(_initialSpawnCount, _maxTotalCount);
            int failures = 0;

            while (Occupied() < target && failures < _maxPlacementAttempts)
            {
                int entryIndex = PickEntryIndexWithFreeSlot();
                if (entryIndex < 0) break;

                if (!TrySpawnNode(entryIndex))
                    failures++;
            }
        }

        private int Occupied() => _aliveNodes.Count + _pending.Count;

        private int PickEntryIndexWithFreeSlot()
        {
            if (_entries == null) return -1;

            var candidates = new List<int>(_entries.Length);
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i] != null && _entries[i].IsValid && _aliveCounts[i] + _pendingCounts[i] < _entries[i].MaxCount)
                    candidates.Add(i);
            }

            if (candidates.Count == 0) return -1;
            return WeightedRandomPicker.Pick(candidates, index => _entries[index].Weight);
        }

        private bool TrySpawnNode(int entryIndex)
        {
            ResourceSpawnEntry entry = _entries[entryIndex];
            if (!entry.IsValid) return false;
            if (!TryFindPlacement(entry.Data, out Vector3 position, out Quaternion rotation)) return false;

            if (!_poolsByData.TryGetValue(entry.Data, out ResourceNodePool pool))
            {
                pool = new ResourceNodePool(entry.Data.NodePrefab, _nodeParent != null ? _nodeParent : transform);
                _poolsByData.Add(entry.Data, pool);
            }

            ResourceNode node = pool.Get();
            Vector2 scaleRange = entry.Data.ScaleRange;
            node.transform.SetPositionAndRotation(position, rotation);
            node.transform.localScale = entry.Data.NodePrefab.transform.localScale * Random.Range(scaleRange.x, scaleRange.y);
            node.Initialize(entry.Data);
            node.Depleted += HandleDepleted;

            _aliveNodes.Add(node);
            _entryIndexByNode[node] = entryIndex;
            _aliveCounts[entryIndex]++;
            return true;
        }

        private void HandleDepleted(ResourceNode node)
        {
            node.Depleted -= HandleDepleted;

            if (!_entryIndexByNode.TryGetValue(node, out int entryIndex)) return;

            _entryIndexByNode.Remove(node);
            _aliveNodes.Remove(node);
            _aliveCounts[entryIndex]--;

            ResourceNodeData data = node.Data;
            if (_poolsByData.TryGetValue(data, out ResourceNodePool pool))
                pool.Release(node);

            if (!_enableRespawn) return;

            _pending.Add(new PendingRespawn
            {
                ReadyTime = Time.time + _entries[entryIndex].RollRespawnDelaySeconds(),
                EntryIndex = entryIndex
            });
            _pendingCounts[entryIndex]++;
        }

        private bool TryFindPlacement(ResourceNodeData data, out Vector3 position, out Quaternion rotation)
        {
            for (int attempt = 0; attempt < _maxPlacementAttempts; attempt++)
            {
                Vector3 candidate = transform.TransformPoint(RandomLocalPoint());
                Vector3 normal = Vector3.up;

                if (_snapToGround)
                {
                    Vector3 origin = candidate + Vector3.up * _raycastHeight;
                    if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundMask, QueryTriggerInteraction.Ignore))
                        continue;

                    candidate = hit.point;
                    normal = hit.normal;
                }

                if (!HasSpacing(candidate, data.MinSpacing)) continue;

                Quaternion align = _alignToGroundNormal ? Quaternion.FromToRotation(Vector3.up, normal) : Quaternion.identity;
                float yaw = data.RandomYaw ? Random.Range(0f, 360f) : 0f;
                position = candidate;
                rotation = align * Quaternion.Euler(0f, yaw, 0f);
                return true;
            }

            position = default;
            rotation = default;
            return false;
        }

        private bool HasSpacing(Vector3 candidate, float minSpacing)
        {
            foreach (ResourceNode other in _aliveNodes)
            {
                float required = Mathf.Max(minSpacing, other.Data.MinSpacing);
                Vector3 delta = other.transform.position - candidate;
                delta.y = 0f;
                if (delta.sqrMagnitude < required * required) return false;
            }

            return true;
        }

        private Vector3 RandomLocalPoint()
        {
            if (_shape == ResourceAreaShape.Box)
                return new Vector3(Random.Range(-_boxSize.x * 0.5f, _boxSize.x * 0.5f), 0f, Random.Range(-_boxSize.y * 0.5f, _boxSize.y * 0.5f));

            Vector2 point = Random.insideUnitCircle * _radius;
            return new Vector3(point.x, 0f, point.y);
        }

        private void OnValidate()
        {
            _boxSize.x = Mathf.Max(0.1f, _boxSize.x);
            _boxSize.y = Mathf.Max(0.1f, _boxSize.y);

            if (_entries == null) return;
            foreach (ResourceSpawnEntry entry in _entries)
                entry?.OnValidate();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 1f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (_shape == ResourceAreaShape.Box)
            {
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(_boxSize.x, 0.1f, _boxSize.y));
                return;
            }

            const int segments = 48;
            Vector3 previous = new Vector3(_radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 next = new Vector3(Mathf.Cos(angle) * _radius, 0f, Mathf.Sin(angle) * _radius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
