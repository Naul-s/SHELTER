using Shelter.Enemies;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>ShelterTargetPoint의 활성 인스턴스 registry를 조회한다.</summary>
    public sealed class ShelterTargetProvider : IShelterTargetProvider
    {
        public bool TryGetNearestShelterTarget(Vector3 origin, out IEnemyTarget target)
        {
            target = null;
            float bestSqrDistance = float.PositiveInfinity;
            ShelterTargetPoint best = null;

            var instances = ShelterTargetPoint.ActiveInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                ShelterTargetPoint candidate = instances[i];
                if (candidate == null || candidate.IsDead) continue;

                float sqrDistance = (candidate.transform.position - origin).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance) continue;

                bestSqrDistance = sqrDistance;
                best = candidate;
            }

            if (best == null) return false;

            target = new EnemyTarget(best.transform, best, EnemyTargetKind.Shelter);
            return true;
        }
    }
}
