namespace Shelter.Enemies
{
    /// <summary>
    /// 좀비 상태 머신. 피격은 상태를 끊지 않고 애니메이션 리액션으로만 처리하므로 별도의 Staggered 상태는 두지 않는다.
    /// 흐름: Seeking(바리케이드로 이동) → Attacking(판자 뜯기) → Entering(넘어 들어옴) → Seeking/Attacking(플레이어) → Dead.
    /// </summary>
    public enum EnemyState
    {
        Spawning,
        /// <summary>타겟(바리케이드 자리·진입 지점·플레이어)으로 이동 중.</summary>
        Seeking,
        /// <summary>판자를 뜯거나 플레이어를 공격 중.</summary>
        Attacking,
        /// <summary>판자가 다 뜯긴 진입 지점을 통과 중(출입구는 걸어서, 창문은 뛰어넘어). NavMesh 이동이 잠긴다.</summary>
        Entering,
        Dead
    }
}
