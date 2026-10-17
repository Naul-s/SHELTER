using System.Collections.Generic;
using Shelter.Enemies;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>
    /// PlayerHealth의 활성 인스턴스 registry를 조회한다 — Update에서 FindObjectOfType을 쓰지 않기 위함.
    /// 플레이어별 타겟 래퍼는 한 번만 만들어 재사용한다(재평가마다 할당하지 않도록).
    /// </summary>
    public sealed class PlayerTargetProvider : IPlayerTargetProvider
    {
        private readonly Dictionary<PlayerHealth, EnemyTarget> _targets = new Dictionary<PlayerHealth, EnemyTarget>();

        public bool TryGetNearestPlayer(Vector3 origin, float radius, out IEnemyTarget target)
        {
            target = null;
            float bestSqrDistance = float.IsPositiveInfinity(radius) ? float.PositiveInfinity : radius * radius;
            PlayerHealth best = null;

            var instances = PlayerHealth.ActiveInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                PlayerHealth candidate = instances[i];
                if (candidate == null || candidate.IsDead) continue;

                float sqrDistance = (candidate.TargetPoint.position - origin).sqrMagnitude;
                if (sqrDistance > bestSqrDistance) continue;

                bestSqrDistance = sqrDistance;
                best = candidate;
            }

            if (best == null) return false;

            if (!_targets.TryGetValue(best, out EnemyTarget cached))
            {
                cached = new EnemyTarget(best.TargetPoint, best, EnemyTargetKind.Player);
                _targets.Add(best, cached);
            }

            target = cached;
            return true;
        }
    }
}
