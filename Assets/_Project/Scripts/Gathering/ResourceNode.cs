using System;
using UnityEngine;

namespace Shelter.Gathering
{
    /// <summary>
    /// 맵에 놓이는 자원 노드. 잔량 차감은 Gather 한 곳에서만 일어난다 — 멀티플레이어 도입 시 이 호출을
    /// 서버에서만 실행하고 잔량을 동기화 변수로 바꾸면 된다(현재는 EnemyBase와 동일하게 로컬 권위).
    /// 고갈되면 Depleted 이벤트로 알리기만 하고, 회수/재생성은 소유한 ResourceSpawnArea가 결정한다.
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        public event Action<ResourceNode> Depleted;

        public ResourceNodeData Data { get; private set; }
        public int RemainingAmount { get; private set; }
        public bool IsDepleted => Data == null || RemainingAmount <= 0;

        public void Initialize(ResourceNodeData data)
        {
            Data = data;
            RemainingAmount = data != null ? data.TotalAmount : 0;
        }

        /// <summary>
        /// 채집을 시도하고 실제로 지급할 양을 반환한다(잔량보다 많이 줄 수 없음). 잔량이 0이 되면 Depleted를 발생시킨다.
        /// </summary>
        public int Gather(int hitCount = 1)
        {
            if (IsDepleted || hitCount <= 0) return 0;

            int amount = Mathf.Min(RemainingAmount, Data.AmountPerHit * hitCount);
            RemainingAmount -= amount;

            if (RemainingAmount <= 0)
                Depleted?.Invoke(this);

            return amount;
        }

        public void ResetForPool()
        {
            Depleted = null;
            Data = null;
            RemainingAmount = 0;
        }
    }
}
