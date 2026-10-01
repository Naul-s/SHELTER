using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>WaveScalingConfig 에셋 값으로 스폰 수를 배율 적용한다. 설정이 없으면 원본 수를 그대로 쓴다(=스케일링 미적용).</summary>
    public sealed class WaveScalingConfigDifficultyScaler : IWaveDifficultyScaler
    {
        private readonly WaveScalingConfig _config;

        public WaveScalingConfigDifficultyScaler(WaveScalingConfig config)
        {
            _config = config;
        }

        public int ScaleCount(int baseCount, int roundNumber, int activePlayerCount)
        {
            if (_config == null) return baseCount;

            float multiplier = _config.Evaluate(roundNumber, activePlayerCount);
            return Mathf.Max(0, Mathf.RoundToInt(baseCount * multiplier));
        }
    }
}
