using UnityEngine;

namespace Shelter.ShelterDefense
{
    /// <summary>
    /// 바리케이드 상태 변화의 로컬 연출: 떨어진 판자가 바깥으로 튕겨 나가 사라지고, 나무 파편 파티클과 효과음을 낸다.
    /// 상태는 바꾸지 않고 <see cref="Barricade"/> 이벤트만 구독하므로, 네트워킹 도입 후에도 각 클라이언트가 그대로 재생하면 된다.
    /// 판자 수가 고정이라 판자 오브젝트 자체를 켜고 끄며(풀링 불필요), 파편 파티클은 바리케이드당 하나를 재사용한다.
    /// </summary>
    [RequireComponent(typeof(Barricade))]
    public class BarricadeView : MonoBehaviour
    {
        private const float Gravity = 9.81f;

        [SerializeField] private ParticleSystem _splinterPrefab;
        [Tooltip("판자가 떨어질 때 무작위로 하나 재생.")]
        [SerializeField] private AudioClip[] _breakClips;
        [Tooltip("판자가 떨어지지 않은 타격(HitsPerBoard > 1)일 때 무작위로 하나 재생.")]
        [SerializeField] private AudioClip[] _hitClips;
        [SerializeField] private AudioSource _audioSource;

        private Barricade _barricade;
        private ParticleSystem _splinters;

        private Transform[] _boards;
        private Renderer[] _boardRenderers;
        private Vector3[] _homeLocalPositions;
        private Quaternion[] _homeLocalRotations;
        private float[] _flyElapsed;
        private Vector3[] _flyVelocity;
        private Vector3[] _flyAngularVelocity;
        private int _flyingCount;

        private void Awake()
        {
            _barricade = GetComponent<Barricade>();

            int count = _barricade.BoardTransforms != null ? _barricade.BoardTransforms.Count : 0;
            _boards = new Transform[count];
            _boardRenderers = new Renderer[count];
            _homeLocalPositions = new Vector3[count];
            _homeLocalRotations = new Quaternion[count];
            _flyElapsed = new float[count];
            _flyVelocity = new Vector3[count];
            _flyAngularVelocity = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                Transform board = _barricade.BoardTransforms[i];
                _boards[i] = board;
                _flyElapsed[i] = -1f;
                if (board == null) continue;

                _boardRenderers[i] = board.GetComponentInChildren<Renderer>();
                _homeLocalPositions[i] = board.localPosition;
                _homeLocalRotations[i] = board.localRotation;
            }

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f;
                _audioSource.rolloffMode = AudioRolloffMode.Linear;
                _audioSource.minDistance = 1.5f;
                _audioSource.maxDistance = 30f;
            }

            // 바리케이드 루트는 비균등 스케일일 수 있어 파티클을 자식으로 두지 않는다(모양이 찌그러짐).
            if (_splinterPrefab != null)
            {
                _splinters = Instantiate(_splinterPrefab);
                _splinters.name = $"{name}_Splinters";
                _splinters.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void OnEnable()
        {
            _barricade.BoardHit += HandleBoardHit;
            _barricade.BoardRemoved += HandleBoardRemoved;
            _barricade.BoardRestored += HandleBoardRestored;
        }

        private void OnDisable()
        {
            _barricade.BoardHit -= HandleBoardHit;
            _barricade.BoardRemoved -= HandleBoardRemoved;
            _barricade.BoardRestored -= HandleBoardRestored;
        }

        private void OnDestroy()
        {
            if (_splinters != null) Destroy(_splinters.gameObject);
        }

        private void HandleBoardHit(int remainingBoards)
        {
            PlayRandom(_hitClips);

            int nextIndex = _barricade.MaxBoards - remainingBoards;
            if (nextIndex >= 0 && nextIndex < _boards.Length)
                EmitSplinters(BoardCenter(nextIndex), 6);
        }

        private void HandleBoardRemoved(int index)
        {
            if (index < 0 || index >= _boards.Length || _boards[index] == null) return;

            Vector3 center = BoardCenter(index);
            EmitSplinters(center, 18);
            PlayRandom(_breakClips);

            // 좀비가 뜯어내는 판자는 바깥(좀비 쪽)으로 튕겨 나간다.
            Vector3 outward = _barricade.OutsidePosition - _barricade.InsidePosition;
            outward.y = 0f;
            outward = outward.sqrMagnitude > 0.0001f ? outward.normalized : -transform.forward;

            float speed = _barricade.Config != null ? _barricade.Config.BoardFlySpeed : 2.5f;
            if (_flyElapsed[index] < 0f) _flyingCount++;
            _flyElapsed[index] = 0f;
            _flyVelocity[index] = outward * speed + Vector3.up * (speed * 0.6f);
            _flyAngularVelocity[index] = Random.onUnitSphere * 360f;
        }

        private void HandleBoardRestored(int index)
        {
            if (index < 0 || index >= _boards.Length || _boards[index] == null) return;

            if (_flyElapsed[index] >= 0f) _flyingCount--;
            _flyElapsed[index] = -1f;

            Transform board = _boards[index];
            board.localPosition = _homeLocalPositions[index];
            board.localRotation = _homeLocalRotations[index];
            board.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_flyingCount <= 0) return;

            float deltaTime = Time.deltaTime;
            float flyTime = _barricade.Config != null ? _barricade.Config.BoardFlyTime : 0.8f;

            for (int i = 0; i < _boards.Length; i++)
            {
                if (_flyElapsed[i] < 0f) continue;

                Transform board = _boards[i];
                _flyElapsed[i] += deltaTime;
                _flyVelocity[i].y -= Gravity * deltaTime;
                board.position += _flyVelocity[i] * deltaTime;
                board.rotation = Quaternion.Euler(_flyAngularVelocity[i] * deltaTime) * board.rotation;

                if (_flyElapsed[i] < flyTime) continue;

                _flyElapsed[i] = -1f;
                _flyingCount--;
                board.gameObject.SetActive(false);
            }
        }

        private Vector3 BoardCenter(int index)
        {
            Renderer boardRenderer = _boardRenderers[index];
            return boardRenderer != null ? boardRenderer.bounds.center : _boards[index].position;
        }

        private void EmitSplinters(Vector3 position, int count)
        {
            if (_splinters == null) return;

            _splinters.transform.position = position;
            _splinters.Emit(count);
        }

        private void PlayRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0 || _audioSource == null) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            float volume = _barricade.Config != null ? _barricade.Config.SfxVolume : 1f;
            _audioSource.pitch = Random.Range(0.92f, 1.08f);
            _audioSource.PlayOneShot(clip, volume);
        }
    }
}
