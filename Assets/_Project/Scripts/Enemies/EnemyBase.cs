using System;
using Shelter.Core;
using Shelter.Enemies.Animation;
using Shelter.Enemies.Combat;
using Shelter.Enemies.Movement;
using Shelter.Enemies.Targeting;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Enemies
{
    /// <summary>
    /// 좀비 한 마리의 오케스트레이터. 이동/공격/애니메이션/타겟 선택은 전부 인터페이스로 주입받아 조합하고,
    /// 이 클래스 자체는 상태 머신과 공유 상태(체력) 갱신만 담당한다.
    ///
    /// 주의: networking-architecture 스킬은 좀비 상태를 서버 권위(NetworkBehaviour)로 두라고 하지만,
    /// 이 프로젝트에는 아직 Photon Fusion이 도입되지 않아 순수 로컬 권위로 구현했다.
    /// 네트워킹 도입 시 TakeDamage/Die/타겟 갱신 등 상태 변경 지점(이 파일 안에만 존재)을 서버 RPC로 옮기면 된다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyBase : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyData _data;

        private IEnemyMovement _movement;
        private IEnemyAttack _attack;
        private IEnemyAnimationDriver _animationDriver;
        private IEnemyTargetSelector _targetSelector;

        private EnemyState _state;
        private IEnemyTarget _currentTarget;
        private float _currentHealth;
        private float _timeSinceTargetVisible;
        private float _reevaluateTimer;
        private float _corpseTimer;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _data.MaxHealth;
        public bool IsDead => _state == EnemyState.Dead;
        public EnemyState State => _state;

        public event Action<DamageInfo> Damaged;
        public event Action Died;

        /// <summary>사체 유지 시간이 끝나 풀로 반환해도 좋다는 신호. Spawner/Pool이 구독한다.</summary>
        public event Action<EnemyBase> DespawnRequested;

        private void Awake()
        {
            _movement = GetComponent<IEnemyMovement>();
            _attack = GetComponent<IEnemyAttack>();
            _animationDriver = GetComponent<IEnemyAnimationDriver>();
            _targetSelector = new WeightedEnemyTargetSelector(new PlayerTargetProvider(), new ShelterTargetProvider());

            if (_movement == null) Debug.LogError($"{name}: IEnemyMovement 구현 컴포넌트가 없습니다.", this);
            if (_attack == null) Debug.LogError($"{name}: IEnemyAttack 구현 컴포넌트가 없습니다.", this);
            if (_animationDriver == null) Debug.LogError($"{name}: IEnemyAnimationDriver 구현 컴포넌트가 없습니다.", this);

            _attack.AttackStarted += HandleAttackStarted;
            _attack.AttackImpact += HandleAttackImpact;
        }

        private void OnDestroy()
        {
            _attack.AttackStarted -= HandleAttackStarted;
            _attack.AttackImpact -= HandleAttackImpact;
        }

        /// <summary>
        /// EnemySpawner를 거치지 않고 씬에 직접 배치된 개체(고정 배치 보스 등)를 위한 자동 초기화.
        /// 스포너가 이미 Spawn을 호출했다면 이 시점엔 상태가 Spawning이 아니므로 아무 일도 하지 않는다.
        /// </summary>
        private void Start()
        {
            if (_state == EnemyState.Spawning && _data != null)
                Spawn(_data, transform.position, transform.rotation);
        }

        /// <summary>풀에서 꺼낸 인스턴스를 실제로 살아있는 좀비로 초기화한다.</summary>
        public void Spawn(EnemyData data, Vector3 position, Quaternion rotation)
        {
            _data = data;

            _movement.Initialize(_data.MoveSpeed, _data.AttackRange);
            _attack.Initialize(_data);
            _attack.CancelAttack();

            transform.SetPositionAndRotation(position, rotation);
            _movement.Warp(position);

            _currentHealth = _data.MaxHealth;
            _currentTarget = null;
            _timeSinceTargetVisible = 0f;
            _reevaluateTimer = 0f;
            _corpseTimer = 0f;

            _animationDriver.ResetForPool();
            _state = EnemyState.Seeking;
        }

        /// <summary>풀 반환 직전 호출 — 진행 중이던 공격/이동을 정리한다.</summary>
        public void ResetForPool()
        {
            _attack.CancelAttack();
            _movement.Stop();
            _currentTarget = null;
        }

        private void Update()
        {
            switch (_state)
            {
                case EnemyState.Seeking:
                    TickSeeking(Time.deltaTime);
                    break;
                case EnemyState.Attacking:
                    TickAttacking(Time.deltaTime);
                    break;
                case EnemyState.Dead:
                    TickDead(Time.deltaTime);
                    break;
            }
        }

        private void TickSeeking(float deltaTime)
        {
            UpdateTargetVisibility(deltaTime);

            _reevaluateTimer -= deltaTime;
            if (_reevaluateTimer <= 0f)
            {
                _reevaluateTimer = _data.TargetReevaluateInterval;
                ReevaluateTarget();
            }

            if (_currentTarget == null)
            {
                _movement.Stop();
                _animationDriver.SetMoveSpeed01(0f);
                return;
            }

            _movement.Resume();
            _movement.SetDestination(_currentTarget.Transform.position);
            _animationDriver.SetMoveSpeed01(_movement.CurrentSpeed / Mathf.Max(0.01f, _data.MoveSpeed));

            if (_attack.IsInRange(transform.position, _currentTarget.Transform.position))
            {
                _state = EnemyState.Attacking;
                _movement.Stop();
            }
        }

        private void TickAttacking(float deltaTime)
        {
            if (_currentTarget == null || _currentTarget.Damageable.IsDead)
            {
                _currentTarget = null;
                _state = EnemyState.Seeking;
                return;
            }

            UpdateTargetVisibility(deltaTime);
            FaceTarget();

            if (!_attack.IsInRange(transform.position, _currentTarget.Transform.position))
            {
                _state = EnemyState.Seeking;
                _movement.Resume();
                return;
            }

            _attack.Tick(deltaTime);
            _attack.TryBeginAttack(_currentTarget);
            _animationDriver.SetMoveSpeed01(0f);
        }

        private void TickDead(float deltaTime)
        {
            _corpseTimer -= deltaTime;
            if (_corpseTimer <= 0f)
                DespawnRequested?.Invoke(this);
        }

        private void ReevaluateTarget()
        {
            var context = new EnemyTargetContext
            {
                Origin = transform.position,
                Data = _data,
                CurrentTarget = _currentTarget,
                TimeSinceTargetVisible = _timeSinceTargetVisible
            };

            EnemyTargetDecision decision = _targetSelector.SelectTarget(context);
            if (!ReferenceEquals(decision.Target, _currentTarget))
                _timeSinceTargetVisible = 0f;

            _currentTarget = decision.Target;
        }

        private void UpdateTargetVisibility(float deltaTime)
        {
            if (_currentTarget == null || _currentTarget.Kind != EnemyTargetKind.Player) return;

            float distanceSqr = (_currentTarget.Transform.position - transform.position).sqrMagnitude;
            _timeSinceTargetVisible = distanceSqr <= _data.AggroRadius * _data.AggroRadius
                ? 0f
                : _timeSinceTargetVisible + deltaTime;
        }

        private void FaceTarget()
        {
            Vector3 toTarget = _currentTarget.Transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void HandleAttackStarted()
        {
            _animationDriver.PlayAttack();
        }

        private void HandleAttackImpact(IEnemyTarget target)
        {
            var info = new DamageInfo(_data.AttackPower, gameObject, target.Transform.position);
            target.Damageable.TakeDamage(info);
            OnAttackImpactApplied(target);
        }

        /// <summary>부머의 자폭, 스피터의 장판 등 타입별 추가 반응을 위한 확장 지점. 얕은 상속으로만 사용할 것.</summary>
        protected virtual void OnAttackImpactApplied(IEnemyTarget target)
        {
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float mitigated = CombatMath.ApplyDefense(damageInfo.Amount, _data.Defense);
            _currentHealth = Mathf.Max(0f, _currentHealth - mitigated);
            Damaged?.Invoke(damageInfo);

            if (_currentHealth <= 0f)
            {
                Die();
                return;
            }

            _animationDriver.PlayHitReaction();

            if (_data.SwitchTargetOnHit && damageInfo.Instigator != null &&
                damageInfo.Instigator.TryGetComponent(out PlayerHealth attacker))
            {
                _currentTarget = new EnemyTarget(attacker.transform, attacker, EnemyTargetKind.Player);
                _timeSinceTargetVisible = 0f;
            }
        }

        private void Die()
        {
            _state = EnemyState.Dead;
            _corpseTimer = _data.CorpseLifetime;

            _attack.CancelAttack();
            _movement.Stop();
            _animationDriver.PlayDeath();

            Died?.Invoke();
        }
    }
}
