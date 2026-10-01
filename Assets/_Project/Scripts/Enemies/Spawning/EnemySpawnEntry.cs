using System;
using Shelter.Enemies;
using UnityEngine;

namespace Shelter.Enemies.Spawning
{
    /// <summary>스폰할 프리팹과, 그 프리팹에 적용할 스탯 SO의 짝. 같은 비주얼을 다른 EnemyData로 재사용할 수 있다.</summary>
    [Serializable]
    public sealed class EnemySpawnEntry
    {
        [SerializeField] private EnemyBase _prefab;
        [SerializeField] private EnemyData _data;
        [SerializeField, Min(0f)] private float _weight = 1f;

        public EnemyBase Prefab => _prefab;
        public EnemyData Data => _data;
        public float Weight => _weight;
    }
}
