using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 명시적으로 제작된 WaveDefinition이 바닥났을 때(라운드 번호가 목록보다 많을 때) 마지막 웨이브를
    /// 우려먹는 대신 자동으로 더 어렵게 만드는 배율 공식. wave-spawning 스킬의 제안 공식
    /// (기본 수 × Day 배율 × 인원 보정)을 전부 이 에셋의 값으로 노출해 코드에는 상수가 없다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Wave/Wave Scaling Config", fileName = "WaveScalingConfig")]
    public class WaveScalingConfig : ScriptableObject
    {
        [Tooltip("가로축 = 라운드(Day) 번호, 세로축 = 배율. 기본 제안값: 1라운드 1.0배, Day당 +15%.")]
        [SerializeField] private AnimationCurve _roundMultiplierCurve =
            AnimationCurve.Linear(1f, 1f, 20f, 1f + 19 * 0.15f);

        [Tooltip("인원 보정계수 = 이 값 + 접속 인원 × PerPlayerCoefficient. 제안값 0.7 (1인=0.85, 4인=1.3).")]
        [SerializeField] private float _playerCountBaseCoefficient = 0.7f;

        [SerializeField] private float _playerCountPerPlayerCoefficient = 0.15f;

        public float Evaluate(int roundNumber, int activePlayerCount)
        {
            float roundMultiplier = _roundMultiplierCurve.Evaluate(roundNumber);
            float playerCoefficient = _playerCountBaseCoefficient + activePlayerCount * _playerCountPerPlayerCoefficient;
            return Mathf.Max(0f, roundMultiplier * playerCoefficient);
        }
    }
}
