namespace Shelter.Enemies
{
    /// <summary>
    /// 좀비 상태 머신. 기획 문서(몹.md) 기준 피격은 상태를 끊지 않고 애니메이션 리액션으로만 처리하므로
    /// 별도의 Staggered 상태는 두지 않는다.
    /// </summary>
    public enum EnemyState
    {
        Spawning,
        Seeking,
        Attacking,
        Dead
    }
}
