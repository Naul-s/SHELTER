using System;
using System.Collections;
using System.Collections.Generic;
using Shelter.Core;
using Shelter.Enemies.Spawning;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Enemies.Spawning.Waves
{
    /// <summary>
    /// 라운드 단위 웨이브 오케스트레이터. "무엇을(Composition) 어디서 몇 마리(PointAllocations) 동시에 최대 몇 마리까지(MaxAlive)
    /// 스폰할지"를 결정만 하고, 실제 개체 생성/풀링은 EnemySpawner에 위임한다. 라운드를 언제 시작할지는 RoundManager가 정한다.
    ///
    /// 주의: networking-architecture 스킬은 웨이브 스폰 결정을 서버 권위(NetworkBehaviour)로 두라고 하지만,
    /// EnemyBase와 동일한 이유로(Photon Fusion 미도입) 이 클래스도 순수 로컬 권위로 구현했다.
    /// 스폰 결정(라운드 해석/가중치 롤/카운트 산출)이 전부 이 파일 안에서만 일어나므로, 네트워킹 도입 시
    /// StartRound 진입점만 서버로 옮기면 된다.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _spawner;
        [Tooltip("라운드 1, 2, 3...에 순서대로 대응하는 웨이브. 목록보다 라운드가 커지면 마지막 웨이브를 스케일링해서 재사용한다.")]
        [SerializeField] private WaveDefinition[] _waveList;
        [Tooltip("인원 보정 + 제작 웨이브를 넘어선 라운드의 물량 증가 규칙. 비워두면 제작 수치를 그대로 쓴다.")]
        [SerializeField] private WaveScalingConfig _scalingConfig;

        [Header("테스트 전용")]
        [Tooltip("RoundManager 없이 웨이브만 확인하는 테스트 씬에서, Play 시작과 동시에 지정한 라운드를 바로 실행한다. 실제 게임 씬에서는 꺼두고 RoundManager가 StartRound를 호출하게 할 것.")]
        [SerializeField] private bool _autoStartOnPlay;
        [SerializeField, Min(1)] private int _autoStartRoundNumber = 1;

        private IWaveProvider _waveProvider;
        private IWaveDifficultyScaler _difficultyScaler;

        public int CurrentRound { get; private set; }

        public EnemySpawner Spawner => _spawner;

        /// <summary>이번 라운드에서 아직 스폰되지 않은 좀비 수. 남은 좀비 = Spawner.AliveCount + 이 값.</summary>
        public int RemainingToSpawn { get; private set; }

        /// <summary>라운드 시작이 결정된 시점(텔레그래프 연출 시작 신호) — ui-audio 연출이 구독한다.</summary>
        public event Action<int> RoundTelegraphStarted;
        /// <summary>텔레그래프 대기가 끝나고 실제 스폰이 시작되는 시점.</summary>
        public event Action<int> RoundSpawningStarted;
        /// <summary>이번 라운드의 모든 스폰 포인트가 물량을 다 소진한 시점(생존한 좀비가 전멸했다는 뜻은 아님).</summary>
        public event Action<int> RoundSpawningCompleted;
        /// <summary>좀비 한 마리가 스폰돼 <see cref="RemainingToSpawn"/>이 줄어든 직후 — 남은 좀비 표시 갱신용.</summary>
        public event Action EnemySpawned;

        private void Awake()
        {
            _waveProvider = new AuthoredWaveListProvider(_waveList);
            _difficultyScaler = new WaveScalingConfigDifficultyScaler(_scalingConfig);
        }

        private void Start()
        {
            if (_autoStartOnPlay)
                StartRound(_autoStartRoundNumber);
        }

        /// <summary>내부 라운드 카운터를 1 증가시켜 다음 라운드를 시작한다(디버그/단독 테스트용).</summary>
        [ContextMenu("Start Next Wave")]
        public void StartNextWave()
        {
            StartRound(CurrentRound + 1);
        }

        /// <summary>절대 라운드 번호로 웨이브를 시작한다. 해당 라운드의 WaveDefinition/스포너가 없어 시작하지 못하면 false.</summary>
        public bool StartRound(int roundNumber)
        {
            WaveDefinition wave = _waveProvider.GetWaveDefinition(roundNumber);
            if (wave == null)
            {
                Debug.LogWarning($"{name}: round {roundNumber}에 대응하는 WaveDefinition이 없습니다.", this);
                return false;
            }

            if (_spawner == null)
            {
                Debug.LogError($"{name}: EnemySpawner가 연결되지 않았습니다.", this);
                return false;
            }

            CurrentRound = roundNumber;
            int activePlayerCount = Mathf.Max(1, PlayerHealth.ActiveInstances.Count);
            int roundsBeyondAuthored = Mathf.Max(0, roundNumber - (_waveList != null ? _waveList.Length : 0));
            StartCoroutine(RunWave(wave, roundNumber, roundsBeyondAuthored, activePlayerCount));
            return true;
        }

        /// <summary>진행 중인 모든 웨이브 스폰을 중단한다(패배 등). 이미 나온 좀비는 그대로 둔다.</summary>
        public void StopAllWaves()
        {
            StopAllCoroutines();
            RemainingToSpawn = 0;
        }

        private IEnumerator RunWave(WaveDefinition wave, int roundNumber, int roundsBeyondAuthored, int activePlayerCount)
        {
            RoundTelegraphStarted?.Invoke(roundNumber);

            if (wave.TelegraphLeadTime > 0f)
                yield return new WaitForSeconds(wave.TelegraphLeadTime);

            IReadOnlyList<WaveSpawnPointAllocation> allocations = wave.PointAllocations;
            int pending = allocations?.Count ?? 0;

            RemainingToSpawn = 0;
            for (int i = 0; i < pending; i++)
                RemainingToSpawn += _difficultyScaler.ScaleCount(allocations[i].Count, roundsBeyondAuthored, activePlayerCount);

            RoundSpawningStarted?.Invoke(roundNumber);

            if (pending == 0)
            {
                RoundSpawningCompleted?.Invoke(roundNumber);
                yield break;
            }

            Action onAllocationDone = () =>
            {
                pending--;
                if (pending == 0) RoundSpawningCompleted?.Invoke(roundNumber);
            };

            for (int i = 0; i < allocations.Count; i++)
                StartCoroutine(RunPointAllocation(allocations[i], wave, roundsBeyondAuthored, activePlayerCount, onAllocationDone));
        }

        private IEnumerator RunPointAllocation(
            WaveSpawnPointAllocation allocation,
            WaveDefinition wave,
            int roundsBeyondAuthored,
            int activePlayerCount,
            Action onComplete)
        {
            if (allocation.StartDelay > 0f)
                yield return new WaitForSeconds(allocation.StartDelay);

            IReadOnlyList<EnemySpawnEntry> composition = wave.Composition;
            int count = _difficultyScaler.ScaleCount(allocation.Count, roundsBeyondAuthored, activePlayerCount);
            List<EnemySpawnPoint> candidates = ResolveSpawnPoints(allocation.Point);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"{name}: '{(allocation.Point != null ? allocation.Point.DisplayName : "null")}' 태그의 스폰 포인트가 씬에 없습니다.", this);
                RemainingToSpawn = Mathf.Max(0, RemainingToSpawn - count);
                EnemySpawned?.Invoke();
            }
            else if (composition == null || composition.Count == 0)
            {
                Debug.LogWarning($"{name}: 웨이브에 스폰할 좀비 구성(Composition)이 비어 있습니다.", this);
                RemainingToSpawn = Mathf.Max(0, RemainingToSpawn - count);
                EnemySpawned?.Invoke();
            }
            else
            {
                var interval = new WaitForSeconds(allocation.SpawnInterval);
                for (int i = 0; i < count; i++)
                {
                    // 동시 생존 상한: 자리가 날 때까지 이 포인트의 남은 물량을 대기시킨다.
                    while (wave.MaxAlive > 0 && _spawner.AliveCount >= wave.MaxAlive)
                        yield return null;

                    EnemySpawnEntry entry = WeightedRandomPicker.Pick(composition, e => e.Weight);
                    EnemySpawnPoint point = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    _spawner.SpawnEntry(entry, point.transform, point.PickTargetBarricade());
                    RemainingToSpawn = Mathf.Max(0, RemainingToSpawn - 1);
                    EnemySpawned?.Invoke();

                    if (i < count - 1)
                        yield return interval;
                }
            }

            onComplete?.Invoke();
        }

        private static List<EnemySpawnPoint> ResolveSpawnPoints(SpawnPointTag tag)
        {
            var matches = new List<EnemySpawnPoint>();
            for (int i = 0; i < EnemySpawnPoint.ActiveInstances.Count; i++)
            {
                if (EnemySpawnPoint.ActiveInstances[i].Tag == tag)
                    matches.Add(EnemySpawnPoint.ActiveInstances[i]);
            }
            return matches;
        }
    }
}
