using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 웨이브 물량 보정 공식. 배율 = 라운드 커브(제작 웨이브를 넘어선 라운드 수) × (기본 계수 + 접속 인원 × 인원당 계수).
    /// 제작된 라운드(1~N)에는 인원 보정만 적용되고(커브 x=1 → 1.0), 라운드가 제작 목록을 넘으면 커브가 물량을 키운다.
    /// 모든 수치를 이 에셋으로 노출해 코드에는 상수가 없다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Wave/Wave Scaling Config", fileName = "WaveScalingConfig")]
    public class WaveScalingConfig : ScriptableObject
    {
        [Tooltip("가로축 = 1 + 제작 웨이브를 넘어선 라운드 수, 세로축 = 배율. x=1(제작 범위 안)은 1.0이어야 제작 수치가 그대로 쓰인다. 기본: 넘어선 라운드당 +15%.")]
        [SerializeField] private AnimationCurve _roundMultiplierCurve =
            AnimationCurve.Linear(1f, 1f, 20f, 1f + 19 * 0.15f);

        [Tooltip("인원 보정계수 = 이 값 + 접속 인원 × PerPlayerCoefficient. 제안값 0.7 (1인=0.85, 4인=1.3).")]
        [SerializeField] private float _playerCountBaseCoefficient = 0.7f;

        [SerializeField] private float _playerCountPerPlayerCoefficient = 0.15f;

        /// <param name="roundsBeyondAuthored">제작된 웨이브 목록을 넘어선 라운드 수(제작 범위 안이면 0).</param>
        public float Evaluate(int roundsBeyondAuthored, int activePlayerCount)
        {
            float roundMultiplier = _roundMultiplierCurve.Evaluate(1 + Mathf.Max(0, roundsBeyondAuthored));
            float playerCoefficient = _playerCountBaseCoefficient + activePlayerCount * _playerCountPerPlayerCoefficient;
            return Mathf.Max(0f, roundMultiplier * playerCoefficient);
        }
    }
}
