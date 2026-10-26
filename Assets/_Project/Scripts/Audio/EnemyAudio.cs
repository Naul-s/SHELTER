using Shelter.Core;
using Shelter.Enemies;
using Shelter.Enemies.Movement;
using UnityEngine;

namespace Shelter.Audio
{
    /// <summary>
    /// 좀비 한 마리의 공간 음향: 간헐적 울음소리, 이동 중 발소리, 피격음, 사망음.
    /// 어느 창문 쪽에서 좀비가 오는지 소리로 알 수 있게 전부 3D로 재생한다. 풀링되는 프리팹에 붙어 있어 AudioSource는 Awake에서 한 번만 만든다.
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyAudio : MonoBehaviour
    {
        [SerializeField] private ShelterAudioConfig _config;

        private EnemyBase _enemy;
        private IEnemyMovement _movement;
        private AudioSource _voice;
        private AudioSource _feet;
        private float _groanTimer;

        private void Awake()
        {
            _enemy = GetComponent<EnemyBase>();
            _movement = GetComponent<IEnemyMovement>();

            if (_config == null)
            {
                Debug.LogError($"{name}: ShelterAudioConfig가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _voice = gameObject.AddComponent<AudioSource>();
            _config.ConfigureSpatial(_voice);

            _feet = gameObject.AddComponent<AudioSource>();
            _config.ConfigureSpatial(_feet);
            _feet.clip = _config.ZombieFootsteps;
            _feet.loop = true;
            _feet.volume = _config.ZombieFootstepVolume;
            _feet.priority = 200; // 동시 발성 수가 모자라면 발소리부터 빠진다(울음·피격·사망 우선)
        }

        private void OnEnable()
        {
            _enemy.Damaged += HandleDamaged;
            _enemy.Died += HandleDied;
            ResetGroanTimer();
        }

        private void OnDisable()
        {
            _enemy.Damaged -= HandleDamaged;
            _enemy.Died -= HandleDied;
            if (_feet != null) _feet.Stop();
        }

        private void Update()
        {
            if (_enemy.IsDead) return;

            UpdateFootsteps();

            _groanTimer -= Time.deltaTime;
            if (_groanTimer > 0f) return;

            ResetGroanTimer();
            PlayRandom(_config.ZombieGroans, _config.ZombieGroanVolume);
        }

        private void UpdateFootsteps()
        {
            if (_feet.clip == null || _movement == null) return;

            bool moving = _movement.CurrentSpeed >= _config.ZombieFootstepMinSpeed || _enemy.State == EnemyState.Entering;
            if (moving && !_feet.isPlaying)
            {
                _feet.time = Random.Range(0f, _feet.clip.length); // 여러 마리의 발소리가 같은 박자로 겹치지 않게
                _feet.Play();
            }
            else if (!moving && _feet.isPlaying)
            {
                _feet.Pause();
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (_enemy.IsDead || _config.ZombieHit == null) return;
            _voice.PlayOneShot(_config.ZombieHit, _config.ZombieHitVolume);
        }

        private void HandleDied()
        {
            _feet.Stop();
            _voice.Stop();
            if (_config.ZombieDeath != null)
                _voice.PlayOneShot(_config.ZombieDeath, _config.ZombieDeathVolume);
        }

        private void ResetGroanTimer()
        {
            Vector2 range = _config != null ? _config.ZombieGroanInterval : new Vector2(4f, 11f);
            _groanTimer = Random.Range(range.x, range.y);
        }

        private void PlayRandom(AudioClip[] clips, float volume)
        {
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            _voice.pitch = Random.Range(0.9f, 1.1f);
            _voice.PlayOneShot(clip, volume);
        }
    }
}
