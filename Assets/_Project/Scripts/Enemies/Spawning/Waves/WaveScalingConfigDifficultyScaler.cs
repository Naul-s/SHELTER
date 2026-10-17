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

        public int ScaleCount(int baseCount, int roundsBeyondAuthored, int activePlayerCount)
        {
            if (_config == null || baseCount <= 0) return Mathf.Max(0, baseCount);

            float multiplier = _config.Evaluate(roundsBeyondAuthored, activePlayerCount);
            // 제작자가 1마리 이상 배정한 포인트가 인원 보정으로 0마리가 되지는 않게 한다.
            return Mathf.Max(1, Mathf.RoundToInt(baseCount * multiplier));
        }
    }
}
