using UnityEngine;

namespace Shelter.Enemies.Animation
{
    /// <summary>
    /// 좀비 Animator 구동. 이동은 MoveSpeed 블렌드 트리(Idle↔Walk, 감쇠로 부드럽게), 공격·피격·사망·넘기는
    /// 트리거 대신 상태를 직접 크로스페이드한다 — 다른 동작 중에 맞아도 그 자리에서 피격 동작으로 끊고 들어가고,
    /// 남아 있던 트리거가 뒤늦게 발동하는 일이 없다. 동작이 끝나면 컨트롤러의 종료 전이가 Locomotion 으로 블렌딩한다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimatorDriver : MonoBehaviour, IEnemyAnimationDriver
    {
        private const int BaseLayer = 0;

        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int AttackState = Animator.StringToHash("Attack");
        private static readonly int HitState = Animator.StringToHash("Hit");
        private static readonly int DeathState = Animator.StringToHash("Dying");
        private static readonly int VaultState = Animator.StringToHash("Vault");

        [Tooltip("이동 속도 변화가 걷기 블렌드에 반영되는 감쇠 시간(초). 멈추고 출발할 때 툭 끊기지 않게.")]
        [SerializeField, Min(0f)] private float _moveSpeedDampTime = 0.15f;
        [SerializeField, Min(0f)] private float _attackFadeTime = 0.15f;
        [Tooltip("피격 동작으로 넘어가는 시간(초). 짧을수록 맞는 순간 바로 움찔한다.")]
        [SerializeField, Min(0f)] private float _hitFadeTime = 0.06f;
        [Tooltip("피격 클립 앞부분의 정지 구간을 건너뛰는 시작 지점(초) — 맞자마자 몸이 젖혀지게.")]
        [SerializeField, Min(0f)] private float _hitStartTime = 0.25f;
        [SerializeField, Min(0f)] private float _deathFadeTime = 0.15f;
        [SerializeField, Min(0f)] private float _vaultFadeTime = 0.12f;

        private Animator _animator;
        private bool _hasAttack;
        private bool _hasHit;
        private bool _hasDeath;
        private bool _hasVault;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _hasAttack = _animator.HasState(BaseLayer, AttackState);
            _hasHit = _animator.HasState(BaseLayer, HitState);
            _hasDeath = _animator.HasState(BaseLayer, DeathState);
            _hasVault = _animator.HasState(BaseLayer, VaultState);
            if (!_hasAttack || !_hasHit)
                Debug.LogError($"{name}: Animator 컨트롤러에 Attack/Hit 상태가 없습니다.", this);
        }

        /// <summary>
        /// 위치는 NavMeshAgent(또는 창문 넘기 스크립트)가 결정한다. 이 콜백을 두면 Animator가 루트 모션을 transform에
        /// 직접 적용하지 않아, 에이전트 이동과 루트 모션이 겹쳐 미끄러지거나 떨리는 일이 없다.
        /// </summary>
        private void OnAnimatorMove()
        {
        }

        public void SetMoveSpeed01(float normalized01)
        {
            _animator.SetFloat(MoveSpeedHash, Mathf.Clamp01(normalized01), _moveSpeedDampTime, Time.deltaTime);
        }

        public void PlayAttack()
        {
            if (_hasAttack) _animator.CrossFadeInFixedTime(AttackState, _attackFadeTime, BaseLayer, 0f);
        }

        /// <summary>이미 피격 동작 중이어도 처음부터 다시 움찔한다(재생 간격은 EnemyBase가 EnemyData로 제한).</summary>
        public void PlayHitReaction()
        {
            if (_hasHit) _animator.CrossFadeInFixedTime(HitState, _hitFadeTime, BaseLayer, _hitStartTime);
        }

        public void PlayDeath()
        {
            if (_hasDeath) _animator.CrossFadeInFixedTime(DeathState, _deathFadeTime, BaseLayer, 0f);
        }

        public void PlayVault()
        {
            if (_hasVault) _animator.CrossFadeInFixedTime(VaultState, _vaultFadeTime, BaseLayer, 0f);
        }

        public void ResetForPool()
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
    }
}
