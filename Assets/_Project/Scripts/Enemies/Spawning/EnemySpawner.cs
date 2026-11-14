using System.Collections.Generic;
using Shelter.Core;
using Shelter.Difficulty;
using Shelter.Enemies;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Spawning
{
    /// <summary>
    /// (프리팹, EnemyData, 가중치) 항목을 받아 좀비를 풀에서 꺼내 생성/회수하는 기본 스포너.
    /// 라운드별 구성·물량은 WaveManager가 이 스포너를 호출해 결정한다 — 이 클래스는 "무엇을 어디서 즉시 스폰할지"만 책임진다.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Tooltip("WaveManager 없이 SpawnRandom/SpawnWave로 테스트할 때만 쓰는 스폰 위치.")]
        [SerializeField] private Transform[] _spawnPoints;
        [Tooltip("WaveManager 없이 테스트할 때만 쓰는 스폰 항목.")]
        [SerializeField] private EnemySpawnEntry[] _entries;
        [SerializeField] private Transform _poolParent;

        private readonly Dictionary<EnemyBase, EnemyPool> _poolsByPrefab = new Dictionary<EnemyBase, EnemyPool>();
        private readonly Dictionary<EnemyBase, EnemyPool> _poolByActiveInstance = new Dictionary<EnemyBase, EnemyPool>();
        private readonly HashSet<EnemyBase> _alive = new HashSet<EnemyBase>();

        /// <summary>스폰됐고 아직 죽지 않은 좀비 수(시체는 제외).</summary>
        public int AliveCount => _alive.Count;

        /// <summary>좀비 한 마리가 죽어 AliveCount가 줄어든 직후.</summary>
        public event System.Action<EnemyBase> EnemyKilled;

        /// <summary>항목 하나를 point 위치에 스폰한다. targetBarricade가 없으면 좀비가 가장 가까운 바리케이드로 간다.</summary>
        public EnemyBase SpawnEntry(EnemySpawnEntry entry, Transform point, Barricade targetBarricade = null)
        {
            if (entry == null || entry.Prefab == null || entry.Data == null || point == null) return null;

            if (!_poolsByPrefab.TryGetValue(entry.Prefab, out EnemyPool pool))
            {
                pool = new EnemyPool(entry.Prefab, _poolParent != null ? _poolParent : transform);
                _poolsByPrefab.Add(entry.Prefab, pool);
            }

            EnemyBase instance = pool.Get();
            _poolByActiveInstance[instance] = pool;
            instance.DespawnRequested += HandleDespawnRequested;
            instance.EnemyDied += HandleEnemyDied;
            instance.Spawn(entry.Data, point.position, point.rotation, targetBarricade,
                DifficultySelection.Modifiers.EnemyHealthMultiplier);
            _alive.Add(instance);
            return instance;
        }

        public EnemyBase SpawnRandom()
        {
            if (_entries == null || _entries.Length == 0 || _spawnPoints == null || _spawnPoints.Length == 0)
                return null;

            EnemySpawnEntry entry = PickWeightedEntry();
            Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            return SpawnEntry(entry, point);
        }

        public void SpawnWave(int count)
        {
            for (int i = 0; i < count; i++)
                SpawnRandom();
        }

        private EnemySpawnEntry PickWeightedEntry()
        {
            return WeightedRandomPicker.Pick(_entries, e => e.Weight);
        }

        private void HandleEnemyDied(EnemyBase instance)
        {
            if (_alive.Remove(instance))
                EnemyKilled?.Invoke(instance);
        }

        private void HandleDespawnRequested(EnemyBase instance)
        {
            instance.DespawnRequested -= HandleDespawnRequested;
            instance.EnemyDied -= HandleEnemyDied;
            _alive.Remove(instance);

            if (_poolByActiveInstance.TryGetValue(instance, out EnemyPool pool))
            {
                _poolByActiveInstance.Remove(instance);
                pool.Release(instance);
            }
        }
    }
}
