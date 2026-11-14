using UnityEngine;

namespace Shelter.Difficulty
{
    /// <summary>
    /// 이번 세션의 난이도. 시작 화면에서 고르고(<see cref="Select"/>) 게임 씬의 시스템이 읽는다 — 씬을 다시 불러와도 유지된다.
    /// 세션 설정이라 게임 진행 중에는 바꾸지 않는다. 고르지 않고 게임 씬을 바로 켜면 Normal(배율 1.0).
    /// 네트워킹 도입 시: 호스트가 고른 값을 [Networked] 세션 값으로 옮기고, 배율 적용(스폰·좀비 HP·보상)은 서버에서만 한다.
    /// </summary>
    public static class DifficultySelection
    {
        public static DifficultyLevel Level { get; private set; } = DifficultyLevel.Normal;
        public static DifficultyModifiers Modifiers { get; private set; } = DifficultyModifiers.Identity;

        public static void Select(DifficultyLevel level, DifficultyConfig config)
        {
            Level = level;
            Modifiers = config != null ? config.Get(level) : DifficultyModifiers.Identity;
        }

        // 에디터에서 도메인 리로드 없이 플레이를 반복해도 이전 선택이 남지 않게 한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Level = DifficultyLevel.Normal;
            Modifiers = DifficultyModifiers.Identity;
        }
    }
}
