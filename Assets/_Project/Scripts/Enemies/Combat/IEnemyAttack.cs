using System;
using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Combat
{
    /// <summary>공격 전략 추상화 — 근접(현재)/원거리(스피터 등) 구현체를 EnemyBase 변경 없이 교체할 수 있다.</summary>
    public interface IEnemyAttack
    {
        /// <summary>공격 동작(예비동작~마무리) 중인가. 이 동안 좀비는 제자리에 서 있는다.</summary>
        bool IsAttacking { get; }
        /// <summary>아직 타격 순간 전(예비동작)인가 — 이때 맞으면 공격이 끊길 수 있다.</summary>
        bool IsImpactPending { get; }
        bool IsOnCooldown { get; }

        void Initialize(EnemyData data);
        bool IsInRange(Vector3 selfPosition, Vector3 targetPosition);
        bool TryBeginAttack(IEnemyTarget target);
        void Tick(float deltaTime);
        void CancelAttack();

        event Action AttackStarted;
        /// <summary>타격 순간에 대상이 아직 닿는 범위에 있었을 때만 발생한다.</summary>
        event Action<IEnemyTarget> AttackImpact;
    }
}
