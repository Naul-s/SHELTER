using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>경제 밸런스 수치. 처치 보상은 좀비 종류별로 EnemyData.KillReward 에 있다. 플레이테스트 후 조정 대상.</summary>
    [CreateAssetMenu(menuName = "Shelter/Economy/Economy Config", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Tooltip("게임 시작 시 플레이어 한 명이 가진 돈 — 첫 준비 시간에 가장 싼 총을 살 수 있을 만큼.")]
        [SerializeField, Min(0)] private int _startingMoney = 500;

        [Header("회복 (구급상자) — 비용·회복량은 기획 미확정, 임시값")]
        [Tooltip("구급상자 1회 사용 비용.")]
        [SerializeField, Min(0)] private int _healCost = 500;
        [Tooltip("1회 회복량(최대 체력 대비 비율). 1 = 가득.")]
        [SerializeField, Range(0.05f, 1f)] private float _healFraction = 1f;

        public int StartingMoney => _startingMoney;
        public int HealCost => _healCost;
        public float HealFraction => _healFraction;
    }
}
