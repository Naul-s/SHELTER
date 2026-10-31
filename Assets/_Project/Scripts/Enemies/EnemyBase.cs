using System;
using Shelter.Core;
using Shelter.Enemies.Animation;
using Shelter.Enemies.Combat;
using Shelter.Enemies.Movement;
using Shelter.Enemies.Ragdoll;
using Shelter.Enemies.Targeting;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies
{
    /// <summary>
    /// 좀비 한 마리의 오케스트레이터. 이동/공격/애니메이션/타겟 선택은 전부 인터페이스로 주입받아 조합하고,
    /// 이 클래스 자체는 상태 머신과 공유 상태(체력) 갱신만 담당한다.
    ///
    /// 행동(2026-09-30 기획): 쉘터 밖 스폰 → 배정된 바리케이드의 자리로 이동 → 판자를 한 장씩 뜯음 →
    /// 판자가 다 뜯기면 진입 지점을 통과(출입구는 걸어서, 창문은 뛰어넘어) → 쉘터 안에서 가장 가까운 플레이어를 추격·공격.
    ///
    /// 주의: networking-architecture 스킬은 좀비 상태를 서버 권위(NetworkBehaviour)로 두라고 하지만,
    /// 이 프로젝트에는 아직 Photon Fusion이 도입되지 않아 순수 로컬 권위로 구현했다.
    /// 네트워킹 도입 시 TakeDamage/Die/타겟 갱신/진입 등 상태 변경 지점(이 파일 안에만 존재)을 서버로 옮기면 된다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyBase : MonoBehaviour, IDamageable
    {
        // 아래는 밸런스 값이 아닌 도착 판정 기하 상수(m, 초).
        private const float SlotArriveDistance = 0.4f;
        private const float EntryArriveDistance = 0.5f;
        private const float StopAtPointDistance = 0.1f;
        private const float RepathThreshold = 0.25f;
        private const float StuckAcceptDistance = 1.5f;
        private const float StuckAcceptTime = 0.75f;
        private const float StuckSpeed = 0.1f;

        [SerializeField] private EnemyData _data;

        private IEnemyMovement _movement;
        private IEnemyAttack _attack;
        private IEnemyAnimationDriver _animationDriver;
        private IEnemyTargetSelector _targetSelector;
        private EnemyRagdoll _ragdoll;

        private EnemyState _state;
        private IEnemyTarget _currentTarget;
        private float _currentHealth;
        private float _reevaluateTimer;
        private float _corpseTimer;

        private bool _insideShelter;
        private Barricade _assignedBarricade;
        private Vector3 _lastDestination;
        private bool _hasDestination;
        private float _stuckTimer;

        private Vector3 _traverseFrom;
        private Vector3 _traverseTo;
        private float _traverseElapsed;
        private float _traverseDuration;
        private float _vaultHeight;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _data.MaxHealth;
        public EnemyData Data => _data;
        /// <summary>마지막으로 피해를 준 공격자(막타 판정 — 처치 보상 지급 대상). 스폰 시 초기화.</summary>
        public GameObject LastAttacker { get; private set; }
        public bool IsDead => _state == EnemyState.Dead;
        public EnemyState State => _state;
        /// <summary>바리케이드를 넘어 쉘터 안으로 들어왔는가.</summary>
        public bool IsInsideShelter => _insideShelter;

        public event Action<DamageInfo> Damaged;
        public event Action Died;
        /// <summary>사망한 개체 자신을 넘겨주는 사망 이벤트 — 스포너가 생존 수를 세는 데 사용한다.</summary>
        public event Action<EnemyBase> EnemyDied;

        /// <summary>사체 유지 시간이 끝나 풀로 반환해도 좋다는 신호. Spawner/Pool이 구독한다.</summary>
        public event Action<EnemyBase> DespawnRequested;

        private void Awake()
        {
            _movement = GetComponent<IEnemyMovement>();
            _attack = GetComponent<IEnemyAttack>();
            _animationDriver = GetComponent<IEnemyAnimationDriver>();
            _targetSelector = new ShelterAssaultTargetSelector(new PlayerTargetProvider(), new BarricadeProvider());
            _ragdoll = GetComponent<EnemyRagdoll>();

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
        /// EnemySpawner를 거치지 않고 씬에 직접 배치된 개체를 위한 자동 초기화.
        /// 스포너가 이미 Spawn을 호출했다면 이 시점엔 상태가 Spawning이 아니므로 아무 일도 하지 않는다.
        /// </summary>
        private void Start()
        {
            if (_state == EnemyState.Spawning && _data != null)
                Spawn(_data, transform.position, transform.rotation);
        }

        /// <summary>풀에서 꺼낸 인스턴스를 실제로 살아있는 좀비로 초기화한다. assignedBarricade가 없으면 가장 가까운 바리케이드로 간다.</summary>
        public void Spawn(EnemyData data, Vector3 position, Quaternion rotation, Barricade assignedBarricade = null)
        {
            _data = data;

            _ragdoll?.ResetRagdoll();

            _movement.Initialize(_data.MoveSpeed, _data.AttackRange);
            _attack.Initialize(_data);
            _attack.CancelAttack();

            transform.SetPositionAndRotation(position, rotation);
            _movement.Warp(position);

            _currentHealth = _data.MaxHealth;
            LastAttacker = null;
            ClearTarget();
            _reevaluateTimer = 0f;
            _corpseTimer = 0f;
            _stuckTimer = 0f;
            _insideShelter = false;
            _assignedBarricade = assignedBarricade;

            _animationDriver.ResetForPool();
            _state = EnemyState.Seeking;
        }

        /// <summary>풀 반환 직전 호출 — 진행 중이던 공격/이동을 정리한다.</summary>
        public void ResetForPool()
        {
            _attack.CancelAttack();
            _movement.Stop();
            ClearTarget();
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
                case EnemyState.Entering:
                    TickEntering(Time.deltaTime);
                    break;
                case EnemyState.Dead:
                    TickDead(Time.deltaTime);
                    break;
            }
        }

        private void TickSeeking(float deltaTime)
        {
            _reevaluateTimer -= deltaTime;
            if (_reevaluateTimer <= 0f)
            {
                _reevaluateTimer = _data.TargetReevaluateInterval;
                ReevaluateTarget();
            }

            if (_currentTarget == null)
            {
                _movement.Stop();
                _hasDestination = false;
                _animationDriver.SetMoveSpeed01(0f);
                return;
            }

            if (_currentTarget is BarricadeTarget barricadeTarget)
            {
                TickApproachBarricade(barricadeTarget, deltaTime);
                return;
            }

            _movement.SetStoppingDistance(_data.AttackRange * 0.8f);
            MoveTo(_currentTarget.Transform.position);

            if (_attack.IsInRange(transform.position, _currentTarget.Transform.position))
                EnterAttacking();
        }

        /// <summary>판자가 남아 있으면 배정된 자리로, 다 뜯겼으면 진입 직전 위치로 간다.</summary>
        private void TickApproachBarricade(BarricadeTarget target, float deltaTime)
        {
            Barricade barricade = target.Barricade;
            bool breached = barricade.IsBreached;
            Vector3 goal = breached ? barricade.OutsidePosition : target.Transform.position;

            _movement.SetStoppingDistance(StopAtPointDistance);
            MoveTo(goal);

            float arriveDistance = breached ? EntryArriveDistance : SlotArriveDistance;
            float distanceSqr = FlatDistanceSqr(transform.position, goal);

            // 다른 좀비에 밀려 정확한 자리에 못 서는 경우: 충분히 가까운데 멈춰 있으면 도착으로 본다.
            bool stuckNearby = distanceSqr <= StuckAcceptDistance * StuckAcceptDistance && _movement.CurrentSpeed < StuckSpeed;
            _stuckTimer = stuckNearby ? _stuckTimer + deltaTime : 0f;

            if (distanceSqr > arriveDistance * arriveDistance && _stuckTimer < StuckAcceptTime) return;

            if (breached) BeginEntering(barricade);
            else EnterAttacking();
        }

        private void TickAttacking(float deltaTime)
        {
            if (_currentTarget == null)
            {
                _state = EnemyState.Seeking;
                return;
            }

            _attack.Tick(deltaTime);
            _animationDriver.SetMoveSpeed01(0f);

            if (_currentTarget is BarricadeTarget barricadeTarget)
            {
                if (barricadeTarget.Barricade.IsBreached)
                {
                    _attack.CancelAttack();
                    _state = EnemyState.Seeking;
                    return;
                }

                FaceTowards(barricadeTarget.Barricade.InsidePosition);
                _attack.TryBeginAttack(barricadeTarget);
                return;
            }

            if (_currentTarget.Damageable.IsDead)
            {
                _attack.CancelAttack();
                ClearTarget();
                _reevaluateTimer = 0f;
                _state = EnemyState.Seeking;
                return;
            }

            FaceTowards(_currentTarget.Transform.position);

            if (!_attack.IsInRange(transform.position, _currentTarget.Transform.position))
            {
                _state = EnemyState.Seeking;
                return;
            }

            _attack.TryBeginAttack(_currentTarget);
        }

        private void EnterAttacking()
        {
            _state = EnemyState.Attacking;
            _movement.Stop();
            _hasDestination = false;
            _stuckTimer = 0f;
        }

        /// <summary>판자가 다 뜯긴 진입 지점을 통과하기 시작한다. 상태 변경 지점 — 네트워킹 도입 시 서버에서 결정한다.</summary>
        private void BeginEntering(Barricade barricade)
        {
            ClearTarget();
            _attack.CancelAttack();
            _movement.BeginTraversal();

            _state = EnemyState.Entering;
            _stuckTimer = 0f;
            _traverseFrom = transform.position;
            _traverseTo = barricade.InsidePosition;
            _traverseElapsed = 0f;

            if (barricade.EntryKind == EntryKind.Window)
            {
                _traverseDuration = _data.WindowVaultDuration;
                float midY = (_traverseFrom.y + _traverseTo.y) * 0.5f;
                _vaultHeight = Mathf.Max(0f, barricade.VaultApexY - midY);
                _animationDriver.SetMoveSpeed01(0f);
                _animationDriver.PlayVault();
            }
            else
            {
                float distance = Mathf.Sqrt(FlatDistanceSqr(_traverseFrom, _traverseTo));
                _traverseDuration = Mathf.Max(0.2f, distance / Mathf.Max(0.1f, _data.MoveSpeed));
                _vaultHeight = 0f;
                _animationDriver.SetMoveSpeed01(1f);
            }
        }

        private void TickEntering(float deltaTime)
        {
            _traverseElapsed += deltaTime;
            float t = Mathf.Clamp01(_traverseElapsed / _traverseDuration);

            Vector3 position = Vector3.Lerp(_traverseFrom, _traverseTo, t);
            position.y += _vaultHeight * 4f * t * (1f - t);
            transform.position = position;
            FaceTowards(_traverseTo + (_traverseTo - _traverseFrom));

            if (t < 1f) return;

            _movement.EndTraversal(_traverseTo);
            _insideShelter = true;
            _reevaluateTimer = 0f;
            _state = EnemyState.Seeking;
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
                IsInsideShelter = _insideShelter,
                AssignedBarricade = _assignedBarricade
            };

            EnemyTargetDecision decision = _targetSelector.SelectTarget(context);
            if (ReferenceEquals(decision.Target, _currentTarget)) return;

            ReleaseBarricadeSlot();
            _currentTarget = decision.Target;
            _hasDestination = false;
            _stuckTimer = 0f;
        }

        private void ClearTarget()
        {
            ReleaseBarricadeSlot();
            _currentTarget = null;
            _hasDestination = false;
        }

        private void ReleaseBarricadeSlot()
        {
            if (_currentTarget is BarricadeTarget barricadeTarget)
                barricadeTarget.ReleaseSlot();
        }

        /// <summary>목적지가 RepathThreshold 이상 바뀐 경우에만 경로를 다시 계산한다(매 프레임 경로 재계산 방지).</summary>
        private void MoveTo(Vector3 destination)
        {
            _movement.Resume();

            if (!_hasDestination || (destination - _lastDestination).sqrMagnitude > RepathThreshold * RepathThreshold)
            {
                _movement.SetDestination(destination);
                _lastDestination = destination;
                _hasDestination = true;
            }

            _animationDriver.SetMoveSpeed01(_movement.CurrentSpeed / Mathf.Max(0.01f, _data.MoveSpeed));
        }

        private void FaceTowards(Vector3 worldPoint)
        {
            Vector3 toTarget = worldPoint - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private static float FlatDistanceSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
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
            if (IsDead || _state == EnemyState.Spawning) return;

            float mitigated = CombatMath.ApplyDefense(damageInfo.Amount, _data.Defense);
            _currentHealth = Mathf.Max(0f, _currentHealth - mitigated);
            if (damageInfo.Instigator != null) LastAttacker = damageInfo.Instigator;
            Damaged?.Invoke(damageInfo);

            if (_currentHealth <= 0f)
            {
                Die();
                return;
            }

            _animationDriver.PlayHitReaction();
        }

        private void Die()
        {
            // 창문을 넘던 중이면 공중에 멈추지 않게 가까운 쪽 끝에 내려놓는다.
            if (_state == EnemyState.Entering)
            {
                Vector3 landing = _traverseElapsed < _traverseDuration * 0.5f ? _traverseFrom : _traverseTo;
                transform.position = landing;
                _movement.EndTraversal(landing);
            }

            _state = EnemyState.Dead;
            _corpseTimer = _data.CorpseLifetime;

            _attack.CancelAttack();
            _movement.Stop();
            ClearTarget();
            _animationDriver.PlayDeath();

            Died?.Invoke();
            EnemyDied?.Invoke(this);
        }
    }
}
