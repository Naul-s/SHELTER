using UnityEngine;

namespace Shelter.Difficulty
{
    /// <summary>난이도별 배율. 에셋 하나(`DifficultyConfig.asset`)를 시작 화면이 들고 있다가 고른 난이도의 배율을 넘긴다.</summary>
    [CreateAssetMenu(menuName = "Shelter/Difficulty/Difficulty Config", fileName = "DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Tooltip("Normal — 기준 밸런스(배율 1.0 권장).")]
        [SerializeField] private DifficultyModifiers _normal = DifficultyModifiers.Identity;
        [Tooltip("Hard — 좀비가 더 단단하고(요청 탄 수 +1) 조금 더 많이 나온다. 임시값, 플레이테스트 후 조정 대상.")]
        [SerializeField] private DifficultyModifiers _hard = new DifficultyModifiers(1.25f, 1.25f, 1.2f, 0.85f);

        public DifficultyModifiers Get(DifficultyLevel level)
        {
            return level == DifficultyLevel.Hard ? _hard : _normal;
        }

        public DifficultyModifiers Normal
        {
            get => _normal;
            set => _normal = value;
        }
        public DifficultyModifiers Hard
        {
            get => _hard;
            set => _hard = value;
        }
    }
}
