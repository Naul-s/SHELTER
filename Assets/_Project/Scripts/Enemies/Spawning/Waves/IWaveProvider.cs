namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>주어진 라운드 번호에 해당하는 스폰 계획을 돌려주는 전략. 명시 제작 목록/절차적 생성 등으로 교체 가능.</summary>
    public interface IWaveProvider
    {
        WaveDefinition GetWaveDefinition(int roundNumber);
    }
}
