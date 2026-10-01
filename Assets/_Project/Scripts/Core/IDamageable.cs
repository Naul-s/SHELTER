using System;
using UnityEngine;

namespace Shelter.Core
{
    /// <summary>피해를 받을 수 있는 모든 대상(플레이어, 쉘터, 적 등)의 공통 계약.</summary>
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }

        void TakeDamage(DamageInfo damageInfo);

        event Action<DamageInfo> Damaged;
        event Action Died;
    }
}
