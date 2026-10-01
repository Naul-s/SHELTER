using System;
using UnityEngine;

namespace Shelter.Gathering
{
    /// <summary>스폰 범위 하나가 만들어낼 자원 한 종류의 설정. 인스펙터에서 범위마다 다르게 조절한다.</summary>
    [Serializable]
    public class ResourceSpawnEntry
    {
        [SerializeField] private ResourceNodeData _data;
        [Tooltip("초기 생성 시 이 자원이 뽑힐 상대 확률.")]
        [SerializeField, Min(0f)] private float _weight = 1f;
        [Tooltip("이 범위 안에 동시에 존재할 수 있는 이 자원의 최대 개수(재생성 대기 중인 것 포함).")]
        [SerializeField, Min(0)] private int _maxCount = 5;

        [Header("재생성 시간 덮어쓰기")]
        [Tooltip("켜면 ResourceNodeData의 기본 재생성 시간 대신 아래 값을 쓴다.")]
        [SerializeField] private bool _overrideRespawnDelay;
        [SerializeField, Min(0f)] private float _respawnDelayMinMinutes = 3f;
        [SerializeField, Min(0f)] private float _respawnDelayMaxMinutes = 5f;

        public ResourceNodeData Data => _data;
        public float Weight => _weight;
        public int MaxCount => _maxCount;
        public bool IsValid => _data != null && _data.NodePrefab != null;

        /// <summary>고갈 후 재생성까지 기다릴 시간(초)을 최소~최대 사이에서 무작위로 뽑는다.</summary>
        public float RollRespawnDelaySeconds()
        {
            float min = _overrideRespawnDelay ? _respawnDelayMinMinutes : _data.RespawnDelayMinMinutes;
            float max = _overrideRespawnDelay ? _respawnDelayMaxMinutes : _data.RespawnDelayMaxMinutes;
            if (max < min) max = min;
            return UnityEngine.Random.Range(min, max) * 60f;
        }

        public void OnValidate()
        {
            if (_respawnDelayMaxMinutes < _respawnDelayMinMinutes)
                _respawnDelayMaxMinutes = _respawnDelayMinMinutes;
        }
    }
}
