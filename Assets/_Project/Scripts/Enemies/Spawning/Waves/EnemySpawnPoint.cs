using System.Collections.Generic;
using Shelter.ShelterDefense;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 씬에 실제로 배치되는 스폰 위치(쉘터 밖). 같은 SpawnPointTag를 여러 EnemySpawnPoint에 붙이면
    /// "이 방향" 하나에 여러 물리적 출현 지점을 둘 수 있다(웨이브가 그 중 무작위로 고른다).
    /// 여기서 나온 좀비는 <see cref="PickTargetBarricade"/>가 고른 바리케이드로 향한다(목록이 비면 가장 가까운 바리케이드).
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        /// <summary>Update 루프에서 FindObjectOfType 없이 태그별 스폰 지점을 찾기 위한 registry.</summary>
        internal static readonly List<EnemySpawnPoint> ActiveInstances = new List<EnemySpawnPoint>();

        [SerializeField] private SpawnPointTag _pointTag;
        [Tooltip("이 지점에서 나온 좀비가 공략할 바리케이드 후보. 좀비마다 무작위로 하나를 고른다. 비워 두면 스폰 위치에서 가장 가까운 바리케이드.")]
        [SerializeField] private Barricade[] _targetBarricades;

        public SpawnPointTag Tag => _pointTag;
        public IReadOnlyList<Barricade> TargetBarricades => _targetBarricades;

        /// <summary>후보 중 하나를 무작위로 고른다. 후보가 없으면 null(좀비가 가장 가까운 바리케이드로 감).</summary>
        public Barricade PickTargetBarricade()
        {
            if (_targetBarricades == null || _targetBarricades.Length == 0) return null;

            Barricade picked = _targetBarricades[Random.Range(0, _targetBarricades.Length)];
            return picked != null && picked.isActiveAndEnabled ? picked : null;
        }

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

            if (_targetBarricades == null) return;
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.6f);
            foreach (Barricade barricade in _targetBarricades)
                if (barricade != null) Gizmos.DrawLine(transform.position, barricade.transform.position);
        }
    }
}
