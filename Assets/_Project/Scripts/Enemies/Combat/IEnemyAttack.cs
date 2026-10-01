using System;
using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Combat
{
    /// <summary>공격 전략 추상화 — 근접(현재)/원거리(스피터 등) 구현체를 EnemyBase 변경 없이 교체할 수 있다.</summary>
    public interface IEnemyAttack
    {
        bool IsAttacking { get; }
        bool IsOnCooldown { get; }

        void Initialize(EnemyData data);
        bool IsInRange(Vector3 selfPosition, Vector3 targetPosition);
        bool TryBeginAttack(IEnemyTarget target);
        void Tick(float deltaTime);
        void CancelAttack();

        event Action AttackStarted;
        event Action<IEnemyTarget> AttackImpact;
    }
}
