using UnityEngine;

namespace Shelter.Environment
{
    /// <summary>
    /// 낡은 전등 느낌의 조명 떨림(로컬 연출 — 게임 상태와 무관). 평소엔 미세하게 일렁이고,
    /// 가끔 몇 번 끊기듯 어두워졌다 돌아온다. 같은 오브젝트의 Light 와, 연결한 광선(LightShaft) 렌더러의
    /// _Intensity 를 함께 흔들어 빛 원뿔이 전등과 같이 깜빡인다.
    /// 광과민·VR 피로를 피하려고 끊김은 초당 4회 이하, 완전히 꺼지지 않는 깊이(_dipDepth)로만 한다.
    /// </summary>
    public class LightFlicker : MonoBehaviour
    {
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        // 끊김 한 번의 길이(초) — 초당 4회 이하가 되도록.
        private const float DipDuration = 0.25f;

        [Tooltip("평소 일렁임 폭(기본 밝기 대비 비율).")]
        [SerializeField, Range(0f, 0.5f)] private float _wobble = 0.08f;
        [SerializeField, Min(0.1f)] private float _wobbleSpeed = 1.6f;
        [Tooltip("끊김이 일어나는 간격(초, 최소~최대 무작위).")]
        [SerializeField] private Vector2 _burstInterval = new Vector2(5f, 14f);
        [Tooltip("한 번에 끊기는 횟수(최소~최대).")]
        [SerializeField] private Vector2Int _dipsPerBurst = new Vector2Int(1, 3);
        [Tooltip("끊길 때 밝기가 떨어지는 정도(0 = 그대로, 1 = 완전히 꺼짐).")]
        [SerializeField, Range(0f, 0.9f)] private float _dipDepth = 0.65f;
        [Tooltip("같이 깜빡일 광선 렌더러(Shelter/LightShaft 머티리얼).")]
        [SerializeField] private Renderer[] _shafts;

        private Light _light;
        private float _baseIntensity;
        private float[] _shaftBase;
        private MaterialPropertyBlock _block;
        private float _seed;
        private float _nextBurst;
        private int _dipsLeft;
        private float _dipTimer;

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_light != null) _baseIntensity = _light.intensity;

            _block = new MaterialPropertyBlock();
            _shaftBase = new float[_shafts != null ? _shafts.Length : 0];
            for (int i = 0; i < _shaftBase.Length; i++)
                _shaftBase[i] = _shafts[i] != null && _shafts[i].sharedMaterial != null ? _shafts[i].sharedMaterial.GetFloat(IntensityId) : 0f;
            _seed = Random.value * 100f;
            ScheduleBurst();
        }

        private void OnDisable()
        {
            Apply(1f);
        }

        private void Update()
        {
            float t = Time.time;
            float wobble = (Mathf.PerlinNoise(_seed, t * _wobbleSpeed) - 0.5f) * 2f * _wobble;

            if (_dipsLeft == 0 && t >= _nextBurst)
            {
                _dipsLeft = Random.Range(_dipsPerBurst.x, _dipsPerBurst.y + 1);
                _dipTimer = 0f;
            }

            float dip = 0f;
            if (_dipsLeft > 0)
            {
                _dipTimer += Time.deltaTime;
                float phase = _dipTimer / DipDuration;
                // 한 번의 끊김: 빠르게 어두워졌다가 조금 느리게 돌아온다
                dip = phase < 0.3f ? phase / 0.3f : 1f - (phase - 0.3f) / 0.7f;
                if (phase >= 1f)
                {
                    _dipTimer = 0f;
                    if (--_dipsLeft == 0) ScheduleBurst();
                }
            }

            Apply((1f + wobble) * (1f - _dipDepth * Mathf.Clamp01(dip)));
        }

        private void Apply(float factor)
        {
            if (_light != null) _light.intensity = _baseIntensity * factor;
            if (_shaftBase == null) return;

            for (int i = 0; i < _shaftBase.Length; i++)
            {
                if (_shafts[i] == null) continue;
                _shafts[i].GetPropertyBlock(_block);
                _block.SetFloat(IntensityId, _shaftBase[i] * factor);
                _shafts[i].SetPropertyBlock(_block);
            }
        }

        private void ScheduleBurst()
        {
            _nextBurst = Time.time + Random.Range(_burstInterval.x, _burstInterval.y);
        }
    }
}
