using System;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 한 라운드 안에서 특정 스폰 포인트(태그)가 감당할 몫 — 몇 마리를, 얼마 간격으로, 얼마나 늦게 시작할지.
    /// 라운드마다 스폰 포인트별 물량이 달라지는 요구사항은 이 값이 WaveDefinition 안에서 여러 개 나열되는 것으로 표현한다.
    /// </summary>
    [Serializable]
    public sealed class WaveSpawnPointAllocation
    {
        [SerializeField] private SpawnPointTag _point;
        [SerializeField, Min(0)] private int _count;
        [SerializeField, Min(0.05f)] private float _spawnInterval = 1f;
        [SerializeField, Min(0f)] private float _startDelay;

        public SpawnPointTag Point => _point;
        public int Count => _count;
        public float SpawnInterval => _spawnInterval;
        public float StartDelay => _startDelay;
    }
}
