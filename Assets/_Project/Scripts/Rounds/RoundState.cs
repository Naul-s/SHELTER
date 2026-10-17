namespace Shelter.Rounds
{
    public enum RoundState
    {
        /// <summary>게임 시작 전.</summary>
        Idle,
        /// <summary>다음 라운드 시작 전 준비 시간.</summary>
        Intermission,
        /// <summary>라운드 진행 중(스폰 중이거나 좀비가 남아 있음).</summary>
        InProgress,
        /// <summary>모든 라운드 클리어.</summary>
        Victory,
        /// <summary>플레이어 전원 사망.</summary>
        Defeat
    }
}
