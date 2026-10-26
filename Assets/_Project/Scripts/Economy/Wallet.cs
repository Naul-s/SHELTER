using System;

namespace Shelter.Economy
{
    /// <summary>
    /// 돈 잔액(순수 로직). 잔액은 공유 상태(치팅 1순위)라 네트워킹 도입 시 서버만 Earn/TrySpend 를 호출한다.
    /// 클라이언트는 금액을 보내지 않는다 — 상점은 품목 ID만 받고 가격은 카탈로그에서 찾는다.
    /// </summary>
    public sealed class Wallet
    {
        public int Balance { get; private set; }

        /// <summary>잔액이 바뀐 직후. 인자 = 새 잔액.</summary>
        public event Action<int> BalanceChanged;

        public Wallet(int startingBalance)
        {
            Balance = Math.Max(0, startingBalance);
        }

        public void Earn(int amount)
        {
            if (amount <= 0) return;

            Balance += amount;
            BalanceChanged?.Invoke(Balance);
        }

        /// <summary>잔액이 충분하면 차감하고 true. 부족하면 상태를 바꾸지 않고 false.</summary>
        public bool TrySpend(int cost)
        {
            if (cost < 0 || cost > Balance) return false;
            if (cost == 0) return true;

            Balance -= cost;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
