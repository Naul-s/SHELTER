using System.Collections.Generic;
using Shelter.Enemies.Spawning;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 한 라운드의 스폰 계획 전체. "어떤 좀비들을 어떤 비율로(Composition)" +
    /// "어느 스폰 포인트에서 몇 마리를(PointAllocations)" + "동시에 최대 몇 마리까지(MaxAlive)"를 코드 수정 없이 이 에셋만으로 정의한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Wave/Wave Definition", fileName = "Wave_")]
    public class WaveDefinition : ScriptableObject
    {
        [Tooltip("이 라운드에 등장할 좀비 종류와 그 가중치. 스폰 시점마다 이 중 하나를 가중치 랜덤으로 고른다.")]
        [SerializeField] private EnemySpawnEntry[] _composition;

        [Tooltip("스폰 포인트(태그)별 물량 배분. 같은 라운드라도 포인트마다 다른 수/간격을 줄 수 있다.")]
        [SerializeField] private WaveSpawnPointAllocation[] _pointAllocations;

        [Tooltip("라운드 시작 알림(텔레그래프 연출) 이후 실제 스폰까지의 대기 시간(초).")]
        [SerializeField, Min(0f)] private float _telegraphLeadTime = 5f;

        [Tooltip("동시에 살아 있을 수 있는 좀비 수 상한(Quest 성능 예산). 상한에 닿으면 남은 물량은 자리가 날 때까지 대기한다. 0 = 무제한.")]
        [SerializeField, Min(0)] private int _maxAlive = 24;

        public IReadOnlyList<EnemySpawnEntry> Composition => _composition;
        public IReadOnlyList<WaveSpawnPointAllocation> PointAllocations => _pointAllocations;
        public float TelegraphLeadTime => _telegraphLeadTime;
        public int MaxAlive => _maxAlive;
    }
}
