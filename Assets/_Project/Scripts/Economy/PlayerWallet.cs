using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>
    /// 플레이어 한 명의 지갑(개인 지갑 — 막타를 친 플레이어가 보상을 받는다, CoD 좀비 방식).
    /// 지갑 단위(개인/팀 공용)는 기획 미확정이라 개인으로 가정했다 — economy-shop 스킬 참고.
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        private static readonly List<PlayerWallet> s_activeInstances = new List<PlayerWallet>();

        /// <summary>Update 루프에서 FindObjectOfType 없이 지갑을 찾기 위한 registry.</summary>
        public static IReadOnlyList<PlayerWallet> ActiveInstances => s_activeInstances;

        /// <summary>
        /// 이 기기를 쓰는 플레이어의 지갑(상점 버튼·잔액 표시용). 네트워킹 도입 전에는 첫 번째로 등록된 지갑.
        /// 도입 후에는 로컬 플레이어(Input Authority)의 지갑으로 바꾼다.
        /// </summary>
        public static PlayerWallet Local => s_activeInstances.Count > 0 ? s_activeInstances[0] : null;

        /// <summary>지갑이 하나라도 새로 등록됐다(표시 UI가 늦게 생긴 지갑을 구독할 수 있게).</summary>
        public static event Action<PlayerWallet> Registered;

        [SerializeField] private EconomyConfig _config;

        private Wallet _wallet;

        public int Balance => _wallet.Balance;

        /// <summary>잔액이 바뀐 직후. 인자 = 새 잔액.</summary>
        public event Action<int> BalanceChanged;

        private void Awake()
        {
            if (_config == null) Debug.LogError($"{name}: EconomyConfig가 연결되지 않았습니다.", this);
            _wallet = new Wallet(_config != null ? _config.StartingMoney : 0);
            _wallet.BalanceChanged += balance => BalanceChanged?.Invoke(balance);
        }

        private void OnEnable()
        {
            s_activeInstances.Add(this);
            Registered?.Invoke(this);
        }

        private void OnDisable()
        {
            s_activeInstances.Remove(this);
        }

        /// <summary>처치 보상 등 지급. 상태 변경 지점 — 네트워킹 도입 시 서버에서만.</summary>
        public void Earn(int amount) => _wallet.Earn(amount);

        /// <summary>구매 차감. 상태 변경 지점 — 네트워킹 도입 시 서버에서만(가격은 서버가 카탈로그에서 찾는다).</summary>
        public bool TrySpend(int cost) => _wallet.TrySpend(cost);
    }
}
