namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>라운드/인원수에 따라 기본 스폰 수를 몇 마리로 늘릴지 결정하는 전략.</summary>
    public interface IWaveDifficultyScaler
    {
        int ScaleCount(int baseCount, int roundNumber, int activePlayerCount);
    }
}
