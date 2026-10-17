using System;
using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Combat
{
    /// <summary>
    /// 근접 공격: TryBeginAttack으로 윈드업을 시작하고, Tick에서 윈드업이 끝나는 순간 AttackImpact를 발생시킨다.
    /// 실제 애니메이션 이벤트 대신 데이터 기반 타이머를 쓰므로, 구매한 좀비 애니메이션 클립에 이벤트를 추가하지 않아도 된다.
    /// </summary>
    public class MeleeEnemyAttack : MonoBehaviour, IEnemyAttack
    {
        /// <summary>발밑(좀비 피벗)에서 서 있는 플레이어 머리까지 닿는 높이 허용치(m). 밸런스 값이 아닌 판정 기하 상수.</summary>
        private const float MaxVerticalReach = 2.2f;

        private EnemyData _data;
        private IEnemyTarget _pendingTarget;
        private float _windupTimer;
        private float _cooldownTimer;

        public bool IsAttacking { get; private set; }
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
            Vector3 offset = targetPosition - selfPosition;
            if (Mathf.Abs(offset.y) > MaxVerticalReach) return false;

            offset.y = 0f;
            return offset.sqrMagnitude <= _data.AttackRange * _data.AttackRange;
        }

        public bool TryBeginAttack(IEnemyTarget target)
        {
            if (IsAttacking || IsOnCooldown || target == null || target.Damageable.IsDead) return false;

            IsAttacking = true;
            _pendingTarget = target;
            _windupTimer = _data.AttackWindupTime;
            AttackStarted?.Invoke();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= deltaTime;

            if (!IsAttacking) return;

            _windupTimer -= deltaTime;
            if (_windupTimer > 0f) return;

            IsAttacking = false;
            _cooldownTimer = _data.AttackCooldown;

            if (_pendingTarget != null && !_pendingTarget.Damageable.IsDead)
                AttackImpact?.Invoke(_pendingTarget);

            _pendingTarget = null;
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            _pendingTarget = null;
            _windupTimer = 0f;
        }
    }
}
