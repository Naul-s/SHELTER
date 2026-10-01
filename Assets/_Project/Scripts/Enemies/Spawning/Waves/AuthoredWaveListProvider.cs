using System.Collections.Generic;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 라운드 1..N에 대해 디자이너가 직접 만든 WaveDefinition을 순서대로 사용한다.
    /// 제작된 목록보다 라운드 번호가 커지면(엔드리스 구간) 마지막 웨이브를 템플릿으로 재사용하고,
    /// 실제 강도 증가는 IWaveDifficultyScaler(WaveScalingConfig)가 맡는다.
    /// </summary>
    public sealed class AuthoredWaveListProvider : IWaveProvider
    {
        private readonly IReadOnlyList<WaveDefinition> _waves;

        public AuthoredWaveListProvider(IReadOnlyList<WaveDefinition> waves)
        {
            _waves = waves;
        }

        public WaveDefinition GetWaveDefinition(int roundNumber)
        {
            if (_waves == null || _waves.Count == 0) return null;

            int index = Mathf.Clamp(roundNumber - 1, 0, _waves.Count - 1);
            return _waves[index];
        }
    }
}
