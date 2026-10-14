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
    /// 라운드 단위 웨이브 오케스트레이터. "무엇을(Composition) 어디서 몇 마리(PointAllocations) 어떤 강도로
    /// (IWaveDifficultyScaler) 스폰할지"를 결정만 하고, 실제 개체 생성/풀링은 EnemySpawner에 위임한다
    /// (EnemySpawner 주석에 미리 적어둔 대로: "낮/밤 페이즈에 맞춘 웨이브 구성은 WaveManager가 이 스포너를
    /// 호출하는 형태로 얹으면 된다").
    ///
    /// day-night-cycle 스킬이 아직 구현되지 않아 이 클래스는 라운드 시작 트리거를 직접 만들지 않는다 —
    /// 밤이 시작될 때 DayNightCycleManager(또는 디버그 UI)가 StartRound(dayNumber)를 호출하면 된다.
    ///
    /// 주의: networking-architecture 스킬은 웨이브 스폰 결정을 서버 권위(NetworkBehaviour)로 두라고 하지만,
    /// EnemyBase와 동일한 이유로(Photon Fusion 미도입) 이 클래스도 순수 로컬 권위로 구현했다.
    /// 스폰 결정(라운드 해석/가중치 롤/카운트 산출)이 전부 이 파일 안에서만 일어나므로, 네트워킹 도입 시
    /// StartRound 진입점만 서버 RPC로 옮기면 된다.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _spawner;
        [Tooltip("라운드 1, 2, 3...에 순서대로 대응하는 웨이브. 목록보다 라운드가 커지면 마지막 웨이브를 스케일링해서 재사용한다.")]
        [SerializeField] private WaveDefinition[] _waveList;
        [Tooltip("제작된 웨이브 목록을 넘어선 라운드의 난이도 자동 증가 규칙. 비워두면 마지막 웨이브 수치를 그대로 반복한다.")]
        [SerializeField] private WaveScalingConfig _scalingConfig;

        [Header("테스트 전용")]
        [Tooltip("day-night-cycle이 아직 이 라운드 트리거를 연결하지 않은 테스트 씬에서, Play 시작과 동시에 지정한 라운드를 바로 실행한다. 실제 게임 씬에서는 꺼두고 DayNightCycleManager가 StartRound를 호출하게 할 것.")]
        [SerializeField] private bool _autoStartOnPlay;
        [SerializeField, Min(1)] private int _autoStartRoundNumber = 1;

        private IWaveProvider _waveProvider;
        private IWaveDifficultyScaler _difficultyScaler;

        public int CurrentRound { get; private set; }

        public EnemySpawner Spawner => _spawner;

        /// <summary>라운드 시작이 결정된 시점(텔레그래프 연출 시작 신호) — ui-audio 스킬이 하늘색/SFX 연출에 구독.</summary>
        public event Action<int> RoundTelegraphStarted;
        /// <summary>텔레그래프 대기가 끝나고 실제 스폰이 시작되는 시점.</summary>
        public event Action<int> RoundSpawningStarted;
        /// <summary>이번 라운드의 모든 스폰 포인트가 물량을 다 소진한 시점(생존한 좀비가 전멸했다는 뜻은 아님).</summary>
        public event Action<int> RoundSpawningCompleted;

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
            int activePlayerCount = PlayerHealth.ActiveInstances.Count;
            StartCoroutine(RunWave(wave, roundNumber, activePlayerCount));
            return true;
        }

        private IEnumerator RunWave(WaveDefinition wave, int roundNumber, int activePlayerCount)
        {
            RoundTelegraphStarted?.Invoke(roundNumber);

            if (wave.TelegraphLeadTime > 0f)
                yield return new WaitForSeconds(wave.TelegraphLeadTime);

            RoundSpawningStarted?.Invoke(roundNumber);

            IReadOnlyList<WaveSpawnPointAllocation> allocations = wave.PointAllocations;
            int pending = allocations?.Count ?? 0;
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
                StartCoroutine(RunPointAllocation(allocations[i], wave.Composition, roundNumber, activePlayerCount, onAllocationDone));
        }

        private IEnumerator RunPointAllocation(
            WaveSpawnPointAllocation allocation,
            IReadOnlyList<EnemySpawnEntry> composition,
            int roundNumber,
            int activePlayerCount,
            Action onComplete)
        {
            if (allocation.StartDelay > 0f)
                yield return new WaitForSeconds(allocation.StartDelay);

            int count = _difficultyScaler.ScaleCount(allocation.Count, roundNumber, activePlayerCount);
            List<EnemySpawnPoint> candidates = ResolveSpawnPoints(allocation.Point);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"{name}: '{(allocation.Point != null ? allocation.Point.DisplayName : "null")}' 태그의 스폰 포인트가 씬에 없습니다.", this);
            }
            else if (composition == null || composition.Count == 0)
            {
                Debug.LogWarning($"{name}: 웨이브에 스폰할 몹 구성(Composition)이 비어 있습니다.", this);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    EnemySpawnEntry entry = WeightedRandomPicker.Pick(composition, e => e.Weight);
                    EnemySpawnPoint point = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    _spawner.SpawnEntry(entry, point.transform);

                    if (i < count - 1)
                        yield return new WaitForSeconds(allocation.SpawnInterval);
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
