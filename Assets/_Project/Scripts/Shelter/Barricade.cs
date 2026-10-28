using System;
using System.Collections.Generic;
using Shelter.Core;
using UnityEngine;

namespace Shelter.ShelterDefense
{
    /// <summary>
    /// 쉘터 진입 지점(창문·출입구) 하나를 막는 바리케이드. 좀비가 판자를 한 장씩 뜯고, 다 뜯기면(Breached)
    /// 좀비가 <see cref="InsidePosition"/>으로 넘어 들어온다.
    ///
    /// 플레이어 공격을 받지 않게 하려고 이 컴포넌트는 <see cref="IDamageable"/>을 구현하지 않는다. 피해 창구
    /// (<see cref="DamageReceiver"/>)는 GameObject에 붙지 않은 내부 객체라 무기의 GetComponent로는 찾을 수 없고,
    /// 좀비의 타겟 선택기만 이 속성으로 접근한다.
    ///
    /// 판자 수는 공유 상태라 서버 권위 대상이다. 상태 변경 지점은 <see cref="ApplyZombieHit"/>·<see cref="TryRepairOne"/> 둘뿐이다.
    /// </summary>
    public class Barricade : MonoBehaviour
    {
        private static readonly List<Barricade> s_activeInstances = new List<Barricade>();

        /// <summary>Update 루프에서 FindObjectOfType 없이 바리케이드를 찾기 위한 registry.</summary>
        public static IReadOnlyList<Barricade> ActiveInstances => s_activeInstances;

        [SerializeField] private BarricadeConfig _config;
        [SerializeField] private EntryKind _entryKind = EntryKind.Window;

        [Tooltip("떨어지는 순서대로 나열한 판자 오브젝트(보통 위 → 아래). 판자 수 = 이 배열 길이.")]
        [SerializeField] private Transform[] _boards;

        [Tooltip("판자를 다 뜯은 좀비가 넘어오기 직전에 서는 위치(쉘터 밖, NavMesh 위).")]
        [SerializeField] private Transform _outsidePoint;
        [Tooltip("넘어온 좀비가 착지하는 위치(쉘터 안, NavMesh 위).")]
        [SerializeField] private Transform _insidePoint;
        [Tooltip("판자를 뜯는 좀비가 서는 자리(쉘터 밖). 여러 마리가 나란히 붙을 수 있게 여러 개를 둔다.")]
        [SerializeField] private Transform[] _attackSlots;

        [Tooltip("창문을 넘을 때 궤적 최고점의 월드 Y. 창틀 높이 + 여유. 출입구는 쓰지 않는다.")]
        [SerializeField] private float _vaultApexY;

        private BarricadeModel _model;
        private int[] _slotClaims;
        private BoardDamageReceiver _receiver;

        public EntryKind EntryKind => _entryKind;
        public BarricadeConfig Config => _config;
        public IReadOnlyList<Transform> BoardTransforms => _boards;
        public int Boards => _model.Boards;
        public int MaxBoards => _model.MaxBoards;
        public bool IsBreached => _model.IsBreached;
        public Vector3 OutsidePosition => _outsidePoint.position;
        public Vector3 InsidePosition => _insidePoint.position;
        public float VaultApexY => _vaultApexY;

        /// <summary>좀비 전용 피해 창구. 공격 1회 = 판자 타격 1회이며 피해량은 무시한다.</summary>
        public IDamageable DamageReceiver => _receiver;

        /// <summary>판자가 떨어지지 않은 타격(HitsPerBoard &gt; 1일 때). 인자 = 남은 판자 수.</summary>
        public event Action<int> BoardHit;
        /// <summary>판자 한 장이 떨어졌다. 인자 = 떨어진 판자의 인덱스(<see cref="BoardTransforms"/> 기준).</summary>
        public event Action<int> BoardRemoved;
        /// <summary>마지막 판자가 떨어져 좀비가 들어올 수 있게 됐다.</summary>
        public event Action Breached;
        /// <summary>판자 한 장이 다시 붙었다. 인자 = 복구된 판자의 인덱스.</summary>
        public event Action<int> BoardRestored;

        private void Awake()
        {
            int boardCount = _boards != null ? _boards.Length : 0;
            _model = new BarricadeModel(boardCount, _config != null ? _config.HitsPerBoard : 1);
            _slotClaims = new int[_attackSlots != null ? _attackSlots.Length : 0];
            _receiver = new BoardDamageReceiver(this);

            if (_config == null) Debug.LogError($"{name}: BarricadeConfig가 연결되지 않았습니다.", this);
            if (_outsidePoint == null || _insidePoint == null)
            {
                Debug.LogError($"{name}: OutsidePoint/InsidePoint가 연결되지 않았습니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            s_activeInstances.Add(this);
        }

        private void OnDisable()
        {
            s_activeInstances.Remove(this);
        }

        /// <summary>좀비 공격 1회. 상태 변경 지점 — 네트워킹 도입 시 서버에서만 호출한다.</summary>
        private void ApplyZombieHit(DamageInfo damageInfo)
        {
            if (_model.IsBreached) return;

            int boardsBefore = _model.Boards;
            bool removed = _model.ApplyHit();
            _receiver.RaiseDamaged(damageInfo);

            if (!removed)
            {
                BoardHit?.Invoke(_model.Boards);
                return;
            }

            BoardRemoved?.Invoke(MaxBoards - boardsBefore);

            if (_model.IsBreached)
            {
                _receiver.RaiseDied();
                Breached?.Invoke();
            }
        }

        /// <summary>판자 한 장 수리. 수리 비용·거리 검증은 호출하는 쪽(economy-shop)이 서버에서 한다.</summary>
        public bool TryRepairOne()
        {
            if (!_model.TryRepairOne()) return false;

            BoardRestored?.Invoke(MaxBoards - _model.Boards);
            return true;
        }

        /// <summary>떨어진 판자를 모두 되붙인다. 되붙인 장수를 돌려준다. 비용 처리는 호출하는 쪽(BarricadeRepairStation) 책임.</summary>
        public int RepairAll()
        {
            int restored = 0;
            while (TryRepairOne()) restored++;
            return restored;
        }

        [ContextMenu("Repair All Boards")]
        private void RepairAllFromMenu()
        {
            if (Application.isPlaying) RepairAll();
        }

        /// <summary>
        /// 판자를 뜯을 자리를 하나 배정한다 — 가장 적게 점유된 자리 중 from에 가까운 곳. 반납은 <see cref="ReleaseAttackSlot"/>.
        /// 자리가 없으면 OutsidePoint를 돌려준다.
        /// </summary>
        public Transform ClaimAttackSlot(Vector3 from)
        {
            if (_attackSlots == null || _attackSlots.Length == 0) return _outsidePoint;

            int best = -1;
            float bestSqr = float.PositiveInfinity;
            for (int i = 0; i < _attackSlots.Length; i++)
            {
                if (_attackSlots[i] == null) continue;

                float sqr = (_attackSlots[i].position - from).sqrMagnitude;
                if (best < 0 || _slotClaims[i] < _slotClaims[best] || (_slotClaims[i] == _slotClaims[best] && sqr < bestSqr))
                {
                    best = i;
                    bestSqr = sqr;
                }
            }

            if (best < 0) return _outsidePoint;

            _slotClaims[best]++;
            return _attackSlots[best];
        }

        public void ReleaseAttackSlot(Transform slot)
        {
            if (_attackSlots == null || slot == null) return;

            for (int i = 0; i < _attackSlots.Length; i++)
            {
                if (_attackSlots[i] != slot) continue;
                _slotClaims[i] = Mathf.Max(0, _slotClaims[i] - 1);
                return;
            }
        }

        private void OnDrawGizmos()
        {
            if (_outsidePoint != null && _insidePoint != null)
            {
                Gizmos.color = _entryKind == EntryKind.Window ? Color.cyan : Color.yellow;
                Gizmos.DrawLine(_outsidePoint.position, _insidePoint.position);
                Gizmos.DrawWireSphere(_insidePoint.position, 0.2f);
            }

            if (_attackSlots == null) return;
            Gizmos.color = Color.red;
            foreach (Transform slot in _attackSlots)
                if (slot != null) Gizmos.DrawWireSphere(slot.position, 0.15f);
        }

        /// <summary>좀비 공격을 받는 내부 창구. GameObject 컴포넌트가 아니라서 플레이어 무기에 잡히지 않는다.</summary>
        private sealed class BoardDamageReceiver : IDamageable
        {
            private readonly Barricade _owner;

            public BoardDamageReceiver(Barricade owner)
            {
                _owner = owner;
            }

            public float CurrentHealth => _owner._model.Boards;
            public float MaxHealth => _owner._model.MaxBoards;
            public bool IsDead => _owner._model.IsBreached;

            public event Action<DamageInfo> Damaged;
            public event Action Died;

            public void TakeDamage(DamageInfo damageInfo)
            {
                _owner.ApplyZombieHit(damageInfo);
            }

            public void RaiseDamaged(DamageInfo damageInfo) => Damaged?.Invoke(damageInfo);
            public void RaiseDied() => Died?.Invoke();
        }
    }
}
