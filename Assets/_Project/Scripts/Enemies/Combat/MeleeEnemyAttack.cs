using System;
using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Combat
{
    /// <summary>
    /// 근접 공격: 예비동작(Windup) → 타격 순간 → 마무리(Recovery) 세 구간.
    /// 타격 순간은 공격 애니메이션의 내려치는 시점(EnemyData.AttackWindupTime)에 맞추고, 그 순간 대상의 거리·방향을 다시 판정한다 —
    /// 공격 모션이 보이자마자 맞는 게 아니라 팔이 닿을 때 범위 안에 있어야 맞는다(물러나거나 비켜서면 빗나감).
    /// 애니메이션 이벤트 대신 데이터 기반 타이머를 쓰므로 클립에 이벤트를 넣지 않아도 된다.
    /// </summary>
    public class MeleeEnemyAttack : MonoBehaviour, IEnemyAttack
    {
        /// <summary>발밑(좀비 피벗)에서 서 있는 플레이어 머리까지 닿는 높이 허용치(m). 밸런스 값이 아닌 판정 기하 상수.</summary>
        private const float MaxVerticalReach = 2.2f;
        /// <summary>휘두르는 팔이 공격을 시작하는 사거리보다 더 닿는 거리(m) — 몸을 숙이며 뻗는 만큼.</summary>
        private const float ImpactReachSlack = 0.35f;
        /// <summary>타격 순간 정면에서 이 각도 안에 있어야 맞는다(도). 팔 궤적의 좌우 폭.</summary>
        private const float ImpactHalfAngle = 70f;

        private enum Phase { Idle, Windup, Recovery }

        private EnemyData _data;
        private IEnemyTarget _pendingTarget;
        private Phase _phase;
        private float _phaseTimer;
        private float _cooldownTimer;

        public bool IsAttacking => _phase != Phase.Idle;
        public bool IsImpactPending => _phase == Phase.Windup;
        public bool IsOnCooldown => _cooldownTimer > 0f;

        public event Action AttackStarted;
        public event Action<IEnemyTarget> AttackImpact;

        public void Initialize(EnemyData data)
        {
            _data = data;
        }

        /// <summary>
        /// 사거리는 수평 거리로 판정한다 — 플레이어 타겟 지점이 머리(HMD) 높이라 3D 거리로는 발밑 기준 좀비가 닿지 못한다.
        /// 높이 차가 MaxVerticalReach를 넘으면(위층·점프 중) 사거리 밖으로 본다.
        /// </summary>
        public bool IsInRange(Vector3 selfPosition, Vector3 targetPosition)
        {
            return IsWithin(selfPosition, targetPosition, _data.AttackRange);
        }

        public bool TryBeginAttack(IEnemyTarget target)
        {
            if (IsAttacking || IsOnCooldown || target == null || target.Damageable.IsDead) return false;

            _phase = Phase.Windup;
            _phaseTimer = _data.AttackWindupTime;
            _pendingTarget = target;
            AttackStarted?.Invoke();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= deltaTime;

            if (_phase == Phase.Idle) return;

            _phaseTimer -= deltaTime;
            if (_phaseTimer > 0f) return;

            if (_phase == Phase.Recovery)
            {
                _phase = Phase.Idle;
                return;
            }

            // 팔이 닿는 순간
            _phase = Phase.Recovery;
            _phaseTimer = _data.AttackRecoveryTime;
            _cooldownTimer = _data.AttackCooldown;

            IEnemyTarget target = _pendingTarget;
            _pendingTarget = null;
            if (target != null && !target.Damageable.IsDead && CanReachAtImpact(target))
                AttackImpact?.Invoke(target);
        }

        public void CancelAttack()
        {
            _phase = Phase.Idle;
            _pendingTarget = null;
            _phaseTimer = 0f;
        }

        /// <summary>판자는 배정된 자리에서 뜯으므로 항상 닿는다(자리를 떠나면 EnemyBase가 공격을 취소). 플레이어는 거리·정면 각도를 본다.</summary>
        private bool CanReachAtImpact(IEnemyTarget target)
        {
            if (target.Kind == EnemyTargetKind.Barricade) return true;

            Vector3 self = transform.position;
            Vector3 targetPosition = target.Transform.position;
            if (!IsWithin(self, targetPosition, _data.AttackRange + ImpactReachSlack)) return false;

            Vector3 toTarget = targetPosition - self;
            toTarget.y = 0f;
            return toTarget.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, toTarget) <= ImpactHalfAngle;
        }

        private static bool IsWithin(Vector3 selfPosition, Vector3 targetPosition, float range)
        {
            Vector3 offset = targetPosition - selfPosition;
            if (Mathf.Abs(offset.y) > MaxVerticalReach) return false;

            offset.y = 0f;
            return offset.sqrMagnitude <= range * range;
        }
    }
}
