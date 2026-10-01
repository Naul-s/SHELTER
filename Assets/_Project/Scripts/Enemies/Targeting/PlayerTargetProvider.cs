using Shelter.Enemies;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>PlayerHealth의 활성 인스턴스 registry를 조회한다 — Update에서 FindObjectOfType을 쓰지 않기 위함.</summary>
    public sealed class PlayerTargetProvider : IPlayerTargetProvider
    {
        public bool TryGetNearestPlayer(Vector3 origin, float radius, out IEnemyTarget target)
        {
            target = null;
            float bestSqrDistance = radius * radius;
            PlayerHealth best = null;

            var instances = PlayerHealth.ActiveInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                PlayerHealth candidate = instances[i];
                if (candidate == null || candidate.IsDead) continue;

                float sqrDistance = (candidate.transform.position - origin).sqrMagnitude;
                if (sqrDistance > bestSqrDistance) continue;

                bestSqrDistance = sqrDistance;
                best = candidate;
            }

            if (best == null) return false;

            target = new EnemyTarget(best.transform, best, EnemyTargetKind.Player);
            return true;
        }
    }
}
