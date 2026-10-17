using Shelter.Enemies;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Targeting
{
    /// <summary>타겟 선택 시점의 상황 정보. EnemyBase가 매 재평가 시점마다 채워서 selector에 전달한다.</summary>
    public struct EnemyTargetContext
    {
        public Vector3 Origin;
        public EnemyData Data;
        public IEnemyTarget CurrentTarget;
        /// <summary>바리케이드를 넘어 쉘터 안으로 들어왔는가.</summary>
        public bool IsInsideShelter;
        /// <summary>스폰 포인트가 지정한 바리케이드(없으면 가장 가까운 바리케이드를 고른다).</summary>
        public Barricade AssignedBarricade;
    }
}
