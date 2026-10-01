using UnityEngine;
using UnityEngine.Pool;

namespace Shelter.Gathering
{
    /// <summary>
    /// 자원 노드도 고갈/재생성으로 자주 생성·파괴되므로 풀링한다(CLAUDE.md 원칙 4). 프리팹 1종당 풀 1개.
    /// </summary>
    public sealed class ResourceNodePool
    {
        private readonly IObjectPool<ResourceNode> _pool;
        private readonly ResourceNode _prefab;
        private readonly Transform _parent;

        public ResourceNodePool(ResourceNode prefab, Transform parent, int defaultCapacity = 8, int maxSize = 64)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<ResourceNode>(
                CreateInstance,
                actionOnGet: instance => instance.gameObject.SetActive(true),
                actionOnRelease: instance => instance.gameObject.SetActive(false),
                actionOnDestroy: instance => Object.Destroy(instance.gameObject),
                collectionCheck: false,
                defaultCapacity,
                maxSize);
        }

        private ResourceNode CreateInstance() => Object.Instantiate(_prefab, _parent);

        public ResourceNode Get() => _pool.Get();

        public void Release(ResourceNode instance)
        {
            instance.ResetForPool();
            _pool.Release(instance);
        }
    }
}
