using UnityEngine;

namespace Shelter.Audio
{
    /// <summary>게임 사운드 클립·볼륨·공간 음향 거리 설정. 클립 교체와 믹싱은 코드 수정 없이 이 에셋에서 한다.</summary>
    [CreateAssetMenu(menuName = "Shelter/Audio/Shelter Audio Config", fileName = "ShelterAudioConfig")]
    public class ShelterAudioConfig : ScriptableObject
    {
        [Header("라운드")]
        [Tooltip("라운드 예고(텔레그래프) 시작 시 한 번 — 좀비 무리의 울부짖음.")]
        [SerializeField] private AudioClip _roundTelegraph;
        [SerializeField, Range(0f, 1f)] private float _roundTelegraphVolume = 0.9f;
        [Tooltip("라운드 진행 중 반복 재생하는 배경음.")]
        [SerializeField] private AudioClip _roundBgm;
        [SerializeField, Range(0f, 1f)] private float _roundBgmVolume = 0.45f;
        [SerializeField, Min(0f)] private float _bgmFadeInTime = 3f;
        [SerializeField, Min(0f)] private float _bgmFadeOutTime = 4f;

        [Header("좀비 (공간 음향)")]
        [SerializeField] private AudioClip[] _zombieGroans;
        [SerializeField, Range(0f, 1f)] private float _zombieGroanVolume = 0.8f;
        [Tooltip("살아 있는 좀비가 울음소리를 내는 간격 범위(초).")]
        [SerializeField] private Vector2 _zombieGroanInterval = new Vector2(4f, 11f);
        [SerializeField] private AudioClip _zombieFootsteps;
        [SerializeField, Range(0f, 1f)] private float _zombieFootstepVolume = 0.35f;
        [Tooltip("이 속도(m/s) 이상 움직일 때 발소리를 낸다.")]
        [SerializeField, Min(0f)] private float _zombieFootstepMinSpeed = 0.3f;
        [Tooltip("플레이어 무기에 맞았을 때.")]
        [SerializeField] private AudioClip _zombieHit;
        [SerializeField, Range(0f, 1f)] private float _zombieHitVolume = 0.8f;
        [SerializeField] private AudioClip _zombieDeath;
        [SerializeField, Range(0f, 1f)] private float _zombieDeathVolume = 0.9f;
        [Tooltip("좀비 소리가 최대 크기로 들리는 거리 / 들리지 않게 되는 거리(m).")]
        [SerializeField] private Vector2 _zombieAudibleRange = new Vector2(2f, 28f);

        [Header("플레이어 (본인에게만, 2D)")]
        [SerializeField] private AudioClip _playerHit;
        [SerializeField, Range(0f, 1f)] private float _playerHitVolume = 0.9f;
        [SerializeField] private AudioClip _playerDeath;
        [SerializeField, Range(0f, 1f)] private float _playerDeathVolume = 1f;

        public AudioClip RoundTelegraph => _roundTelegraph;
        public float RoundTelegraphVolume => _roundTelegraphVolume;
        public AudioClip RoundBgm => _roundBgm;
        public float RoundBgmVolume => _roundBgmVolume;
        public float BgmFadeInTime => _bgmFadeInTime;
        public float BgmFadeOutTime => _bgmFadeOutTime;

        public AudioClip[] ZombieGroans => _zombieGroans;
        public float ZombieGroanVolume => _zombieGroanVolume;
        public Vector2 ZombieGroanInterval => _zombieGroanInterval;
        public AudioClip ZombieFootsteps => _zombieFootsteps;
        public float ZombieFootstepVolume => _zombieFootstepVolume;
        public float ZombieFootstepMinSpeed => _zombieFootstepMinSpeed;
        public AudioClip ZombieHit => _zombieHit;
        public float ZombieHitVolume => _zombieHitVolume;
        public AudioClip ZombieDeath => _zombieDeath;
        public float ZombieDeathVolume => _zombieDeathVolume;
        public Vector2 ZombieAudibleRange => _zombieAudibleRange;

        public AudioClip PlayerHit => _playerHit;
        public float PlayerHitVolume => _playerHitVolume;
        public AudioClip PlayerDeath => _playerDeath;
        public float PlayerDeathVolume => _playerDeathVolume;

        /// <summary>3D 효과음용 AudioSource 공통 설정(로그 감쇠 대신 선형 — 거리감이 분명해 방향 판단에 유리).</summary>
        public void ConfigureSpatial(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = _zombieAudibleRange.x;
            source.maxDistance = _zombieAudibleRange.y;
            source.dopplerLevel = 0f;
        }
    }
}
