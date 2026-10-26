using Shelter.Enemies;
using Shelter.Enemies.Spawning;
using Shelter.Enemies.Spawning.Waves;
using Shelter.Rounds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shelter.UI
{
    /// <summary>
    /// 쉘터 벽에 걸린 월드 스페이스 라운드 표시(Nacht der Untoten 의 벽 낙서 느낌).
    /// 1~5라운드는 핏빛 집계 표시(획), 6라운드부터는 핏빛 숫자. 아래 줄에 준비 시간 카운트다운·남은 좀비·승패를 쓴다.
    /// 라운드 상태는 읽기만 한다 — RoundManager/WaveManager/EnemySpawner 이벤트를 구독하고, 카운트다운만 Update에서 초 단위로 갱신한다.
    /// </summary>
    public class RoundDisplay : MonoBehaviour
    {
        private const int TallyRounds = 5;

        [SerializeField] private RoundManager _roundManager;
        [SerializeField] private WaveManager _waveManager;

        [Header("표시 요소")]
        [Tooltip("1~5라운드 집계 표시 획(앞 4개 세로, 5번째 사선).")]
        [SerializeField] private Graphic[] _tallyStrokes;
        [SerializeField] private TMP_Text _roundNumber;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _timerText;

        [Header("색")]
        [SerializeField] private Color _bloodColor = new Color(0.55f, 0.02f, 0.02f, 1f);
        [SerializeField] private Color _flashColor = new Color(1f, 0.93f, 0.85f, 1f);
        [Tooltip("상태 문구·카운트다운 색 — 벽에 피로 쓴 글씨(짙은 핏빛).")]
        [SerializeField] private Color _textColor = new Color(0.62f, 0.04f, 0.03f, 1f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.08f, 0.05f, 1f);

        [Header("연출")]
        [Tooltip("라운드 시작 시 흰색 → 핏빛으로 번지는 시간(초).")]
        [SerializeField, Min(0.1f)] private float _startFlashTime = 2f;
        [Tooltip("라운드 클리어 후 숫자가 깜빡이는 시간(초).")]
        [SerializeField, Min(0.1f)] private float _clearFlashTime = 4f;
        [Tooltip("카운트다운이 이 초 이하면 경고색으로 맥동한다.")]
        [SerializeField, Min(0)] private int _warningSeconds = 10;

        private EnemySpawner _spawner;
        private float _flashElapsed = float.PositiveInfinity;
        private float _flashDuration;
        private bool _flashBlink;
        private int _shownSeconds = -1;
        private bool _awaitingHorde;

        private void Awake()
        {
            if (_roundManager == null || _waveManager == null)
            {
                Debug.LogError($"{name}: RoundManager 또는 WaveManager가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _spawner = _waveManager.Spawner;
        }

        private void OnEnable()
        {
            if (_roundManager == null || _waveManager == null) return;

            _roundManager.IntermissionStarted += HandleIntermissionStarted;
            _roundManager.RoundStarted += HandleRoundStarted;
            _roundManager.RoundCleared += HandleRoundCleared;
            _roundManager.AllRoundsCleared += HandleAllRoundsCleared;
            _roundManager.Defeated += HandleDefeated;
            _waveManager.RoundSpawningStarted += HandleSpawningStarted;
            _waveManager.EnemySpawned += RefreshRemaining;
            if (_spawner != null) _spawner.EnemyKilled += HandleEnemyKilled;

            ShowRoundMark(_roundManager.CurrentRound);
            SetStatus(string.Empty, _textColor);
            _timerText.text = string.Empty;
        }

        private void OnDisable()
        {
            if (_roundManager == null || _waveManager == null) return;

            _roundManager.IntermissionStarted -= HandleIntermissionStarted;
            _roundManager.RoundStarted -= HandleRoundStarted;
            _roundManager.RoundCleared -= HandleRoundCleared;
            _roundManager.AllRoundsCleared -= HandleAllRoundsCleared;
            _roundManager.Defeated -= HandleDefeated;
            _waveManager.RoundSpawningStarted -= HandleSpawningStarted;
            _waveManager.EnemySpawned -= RefreshRemaining;
            if (_spawner != null) _spawner.EnemyKilled -= HandleEnemyKilled;
        }

        private void Update()
        {
            TickFlash(Time.deltaTime);
            TickCountdown();
            TickHordeWarning();
        }

        // ───────────── 이벤트

        private void HandleIntermissionStarted(int nextRound, float seconds)
        {
            _awaitingHorde = false;
            _shownSeconds = -1;
            SetStatus(nextRound <= 1 ? "곧 놈들이 몰려온다" : "준비 시간", _textColor);
        }

        private void HandleRoundStarted(int round)
        {
            ShowRoundMark(round);
            StartFlash(_startFlashTime, blink: false);
            _timerText.text = string.Empty;
            _awaitingHorde = true;
            SetStatus("놈들이 온다", _warningColor);
        }

        private void HandleSpawningStarted(int round)
        {
            _awaitingHorde = false;
            RefreshRemaining();
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            RefreshRemaining();
        }

        private void HandleRoundCleared(int round)
        {
            StartFlash(_clearFlashTime, blink: true);
            _awaitingHorde = false;
        }

        private void HandleAllRoundsCleared()
        {
            _timerText.text = string.Empty;
            SetStatus("살아남았다", _flashColor);
        }

        private void HandleDefeated(int round)
        {
            _timerText.text = string.Empty;
            _awaitingHorde = false;
            _statusText.color = _warningColor;
            _statusText.SetText("전멸 — {0}라운드", round);
        }

        // ───────────── 표시

        private void RefreshRemaining()
        {
            if (_roundManager.State != RoundState.InProgress || _awaitingHorde) return;

            int remaining = (_spawner != null ? _spawner.AliveCount : 0) + _waveManager.RemainingToSpawn;
            _statusText.color = _textColor;
            _statusText.SetText("남은 좀비  {0}", remaining);
        }

        private void ShowRoundMark(int round)
        {
            bool useTally = round <= TallyRounds;
            for (int i = 0; i < _tallyStrokes.Length; i++)
                _tallyStrokes[i].enabled = useTally && i < round;

            _roundNumber.enabled = !useTally;
            if (!useTally) _roundNumber.SetText("{0}", round);

            ApplyMarkColor(_bloodColor);
        }

        private void StartFlash(float duration, bool blink)
        {
            _flashDuration = duration;
            _flashElapsed = 0f;
            _flashBlink = blink;
        }

        private void TickFlash(float deltaTime)
        {
            if (_flashElapsed >= _flashDuration) return;

            _flashElapsed += deltaTime;
            float t = Mathf.Clamp01(_flashElapsed / _flashDuration);
            float flash = _flashBlink
                ? (Mathf.Sin(_flashElapsed * Mathf.PI * 3f) * 0.5f + 0.5f) * (1f - t)   // 클리어: 흰색↔핏빛 깜빡임이 잦아든다
                : 1f - t * t;                                                          // 시작: 흰색에서 핏빛으로 번진다
            ApplyMarkColor(Color.Lerp(_bloodColor, _flashColor, flash));
        }

        private void ApplyMarkColor(Color color)
        {
            for (int i = 0; i < _tallyStrokes.Length; i++)
                _tallyStrokes[i].color = color;
            _roundNumber.color = color;
        }

        private void TickCountdown()
        {
            if (_roundManager.State != RoundState.Intermission) return;

            int seconds = Mathf.CeilToInt(_roundManager.IntermissionTimeRemaining);
            bool warning = seconds <= _warningSeconds;
            if (warning)
            {
                float pulse = Mathf.Sin(Time.time * Mathf.PI * 2f) * 0.5f + 0.5f;
                _timerText.color = Color.Lerp(_bloodColor, _warningColor, pulse);
            }
            else
            {
                _timerText.color = _textColor;
            }

            if (seconds == _shownSeconds) return;

            _shownSeconds = seconds;
            _timerText.SetText("{0}:{1:00}", seconds / 60, seconds % 60);
        }

        private void TickHordeWarning()
        {
            if (!_awaitingHorde) return;

            float pulse = Mathf.Sin(Time.time * Mathf.PI * 1.5f) * 0.5f + 0.5f;
            _statusText.color = Color.Lerp(_bloodColor, _warningColor, pulse);
        }

        private void SetStatus(string text, Color color)
        {
            _statusText.text = text;
            _statusText.color = color;
        }
    }
}
