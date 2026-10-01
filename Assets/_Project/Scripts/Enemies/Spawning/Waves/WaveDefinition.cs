using System.Collections.Generic;
using Shelter.Enemies.Spawning;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 한 라운드(밤)의 스폰 계획 전체. "어떤 몹들을 어떤 비율로(Composition)" +
    /// "어느 포인트에서 몇 마리를(PointAllocations)" 를 코드 수정 없이 이 에셋만으로 정의한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Wave/Wave Definition", fileName = "Wave_")]
    public class WaveDefinition : ScriptableObject
    {
        [Tooltip("이 라운드에 등장할 몹 종류와 그 가중치. 스폰 시점마다 이 중 하나를 가중치 랜덤으로 고른다.")]
        [SerializeField] private EnemySpawnEntry[] _composition;

        [Tooltip("스폰 포인트(태그)별 물량 배분. 같은 라운드라도 포인트마다 다른 수/간격을 줄 수 있다.")]
        [SerializeField] private WaveSpawnPointAllocation[] _pointAllocations;

        [Tooltip("웨이브 시작을 알리는 텔레그래프 연출(하늘색 변화, SFX 등) 이후 실제 스폰까지의 대기 시간.")]
        [SerializeField, Min(0f)] private float _telegraphLeadTime = 5f;

        public IReadOnlyList<EnemySpawnEntry> Composition => _composition;
        public IReadOnlyList<WaveSpawnPointAllocation> PointAllocations => _pointAllocations;
        public float TelegraphLeadTime => _telegraphLeadTime;
    }
}
