using Shelter.Enemies.Spawning.Waves;
using Shelter.Rounds;
using UnityEngine;

namespace Shelter.Audio
{
    /// <summary>
    /// 라운드 흐름에 맞춘 2D 음악·스팅어: 라운드 예고 때 좀비 무리 울부짖음, 라운드 중 배경음(페이드 인), 클리어·패배·승리 시 페이드 아웃.
    /// 라운드 상태는 읽기만 한다(이벤트 구독). 각 클라이언트가 로컬로 재생하는 표현 계층이다.
    /// </summary>
    public class RoundAudioDirector : MonoBehaviour
    {
        [SerializeField] private ShelterAudioConfig _config;
        [SerializeField] private RoundManager _roundManager;
        [SerializeField] private WaveManager _waveManager;

        private AudioSource _stinger;
        private AudioSource _bgm;
        private float _bgmTarget;
        private float _bgmFadeSpeed;

        private void Awake()
        {
            if (_config == null || _roundManager == null || _waveManager == null)
            {
                Debug.LogError($"{name}: ShelterAudioConfig / RoundManager / WaveManager 가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _stinger = CreateSource("Stinger", loop: false);
            _bgm = CreateSource("BGM", loop: true);
            _bgm.clip = _config.RoundBgm;
            _bgm.volume = 0f;
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var source = new GameObject(sourceName).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private void OnEnable()
        {
            if (_roundManager == null || _waveManager == null) return;

            _waveManager.RoundTelegraphStarted += HandleTelegraph;
            _roundManager.RoundStarted += HandleRoundStarted;
            _roundManager.RoundCleared += HandleRoundEnded;
            _roundManager.Defeated += HandleRoundEnded;
            _roundManager.AllRoundsCleared += HandleAllRoundsCleared;
        }

        private void OnDisable()
        {
            if (_roundManager == null || _waveManager == null) return;

            _waveManager.RoundTelegraphStarted -= HandleTelegraph;
            _roundManager.RoundStarted -= HandleRoundStarted;
            _roundManager.RoundCleared -= HandleRoundEnded;
            _roundManager.Defeated -= HandleRoundEnded;
            _roundManager.AllRoundsCleared -= HandleAllRoundsCleared;
        }

        private void HandleTelegraph(int round)
        {
            if (_config.RoundTelegraph != null)
                _stinger.PlayOneShot(_config.RoundTelegraph, _config.RoundTelegraphVolume);
        }

        private void HandleRoundStarted(int round)
        {
            if (_bgm.clip == null) return;

            if (!_bgm.isPlaying) _bgm.Play();
            FadeBgm(_config.RoundBgmVolume, _config.BgmFadeInTime);
        }

        private void HandleRoundEnded(int round)
        {
            FadeBgm(0f, _config.BgmFadeOutTime);
        }

        private void HandleAllRoundsCleared()
        {
            FadeBgm(0f, _config.BgmFadeOutTime);
        }

        private void FadeBgm(float target, float seconds)
        {
            _bgmTarget = target;
            _bgmFadeSpeed = seconds > 0f ? Mathf.Max(0.01f, Mathf.Abs(target - _bgm.volume) / seconds) : float.PositiveInfinity;
        }

        private void Update()
        {
            if (Mathf.Approximately(_bgm.volume, _bgmTarget))
            {
                if (_bgmTarget <= 0f && _bgm.isPlaying) _bgm.Pause();
                return;
            }

            _bgm.volume = Mathf.MoveTowards(_bgm.volume, _bgmTarget, _bgmFadeSpeed * Time.deltaTime);
        }
    }
}
