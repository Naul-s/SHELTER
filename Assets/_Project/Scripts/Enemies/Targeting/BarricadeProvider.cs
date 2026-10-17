using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>Barricade의 활성 인스턴스 registry를 조회한다 — Update에서 FindObjectOfType을 쓰지 않기 위함.</summary>
    public sealed class BarricadeProvider : IBarricadeProvider
    {
        public bool TryGetNearestBarricade(Vector3 origin, out Barricade barricade)
        {
            barricade = null;
            float bestSqrDistance = float.PositiveInfinity;

            var instances = Barricade.ActiveInstances;
            for (int i = 0; i < instances.Count; i++)
            {
                Barricade candidate = instances[i];
                if (candidate == null) continue;

                float sqrDistance = (candidate.OutsidePosition - origin).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance) continue;

                bestSqrDistance = sqrDistance;
                barricade = candidate;
            }

            return barricade != null;
        }
    }
}
