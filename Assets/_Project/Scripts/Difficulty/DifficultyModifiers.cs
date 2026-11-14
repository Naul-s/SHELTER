using System;
using UnityEngine;

namespace Shelter.Difficulty
{
    /// <summary>
    /// 난이도 하나의 배율 묶음(순수 C# — EditMode 테스트 대상). 각 시스템은 자기 SO 수치에 이 배율만 곱한다.
    /// 수치는 <see cref="DifficultyConfig"/> 에셋에서 정한다 — 플레이테스트 후 조정 대상.
    /// </summary>
    [Serializable]
    public struct DifficultyModifiers
    {
        [Tooltip("좀비 최대 체력 배율.")]
        [SerializeField, Min(0.1f)] private float _enemyHealth;
        [Tooltip("라운드 총 스폰 수 배율(라운드 총원에 곱한 뒤 스폰 포인트들에 나눠 준다).")]
        [SerializeField, Min(0.1f)] private float _spawnCount;
        [Tooltip("동시 생존 상한(WaveDefinition.MaxAlive) 배율.")]
        [SerializeField, Min(0.1f)] private float _maxAlive;
        [Tooltip("처치 보상 배율.")]
        [SerializeField, Min(0f)] private float _killReward;

        public DifficultyModifiers(float enemyHealth, float spawnCount, float maxAlive, float killReward)
        {
            _enemyHealth = enemyHealth;
            _spawnCount = spawnCount;
            _maxAlive = maxAlive;
            _killReward = killReward;
        }

        /// <summary>배율 1.0 — 아무것도 바꾸지 않는다. 난이도를 고르지 않고 게임 씬을 바로 켰을 때의 값.</summary>
        public static DifficultyModifiers Identity => new DifficultyModifiers(1f, 1f, 1f, 1f);

        public float EnemyHealthMultiplier => _enemyHealth > 0f ? _enemyHealth : 1f;
        public float SpawnCountMultiplier => _spawnCount > 0f ? _spawnCount : 1f;
        public float MaxAliveMultiplier => _maxAlive > 0f ? _maxAlive : 1f;
        public float KillRewardMultiplier => _killReward >= 0f ? _killReward : 1f;

        public float ApplyEnemyHealth(float baseHealth) => baseHealth * EnemyHealthMultiplier;

        public int ApplyKillReward(int baseReward) => Mathf.Max(0, Mathf.RoundToInt(baseReward * KillRewardMultiplier));

        /// <summary>0 이하(상한 없음)는 그대로 둔다. 상한이 있으면 최소 1.</summary>
        public int ApplyMaxAlive(int baseMaxAlive)
        {
            if (baseMaxAlive <= 0) return baseMaxAlive;
            return Mathf.Max(1, Mathf.RoundToInt(baseMaxAlive * MaxAliveMultiplier));
        }

        /// <summary>
        /// 스폰 포인트별 수(counts)를 라운드 총원 기준으로 늘리거나 줄인다(제자리 수정).
        /// 포인트마다 따로 반올림하면 1×1.25 = 1 처럼 배율이 사라지므로, 총원 = round(합 × 배율)을 먼저 정하고
        /// 각 포인트에 내림 몫을 준 뒤 남는 수를 소수부가 큰 포인트부터 하나씩 더한다(최대 나머지 배분).
        /// 원래 0인 포인트는 0으로 둔다(그 방향은 이번 라운드에 열리지 않는다).
        /// </summary>
        public void ScaleSpawnCounts(int[] counts)
        {
            if (counts == null || counts.Length == 0) return;
            float multiplier = SpawnCountMultiplier;
            if (Mathf.Approximately(multiplier, 1f)) return;

            int baseTotal = 0;
            for (int i = 0; i < counts.Length; i++) baseTotal += Mathf.Max(0, counts[i]);
            if (baseTotal == 0) return;

            int target = Mathf.Max(1, Mathf.RoundToInt(baseTotal * multiplier));
            var remainders = new float[counts.Length];
            int assigned = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                float exact = Mathf.Max(0, counts[i]) * (float)target / baseTotal;
                int floor = Mathf.FloorToInt(exact);
                counts[i] = floor;
                remainders[i] = counts[i] > 0 || exact > 0f ? exact - floor : -1f;
                assigned += floor;
            }

            for (int left = target - assigned; left > 0; left--)
            {
                int best = -1;
                for (int i = 0; i < counts.Length; i++)
                    if (remainders[i] >= 0f && (best < 0 || remainders[i] > remainders[best])) best = i;
                if (best < 0) break;
                counts[best]++;
                remainders[best] = -1f; // 한 포인트에 두 번 주지 않는다(남는 수 < 포인트 수)
            }
        }
    }
}
