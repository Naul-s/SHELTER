using Shelter.Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace Shelter.Enemies.Spawning
{
    /// <summary>
    /// 좀비/투사체/이펙트는 오브젝트 풀링이 필수(CLAUDE.md)라서, 프리팹 1종당 이 풀 1개를 둔다.
    /// Unity 내장 UnityEngine.Pool.ObjectPool을 감싸 EnemyBase 전용 get/release만 노출한다.
    /// </summary>
    public sealed class EnemyPool
    {
        private readonly IObjectPool<EnemyBase> _pool;
        private readonly EnemyBase _prefab;
        private readonly Transform _parent;

        public EnemyPool(EnemyBase prefab, Transform parent, int defaultCapacity = 16, int maxSize = 128)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<EnemyBase>(
                CreateInstance,
                actionOnGet: instance => instance.gameObject.SetActive(true),
                actionOnRelease: instance => instance.gameObject.SetActive(false),
                actionOnDestroy: instance => Object.Destroy(instance.gameObject),
                collectionCheck: false,
                defaultCapacity,
                maxSize);
        }

        private EnemyBase CreateInstance() => Object.Instantiate(_prefab, _parent);

        public EnemyBase Get() => _pool.Get();

        public void Release(EnemyBase instance)
        {
            instance.ResetForPool();
            _pool.Release(instance);
        }
    }
}
