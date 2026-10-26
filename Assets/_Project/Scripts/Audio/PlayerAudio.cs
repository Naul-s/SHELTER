using Shelter.Core;
using Shelter.Player;
using UnityEngine;

namespace Shelter.Audio
{
    /// <summary>플레이어 본인의 피격·사망 소리(2D, 로컬). 원격 플레이어 소리는 네트워킹 도입 시 3D로 따로 재생한다.</summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private ShelterAudioConfig _config;

        private PlayerHealth _health;
        private AudioSource _source;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();

            if (_config == null)
            {
                Debug.LogError($"{name}: ShelterAudioConfig가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            _health.Damaged += HandleDamaged;
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Damaged -= HandleDamaged;
            _health.Died -= HandleDied;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (_health.IsDead || _config.PlayerHit == null) return;
            _source.PlayOneShot(_config.PlayerHit, _config.PlayerHitVolume);
        }

        private void HandleDied()
        {
            if (_config.PlayerDeath == null) return;
            _source.Stop();
            _source.PlayOneShot(_config.PlayerDeath, _config.PlayerDeathVolume);
        }
    }
}
