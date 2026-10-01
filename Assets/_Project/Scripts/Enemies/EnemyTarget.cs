using Shelter.Core;
using UnityEngine;

namespace Shelter.Enemies
{
    /// <summary>IDamageable을 IEnemyTarget 시점으로 감싸는 어댑터. Player/Shelter 네임스페이스가 Enemies를 몰라도 되게 한다.</summary>
    public sealed class EnemyTarget : IEnemyTarget
    {
        public Transform Transform { get; }
        public IDamageable Damageable { get; }
        public EnemyTargetKind Kind { get; }

        public EnemyTarget(Transform transform, IDamageable damageable, EnemyTargetKind kind)
        {
            Transform = transform;
            Damageable = damageable;
            Kind = kind;
        }
    }
}
