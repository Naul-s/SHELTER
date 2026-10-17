using Shelter.Core;
using Shelter.Enemies;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>판자를 뜯을 바리케이드 + 배정받은 자리. Transform은 좀비가 서야 할 자리(쉘터 밖)다.</summary>
    public sealed class BarricadeTarget : IEnemyTarget
    {
        public Barricade Barricade { get; }
        public Transform Transform { get; }
        public IDamageable Damageable => Barricade.DamageReceiver;
        public EnemyTargetKind Kind => EnemyTargetKind.Barricade;

        public BarricadeTarget(Barricade barricade, Transform attackSlot)
        {
            Barricade = barricade;
            Transform = attackSlot;
        }

        /// <summary>배정받은 자리를 반납한다. 타겟이 바뀌거나 진입·사망할 때 EnemyBase가 호출한다.</summary>
        public void ReleaseSlot()
        {
            if (Barricade != null) Barricade.ReleaseAttackSlot(Transform);
        }
    }
}
