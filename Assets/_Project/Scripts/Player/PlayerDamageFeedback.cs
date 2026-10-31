using Shelter.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace Shelter.Player
{
    /// <summary>
    /// 로컬 플레이어가 맞았다는 걸 VR 에서 확실히 느끼게 하는 피드백(개인 표현 — 클라이언트 로컬).
    /// 카메라는 절대 움직이지 않는다(vr-performance-comfort). 대신:
    /// - 시야 가장자리 붉은 비네트: 맞는 순간 번쩍(공격한 좀비가 있는 쪽이 더 진하게) → 서서히 빠짐
    /// - 양손 컨트롤러 진동: 공격이 들어온 쪽 손이 더 세게
    /// - 저체력: 가장자리가 붉게 남아 심장박동에 맞춰 맥동 + 심장박동 소리(체력이 낮을수록 크고 빠르게)
    /// - 회복: 옅은 초록 번쩍 + 약한 진동 / 사망: 시야가 어둡게 가라앉음 + 긴 진동
    /// 비네트는 이 플레이어의 카메라가 그릴 때만 켠다 — 다른 카메라(다른 플레이어·관전)에는 머리 앞 원판이 보이지 않는다.
    /// 네트워킹 도입 시 원격 플레이어 리그에서는 이 컴포넌트를 끈다(자기 시야·손에만 해당).
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDamageFeedback : MonoBehaviour
    {
        // 비네트 원판 메시 해상도와 배치(카메라 기준). 연출 수치가 아닌 기하 상수.
        private const int Rings = 10;
        private const int Segments = 48;
        private const float MeshDistance = 0.1f;
        private const float MaxAngle = 85f;
        /// <summary>이 각도(도)에서 비네트가 가장 진하다 — 대략 헤드셋 시야 가장자리.</summary>
        private const float EdgeAngle = 50f;

        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int LowColorId = Shader.PropertyToID("_LowColor");
        private static readonly int FadeColorId = Shader.PropertyToID("_FadeColor");
        private static readonly int HitDirId = Shader.PropertyToID("_HitDir");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int FlashCoverageId = Shader.PropertyToID("_FlashCoverage");
        private static readonly int LowId = Shader.PropertyToID("_Low");
        private static readonly int LowCoverageId = Shader.PropertyToID("_LowCoverage");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");

        [SerializeField] private Shader _vignetteShader;

        [Header("피격")]
        [SerializeField] private Color _hitColor = new Color(0.6f, 0f, 0f, 1f);
        [Tooltip("최대 체력의 이 비율 이상을 한 번에 잃으면 가장 강한 피드백.")]
        [SerializeField, Range(0.01f, 1f)] private float _heavyHitFraction = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _hitFlashMin = 0.55f;
        [SerializeField, Range(0f, 1f)] private float _hitFlashMax = 0.9f;
        [SerializeField, Min(0.05f)] private float _hitFlashFadeTime = 0.7f;
        [Tooltip("번쩍임이 시야 안쪽으로 들어오는 정도(0~1).")]
        [SerializeField, Range(0.05f, 1f)] private float _hitFlashCoverage = 0.6f;
        [Tooltip("맞은 방향 쪽으로 치우치는 정도(0 = 고르게).")]
        [SerializeField, Range(0f, 1f)] private float _hitDirectional = 0.75f;
        [SerializeField, Range(0f, 1f)] private float _hitHapticMin = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _hitHapticMax = 1f;
        [SerializeField, Min(0f)] private float _hitHapticDuration = 0.2f;
        [Tooltip("공격 반대쪽 손의 진동 비율.")]
        [SerializeField, Range(0f, 1f)] private float _farHandHaptic = 0.35f;

        [Header("저체력")]
        [Tooltip("체력이 이 비율 아래로 내려가면 저체력 연출 시작.")]
        [SerializeField, Range(0.05f, 1f)] private float _lowHealthThreshold = 0.35f;
        [SerializeField] private Color _lowHealthColor = new Color(0.4f, 0f, 0f, 1f);
        [SerializeField, Range(0f, 1f)] private float _lowHealthMaxOpacity = 0.6f;
        [SerializeField, Range(0.05f, 1f)] private float _lowHealthCoverage = 0.45f;
        [SerializeField] private AudioClip _heartbeatClip;
        [SerializeField, Range(0f, 1f)] private float _heartbeatMaxVolume = 0.8f;
        [Tooltip("체력 0 직전의 심장박동 재생 속도(1 = 원본).")]
        [SerializeField, Range(1f, 2f)] private float _heartbeatMaxRate = 1.45f;

        [Header("회복")]
        [SerializeField] private Color _healColor = new Color(0.25f, 0.75f, 0.35f, 1f);
        [SerializeField, Range(0f, 1f)] private float _healFlash = 0.45f;
        [SerializeField, Min(0.05f)] private float _healFlashFadeTime = 1f;
        [SerializeField, Range(0f, 1f)] private float _healHaptic = 0.3f;
        [SerializeField] private AudioClip _healClip;
        [SerializeField, Range(0f, 1f)] private float _healVolume = 0.8f;

        [Header("사망")]
        [SerializeField] private Color _deathColor = new Color(0.07f, 0f, 0f, 1f);
        [Tooltip("사망 후 시야가 가라앉는 최종 정도(1 = 완전히 가림).")]
        [SerializeField, Range(0f, 1f)] private float _deathFadeTarget = 0.88f;
        [SerializeField, Min(0.05f)] private float _deathFadeTime = 1.6f;

        private PlayerHealth _health;
        private Camera _camera;
        private Transform _head;
        private HapticImpulsePlayer[] _haptics;
        private Renderer _vignetteRenderer;
        private Material _material;
        private AudioSource _heartbeat;
        private AudioSource _sfx;

        private float _flash;
        private float _flashFadeTime;
        private float _fade;
        private float _low;
        private bool _visible;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            Camera cam = GetComponentInChildren<Camera>(true);

            if (_vignetteShader == null || cam == null)
            {
                Debug.LogError($"{name}: 비네트 셰이더 또는 플레이어 카메라가 없습니다.", this);
                enabled = false;
                return;
            }

            _camera = cam;
            _head = cam.transform;
            _haptics = GetComponentsInChildren<HapticImpulsePlayer>(true);
            BuildVignette();

            _heartbeat = gameObject.AddComponent<AudioSource>();
            _heartbeat.playOnAwake = false;
            _heartbeat.loop = true;
            _heartbeat.spatialBlend = 0f;
            _heartbeat.clip = _heartbeatClip;
            _heartbeat.volume = 0f;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            _health.Damaged += HandleDamaged;
            _health.Healed += HandleHealed;
            _health.Died += HandleDied;
            RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
        }

        private void OnDisable()
        {
            _health.Damaged -= HandleDamaged;
            _health.Healed -= HandleHealed;
            _health.Died -= HandleDied;
            RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
            _visible = false;
            if (_vignetteRenderer != null) _vignetteRenderer.enabled = false;
            if (_heartbeat != null) _heartbeat.Stop();
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            float severity = Mathf.Clamp01(damageInfo.Amount / (_health.MaxHealth * _heavyHitFraction));

            Vector3 from = damageInfo.Instigator != null ? damageInfo.Instigator.transform.position : _head.position + _head.forward;
            Vector3 local = _head.InverseTransformDirection(from - _head.position);
            var planar = new Vector2(local.x, local.y);
            float directional = local.sqrMagnitude > 0.0001f ? planar.magnitude / local.magnitude : 0f;
            if (planar.sqrMagnitude > 0.0001f) planar.Normalize();

            _material.SetColor(FlashColorId, _hitColor);
            _material.SetVector(HitDirId, new Vector4(planar.x, planar.y, directional * _hitDirectional, 0f));
            _material.SetFloat(FlashCoverageId, _hitFlashCoverage);
            _flash = Mathf.Max(_flash, Mathf.Lerp(_hitFlashMin, _hitFlashMax, severity));
            _flashFadeTime = _hitFlashFadeTime;

            PulseHands(Mathf.Lerp(_hitHapticMin, _hitHapticMax, severity), _hitHapticDuration, from);
        }

        private void HandleHealed(float amount)
        {
            _material.SetColor(FlashColorId, _healColor);
            _material.SetVector(HitDirId, Vector4.zero);
            _material.SetFloat(FlashCoverageId, 0.5f);
            _flash = _healFlash;
            _flashFadeTime = _healFlashFadeTime;

            PulseHands(_healHaptic, 0.12f, _head.position);
            if (_healClip != null) _sfx.PlayOneShot(_healClip, _healVolume);
        }

        private void HandleDied()
        {
            _heartbeat.Stop();
            PulseHands(1f, 0.6f, _head.position);
        }

        /// <summary>공격이 들어온 쪽 손은 세게, 반대쪽 손은 약하게 진동시킨다.</summary>
        private void PulseHands(float amplitude, float duration, Vector3 source)
        {
            Vector3 toSource = source - _head.position;
            toSource.y = 0f;
            bool hasSide = toSource.sqrMagnitude > 0.01f;
            if (hasSide) toSource.Normalize();

            for (int i = 0; i < _haptics.Length; i++)
            {
                HapticImpulsePlayer hand = _haptics[i];
                if (hand == null || !hand.isActiveAndEnabled) continue;

                float weight = 1f;
                if (hasSide)
                {
                    Vector3 toHand = hand.transform.position - _head.position;
                    toHand.y = 0f;
                    float side = toHand.sqrMagnitude > 0.0001f ? Vector3.Dot(toHand.normalized, toSource) : 0f;
                    weight = Mathf.Lerp(_farHandHaptic, 1f, side * 0.5f + 0.5f);
                }
                hand.SendHapticImpulse(amplitude * weight, duration);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_flash > 0f) _flash = Mathf.Max(0f, _flash - dt / _flashFadeTime);

            float targetFade = _health.IsDead ? _deathFadeTarget : 0f;
            _fade = Mathf.MoveTowards(_fade, targetFade, dt / _deathFadeTime);

            UpdateLowHealth();

            _visible = _flash > 0.001f || _low > 0.001f || _fade > 0.001f;
            if (!_visible) return;

            _material.SetFloat(FlashId, _flash);
            _material.SetFloat(LowId, _low);
            _material.SetFloat(FadeId, _fade);
        }

        /// <summary>카메라마다 컬링 직전에 호출된다 — 자기 카메라일 때만 비네트를 켠다.</summary>
        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            _vignetteRenderer.enabled = _visible && camera == _camera;
        }

        private void UpdateLowHealth()
        {
            float danger = _health.IsDead ? 0f : Mathf.Clamp01((_lowHealthThreshold - _health.Health01) / _lowHealthThreshold);

            if (_heartbeatClip != null)
            {
                if (danger > 0f && !_heartbeat.isPlaying) _heartbeat.Play();
                else if (danger <= 0f && _heartbeat.isPlaying) _heartbeat.Stop();
                _heartbeat.volume = Mathf.Lerp(0.35f, 1f, danger) * _heartbeatMaxVolume * (danger > 0f ? 1f : 0f);
                _heartbeat.pitch = Mathf.Lerp(1f, _heartbeatMaxRate, danger);
            }

            if (danger <= 0f)
            {
                _low = 0f;
                return;
            }

            // 심장이 뛸 때(쿵-쿵) 가장자리가 조여드는 맥동. 소리가 있으면 재생 위치에, 없으면 시간에 맞춘다.
            float phase = _heartbeat.isPlaying && _heartbeatClip.length > 0f
                ? _heartbeat.time / _heartbeatClip.length
                : Time.time * _heartbeat.pitch % 1f;
            float beat = Mathf.Max(Mathf.Exp(-phase * 9f), phase > 0.3f ? 0.7f * Mathf.Exp(-(phase - 0.3f) * 9f) : 0f);
            _low = danger * _lowHealthMaxOpacity * (0.7f + 0.3f * beat);
        }

        /// <summary>카메라 앞에 시야를 덮는 원판 메시를 만든다(중심 → 바깥으로 링, 각 정점에 가장자리 비율과 방향을 넣는다).</summary>
        private void BuildVignette()
        {
            var go = new GameObject("DamageVignette");
            go.layer = _head.gameObject.layer;
            go.transform.SetParent(_head, false);

            int vertexCount = 1 + Rings * Segments;
            var vertices = new Vector3[vertexCount];
            var edge = new Vector2[vertexCount];
            var dirs = new Vector2[vertexCount];
            var triangles = new int[(Segments + (Rings - 1) * Segments * 2) * 3];

            vertices[0] = new Vector3(0f, 0f, MeshDistance);
            for (int r = 0; r < Rings; r++)
            {
                float angle = MaxAngle * (r + 1) / Rings;
                float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * MeshDistance;
                for (int s = 0; s < Segments; s++)
                {
                    float a = s * Mathf.PI * 2f / Segments;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    int v = 1 + r * Segments + s;
                    vertices[v] = new Vector3(dir.x * radius, dir.y * radius, MeshDistance);
                    edge[v] = new Vector2(Mathf.Min(angle / EdgeAngle, 1.2f), 0f);
                    dirs[v] = dir;
                }
            }

            int t = 0;
            for (int s = 0; s < Segments; s++)
            {
                triangles[t++] = 0;
                triangles[t++] = 1 + (s + 1) % Segments;
                triangles[t++] = 1 + s;
            }
            for (int r = 0; r < Rings - 1; r++)
            {
                for (int s = 0; s < Segments; s++)
                {
                    int a = 1 + r * Segments + s;
                    int b = 1 + r * Segments + (s + 1) % Segments;
                    int c = a + Segments;
                    int d = b + Segments;
                    triangles[t++] = a; triangles[t++] = b; triangles[t++] = d;
                    triangles[t++] = a; triangles[t++] = d; triangles[t++] = c;
                }
            }

            var mesh = new Mesh { name = "DamageVignette" };
            mesh.vertices = vertices;
            mesh.uv = edge;
            mesh.uv2 = dirs;
            mesh.triangles = triangles;
            mesh.bounds = new Bounds(new Vector3(0f, 0f, MeshDistance), Vector3.one * 4f);

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.enabled = false;

            _material = new Material(_vignetteShader) { name = "DamageVignette (Instance)" };
            _material.SetColor(LowColorId, _lowHealthColor);
            _material.SetColor(FadeColorId, _deathColor);
            _material.SetFloat(LowCoverageId, _lowHealthCoverage);
            renderer.sharedMaterial = _material;
            _vignetteRenderer = renderer;
        }
    }
}
