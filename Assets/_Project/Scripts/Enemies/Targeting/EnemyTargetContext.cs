using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>타겟 선택 시점의 상황 정보. EnemyBase가 매 재평가 시점마다 채워서 selector에 전달한다.</summary>
    public struct EnemyTargetContext
    {
        public Vector3 Origin;
        public EnemyData Data;
        public IEnemyTarget CurrentTarget;
        public float TimeSinceTargetVisible;
    }
}
