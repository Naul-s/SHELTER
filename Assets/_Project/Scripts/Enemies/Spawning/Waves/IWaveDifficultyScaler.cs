namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>제작 웨이브를 넘어선 라운드 수/인원수에 따라 기본 스폰 수를 몇 마리로 조정할지 결정하는 전략.</summary>
    public interface IWaveDifficultyScaler
    {
        /// <param name="roundsBeyondAuthored">제작된 웨이브 목록을 넘어선 라운드 수(제작 범위 안이면 0).</param>
        int ScaleCount(int baseCount, int roundsBeyondAuthored, int activePlayerCount);
    }
}
