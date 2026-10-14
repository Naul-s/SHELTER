using UnityEngine;

namespace Shelter.Rounds
{
    /// <summary>라운드 진행 규칙의 밸런싱 수치. 코드에 하드코딩하지 않고 이 에셋으로만 조정한다.</summary>
    [CreateAssetMenu(menuName = "Shelter/Round/Round Config", fileName = "RoundConfig")]
    public class RoundConfig : ScriptableObject
    {
        [Tooltip("이 라운드를 모두 막아내면 게임 승리. 기획 기준 10라운드.")]
        [SerializeField, Min(1)] private int _maxRounds = 10;

        [Tooltip("게임 시작 후 1라운드가 시작되기까지의 준비 시간(초).")]
        [SerializeField, Min(0f)] private float _firstRoundDelay = 10f;

        [Tooltip("라운드를 클리어한 뒤 다음 라운드가 시작되기까지의 준비 시간(초).")]
        [SerializeField, Min(0f)] private float _intermissionTime = 15f;

        public int MaxRounds => _maxRounds;
        public float FirstRoundDelay => _firstRoundDelay;
        public float IntermissionTime => _intermissionTime;
    }
}
