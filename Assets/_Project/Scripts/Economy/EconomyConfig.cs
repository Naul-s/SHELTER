using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>경제 밸런스 수치. 처치 보상은 좀비 종류별로 EnemyData.KillReward 에 있다. 플레이테스트 후 조정 대상.</summary>
    [CreateAssetMenu(menuName = "Shelter/Economy/Economy Config", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Tooltip("게임 시작 시 플레이어 한 명이 가진 돈 — 첫 준비 시간에 가장 싼 총을 살 수 있을 만큼.")]
        [SerializeField, Min(0)] private int _startingMoney = 500;

        public int StartingMoney => _startingMoney;
    }
}
