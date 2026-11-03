using UnityEngine;

namespace Shelter.UI
{
    /// <summary>
    /// 메뉴 캔버스를 화면에 붙이지 않고 월드에 띄운다(ui-audio: 화면 고정 UI 금지).
    /// 켜질 때 플레이어 머리 정면(수평 방향) 일정 거리·높이에 놓고 머리를 향하게 한 뒤 서서히 나타난다.
    /// 고개를 크게 돌려 한동안 메뉴를 보지 않으면 천천히(감쇠) 다시 정면으로 온다 — 카메라는 움직이지 않는다.
    /// 머리(카메라) 아래에 붙어 있어도 부모를 바꾸지 않고, 매 프레임 마지막에 월드 위치를 다시 고정한다
    /// (프리팹 인스턴스 안에서는 부모를 바꿀 수 없다).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class WorldMenuPlacer : MonoBehaviour
    {
        [SerializeField, Min(0.3f)] private float _distance = 1.6f;
        [Tooltip("눈높이 기준 높이(m). 음수면 조금 아래 — 고개를 숙이지 않고 읽기 편한 위치.")]
        [SerializeField] private float _heightOffset = -0.12f;
        [SerializeField, Min(0f)] private float _fadeInTime = 0.35f;
        [Tooltip("메뉴 앞을 막는 벽·입구 차단벽이 있으면 그 앞으로 당겨 온다. 플레이어(2)·UI(5)·자기 몸(8) 레이어는 뺀다.")]
        [SerializeField] private LayerMask _obstacleMask = ~((1 << 2) | (1 << 5) | (1 << 8));
        [SerializeField, Min(0.3f)] private float _minDistance = 0.6f;

        [Header("따라오기")]
        [SerializeField] private bool _follow = true;
        [Tooltip("메뉴가 시선에서 이 각도(도) 넘게 벗어나 있으면 따라온다.")]
        [SerializeField, Range(10f, 120f)] private float _followAngle = 55f;
        [SerializeField, Min(0f)] private float _followDelay = 0.6f;
        [SerializeField, Min(0.1f)] private float _followSpeed = 2.5f;

        private CanvasGroup _group;
        private Transform _head;
        private float _fade;
        private float _awayTimer;
        private bool _following;
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (_head == null && Camera.main != null) _head = Camera.main.transform;
            PlaceInFrontOfViewer();
            _fade = _fadeInTime > 0f ? 0f : 1f;
            _group.alpha = _fade;
        }

        /// <summary>지금 머리 정면에 바로 놓는다.</summary>
        public void PlaceInFrontOfViewer()
        {
            if (_head == null) return;
            ComputeTarget(out _targetPosition, out _targetRotation);
            _position = _targetPosition;
            _rotation = _targetRotation;
            transform.SetPositionAndRotation(_position, _rotation);
            _following = false;
            _awayTimer = 0f;
        }

        private void ComputeTarget(out Vector3 position, out Quaternion rotation)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_head.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.ProjectOnPlane(_head.up, Vector3.up);
            forward.Normalize();
            // 벽 속에 묻히지 않게: 앞이 막혀 있으면 벽 바로 앞(최소 거리까지)으로
            float distance = _distance;
            if (Physics.Raycast(_head.position, forward, out RaycastHit hit, _distance + 0.2f, _obstacleMask, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(_minDistance, hit.distance - 0.2f);
            position = _head.position + forward * distance + Vector3.up * _heightOffset;
            // 월드 캔버스는 앞면이 -forward 쪽 → forward 를 머리에서 멀어지는 방향으로
            rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_fade < 1f)
            {
                _fade = Mathf.Min(1f, _fade + dt / _fadeInTime);
                _group.alpha = _fade * _fade * (3f - 2f * _fade);
            }

            if (!_follow || _head == null) return;

            Vector3 toMenu = Vector3.ProjectOnPlane(_position - _head.position, Vector3.up);
            Vector3 look = Vector3.ProjectOnPlane(_head.forward, Vector3.up);
            float angle = toMenu.sqrMagnitude > 0.0001f && look.sqrMagnitude > 0.0001f ? Vector3.Angle(toMenu, look) : 0f;

            if (!_following)
            {
                _awayTimer = angle > _followAngle ? _awayTimer + dt : 0f;
                if (_awayTimer < _followDelay) return;
                _following = true;
            }

            ComputeTarget(out _targetPosition, out _targetRotation);
            float k = 1f - Mathf.Exp(-_followSpeed * dt);
            _position = Vector3.Lerp(_position, _targetPosition, k);
            _rotation = Quaternion.Slerp(_rotation, _targetRotation, k);
            if ((_position - _targetPosition).sqrMagnitude < 0.0004f) _following = false;
        }

        // 부모(머리)가 움직여도 월드에 고정 — 모든 이동이 끝난 뒤 적용
        private void LateUpdate()
        {
            transform.SetPositionAndRotation(_position, _rotation);
        }
    }
}
