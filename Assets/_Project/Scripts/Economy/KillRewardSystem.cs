using Shelter.Enemies;
using Shelter.Enemies.Spawning;
using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>
    /// 좀비 처치 보상 지급. 스포너의 처치 이벤트를 구독해 막타를 친 플레이어(EnemyBase.LastAttacker)의 지갑에 EnemyData.KillReward 를 넣는다.
    /// 무기·좀비 코드는 지갑을 모른다(이벤트로 분리). 네트워킹 도입 시 서버에서만 동작해야 한다.
    /// </summary>
    public class KillRewardSystem : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _spawner;

        private void OnEnable()
        {
            if (_spawner == null)
            {
                Debug.LogError($"{name}: EnemySpawner가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _spawner.EnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            if (_spawner != null) _spawner.EnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            if (enemy.Data == null) return;

            PlayerWallet wallet = enemy.LastAttacker != null ? enemy.LastAttacker.GetComponentInParent<PlayerWallet>() : null;
            if (wallet == null) return; // 플레이어가 아닌 원인(낙사 등)으로 죽은 좀비는 보상 없음

            wallet.Earn(enemy.Data.KillReward);
        }
    }
}
