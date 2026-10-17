using System;

namespace Shelter.Player
{
    /// <summary>
    /// 플레이어 전원의 생사 집계. 패배 조건(Notion 확정) = 플레이어 전원 사망 — 한 명이 죽을 때마다 전원 사망인지 확인해
    /// <see cref="PartyWiped"/>를 알린다. RoundManager가 구독해 패배로 전이한다.
    /// 플레이어 생사는 공유 상태라 네트워킹 도입 시 서버에서만 판정한다.
    /// </summary>
    public static class PlayerParty
    {
        /// <summary>마지막 생존 플레이어가 죽어 전원 사망이 된 순간.</summary>
        public static event Action PartyWiped;

        /// <summary>등록된 플레이어가 한 명 이상이고 모두 죽었는가.</summary>
        public static bool IsWiped
        {
            get
            {
                var players = PlayerHealth.ActiveInstances;
                if (players.Count == 0) return false;

                for (int i = 0; i < players.Count; i++)
                    if (players[i] != null && !players[i].IsDead) return false;

                return true;
            }
        }

        internal static void NotifyPlayerDied(PlayerHealth player)
        {
            if (IsWiped) PartyWiped?.Invoke();
        }
    }
}
