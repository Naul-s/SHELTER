using UnityEngine;

namespace Shelter.Gathering
{
    /// <summary>
    /// 자원 노드 한 종류의 밸런싱 수치를 담는 ScriptableObject. 잔량/히트당 채집량/기본 재생성 시간/배치 규칙은
    /// 전부 여기서만 정의하고, 스폰 범위(ResourceSpawnArea)는 "어떤 자원을 몇 개까지" 만 결정한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Resource/Resource Node Data", fileName = "ResourceNodeData_")]
    public class ResourceNodeData : ScriptableObject
    {
        [Header("정체성")]
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private string _displayName;

        [Header("프리팹")]
        [SerializeField] private ResourceNode _nodePrefab;

        [Header("채집")]
        [Tooltip("노드 하나에서 채집할 수 있는 총량. 0이 되면 고갈되어 사라지고 재생성 대기에 들어간다.")]
        [SerializeField, Min(1)] private int _totalAmount = 10;
        [Tooltip("한 번의 히트(채집 판정)로 얻는 양.")]
        [SerializeField, Min(1)] private int _amountPerHit = 1;

        [Header("재생성 (기본값 — 스폰 범위에서 항목별로 덮어쓸 수 있음)")]
        [Tooltip("고갈 후 재생성까지 걸리는 최소 시간(분).")]
        [SerializeField, Min(0f)] private float _respawnDelayMinMinutes = 3f;
        [Tooltip("고갈 후 재생성까지 걸리는 최대 시간(분). 최소~최대 사이 무작위.")]
        [SerializeField, Min(0f)] private float _respawnDelayMaxMinutes = 5f;

        [Header("배치")]
        [Tooltip("같은 범위 안의 다른 노드와 최소한 이만큼(XZ 평면, m) 떨어져야 생성된다.")]
        [SerializeField, Min(0f)] private float _minSpacing = 1.5f;
        [SerializeField] private bool _randomYaw = true;
        [Tooltip("생성 시 프리팹 스케일에 곱해지는 무작위 배율 범위.")]
        [SerializeField] private Vector2 _scaleRange = Vector2.one;

        public ResourceType ResourceType => _resourceType;
        public string DisplayName => _displayName;
        public ResourceNode NodePrefab => _nodePrefab;

        public int TotalAmount => _totalAmount;
        public int AmountPerHit => _amountPerHit;

        public float RespawnDelayMinMinutes => _respawnDelayMinMinutes;
        public float RespawnDelayMaxMinutes => _respawnDelayMaxMinutes;

        public float MinSpacing => _minSpacing;
        public bool RandomYaw => _randomYaw;
        public Vector2 ScaleRange => _scaleRange;

        private void OnValidate()
        {
            if (_respawnDelayMaxMinutes < _respawnDelayMinMinutes)
                _respawnDelayMaxMinutes = _respawnDelayMinMinutes;

            _scaleRange.x = Mathf.Max(0.01f, _scaleRange.x);
            if (_scaleRange.y < _scaleRange.x)
                _scaleRange.y = _scaleRange.x;
        }
    }
}
