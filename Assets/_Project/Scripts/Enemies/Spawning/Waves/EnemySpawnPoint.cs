using System.Collections.Generic;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 씬에 실제로 배치되는 스폰 위치. 같은 SpawnPointTag를 여러 EnemySpawnPoint에 붙이면
    /// "이 방향" 하나에 여러 물리적 출현 지점을 둘 수 있다(웨이브가 그 중 무작위로 고른다).
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        /// <summary>Update 루프에서 FindObjectOfType 없이 태그별 스폰 지점을 찾기 위한 registry.</summary>
        internal static readonly List<EnemySpawnPoint> ActiveInstances = new List<EnemySpawnPoint>();

        [SerializeField] private SpawnPointTag _pointTag;

        public SpawnPointTag Tag => _pointTag;

        private void OnEnable()
        {
            ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
