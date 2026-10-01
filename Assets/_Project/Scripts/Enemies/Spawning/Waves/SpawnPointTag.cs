using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 스폰 포인트의 "이름표". 문자열/enum 대신 에셋 참조로 스폰 위치를 식별해 오타로 인한
    /// 불일치를 막는다. 씬의 EnemySpawnPoint와 WaveDefinition의 WaveSpawnPointAllocation이
    /// 같은 태그 에셋을 참조해 서로를 연결한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Wave/Spawn Point Tag", fileName = "SpawnPointTag_")]
    public class SpawnPointTag : ScriptableObject
    {
        [SerializeField] private string _displayName;

        public string DisplayName => _displayName;
    }
}
