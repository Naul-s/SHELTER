using System;
using System.Collections;
using Shelter.Enemies;
using Shelter.Enemies.Spawning;
using Shelter.Enemies.Spawning.Waves;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Rounds
{
    /// <summary>
    /// 1~MaxRounds(기본 10) 라운드의 진행을 관리한다: 준비 시간 → WaveManager로 웨이브 스폰 →
    /// 스폰이 끝나고 좀비가 전멸하면 라운드 클리어 → 준비 시간 → 다음 라운드, 마지막 라운드 클리어 시 승리.
    /// 플레이어 전원이 죽으면(<see cref="PlayerParty.PartyWiped"/>) 진행 중이든 준비 시간이든 즉시 패배한다.
    /// 웨이브 구성/난이도는 WaveManager가, 수치는 RoundConfig가 책임지고 이 클래스는 흐름만 담당한다.
    ///
    /// 주의: 라운드 진행은 공유 상태라 networking-architecture상 서버 권위여야 하지만, WaveManager/EnemyBase와
    /// 동일한 이유로(Photon Fusion 미도입) 로컬 권위로 구현했다. 네트워킹 도입 시 BeginGame/상태 전이 지점만 서버로 옮긴다.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] private RoundConfig _config;
        [SerializeField] private WaveManager _waveManager;
        [SerializeField] private bool _beginOnStart = true;

        private EnemySpawner _spawner;
        private bool _spawningCompleted;
        private Coroutine _intermissionRoutine;
        private float _intermissionEndTime;

        public RoundState State { get; private set; } = RoundState.Idle;
        public int CurrentRound { get; private set; }
        public int MaxRounds => _config != null ? _config.MaxRounds : 0;
        /// <summary>준비 시간 남은 초(준비 시간이 아니면 0). UI 카운트다운용 — 매 프레임 이벤트 대신 표시 쪽이 읽는다.</summary>
        public float IntermissionTimeRemaining =>
            State == RoundState.Intermission ? Mathf.Max(0f, _intermissionEndTime - Time.time) : 0f;

        /// <summary>(다음 라운드 번호, 준비 시간 초) — 준비 시간 시작 시점. UI 카운트다운용.</summary>
        public event Action<int, float> IntermissionStarted;
        public event Action<int> RoundStarted;
        public event Action<int> RoundCleared;
        public event Action AllRoundsCleared;
        /// <summary>플레이어 전원 사망으로 패배한 순간. 인자 = 패배한 라운드 번호.</summary>
        public event Action<int> Defeated;

        private void Start()
        {
            if (_config == null || _waveManager == null)
            {
                Debug.LogError($"{name}: RoundConfig 또는 WaveManager가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _spawner = _waveManager.Spawner;
            if (_spawner == null)
            {
                Debug.LogError($"{name}: WaveManager에 EnemySpawner가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _waveManager.RoundSpawningCompleted += HandleSpawningCompleted;
            _spawner.EnemyKilled += HandleEnemyKilled;
            PlayerParty.PartyWiped += HandlePartyWiped;

            if (_beginOnStart)
                BeginGame();
        }

        private void OnDestroy()
        {
            if (_waveManager != null) _waveManager.RoundSpawningCompleted -= HandleSpawningCompleted;
            if (_spawner != null) _spawner.EnemyKilled -= HandleEnemyKilled;
            PlayerParty.PartyWiped -= HandlePartyWiped;
        }

        /// <summary>게임을 시작한다(1라운드 준비 시간부터).</summary>
        public void BeginGame()
        {
            if (State != RoundState.Idle) return;

            CurrentRound = 0;
            BeginIntermission(_config.FirstRoundDelay);
        }

        /// <summary>패배로 전이한다 — 플레이어 전원 사망 시 호출. 이후 라운드·스폰을 더 진행하지 않는다.</summary>
        public void NotifyDefeat()
        {
            if (State == RoundState.Idle || State == RoundState.Victory || State == RoundState.Defeat) return;

            if (_intermissionRoutine != null)
            {
                StopCoroutine(_intermissionRoutine);
                _intermissionRoutine = null;
            }

            _waveManager.StopAllWaves();
            State = RoundState.Defeat;
            Debug.Log($"[Round] 패배 — {CurrentRound}라운드에서 플레이어 전원 사망", this);
            Defeated?.Invoke(CurrentRound);
        }

        private void HandlePartyWiped()
        {
            NotifyDefeat();
        }

        private void BeginIntermission(float seconds)
        {
            State = RoundState.Intermission;
            _intermissionEndTime = Time.time + seconds;
            Debug.Log($"[Round] 준비 시간 {seconds:0}초 — 다음 {CurrentRound + 1}라운드", this);
            IntermissionStarted?.Invoke(CurrentRound + 1, seconds);
            _intermissionRoutine = StartCoroutine(IntermissionThenStartRound(seconds));
        }

        private IEnumerator IntermissionThenStartRound(float seconds)
        {
            if (seconds > 0f)
                yield return new WaitForSeconds(seconds);

            _intermissionRoutine = null;
            StartNextRound();
        }

        /// <summary>준비 시간을 건너뛰고 바로 다음 라운드를 시작한다(디버그용).</summary>
        [ContextMenu("Skip Intermission")]
        public void SkipIntermission()
        {
            if (State != RoundState.Intermission) return;

            if (_intermissionRoutine != null)
            {
                StopCoroutine(_intermissionRoutine);
                _intermissionRoutine = null;
            }

            StartNextRound();
        }

        private void StartNextRound()
        {
            int next = CurrentRound + 1;
            _spawningCompleted = false;

            // 웨이브가 즉시 끝나는 경우 StartRound 안에서 완료 이벤트가 동기 발생하므로 상태를 먼저 맞춰 둔다.
            int previousRound = CurrentRound;
            CurrentRound = next;
            State = RoundState.InProgress;

            if (!_waveManager.StartRound(next))
            {
                Debug.LogError($"{name}: {next}라운드를 시작하지 못했습니다(WaveDefinition 누락?). 라운드 진행을 중단합니다.", this);
                CurrentRound = previousRound;
                State = RoundState.Idle;
                return;
            }

            Debug.Log($"[Round] {CurrentRound}라운드 시작", this);
            RoundStarted?.Invoke(CurrentRound);
        }

        private void HandleSpawningCompleted(int roundNumber)
        {
            if (State != RoundState.InProgress || roundNumber != CurrentRound) return;

            _spawningCompleted = true;
            TryCompleteRound();
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            TryCompleteRound();
        }

        private void TryCompleteRound()
        {
            if (State != RoundState.InProgress || !_spawningCompleted || _spawner.AliveCount > 0) return;

            Debug.Log($"[Round] {CurrentRound}라운드 클리어", this);
            RoundCleared?.Invoke(CurrentRound);

            if (CurrentRound >= _config.MaxRounds)
            {
                State = RoundState.Victory;
                Debug.Log("[Round] 승리 — 모든 라운드 클리어", this);
                AllRoundsCleared?.Invoke();
                return;
            }

            BeginIntermission(_config.IntermissionTime);
        }
    }
}
