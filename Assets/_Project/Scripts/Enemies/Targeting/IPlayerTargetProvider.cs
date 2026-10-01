using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    public interface IPlayerTargetProvider
    {
        /// <summary>origin 기준 radius 안에서 가장 가까운 생존 플레이어를 찾는다.</summary>
        bool TryGetNearestPlayer(Vector3 origin, float radius, out IEnemyTarget target);
    }
}
