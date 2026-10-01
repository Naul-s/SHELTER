using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    public interface IShelterTargetProvider
    {
        bool TryGetNearestShelterTarget(Vector3 origin, out IEnemyTarget target);
    }
}
