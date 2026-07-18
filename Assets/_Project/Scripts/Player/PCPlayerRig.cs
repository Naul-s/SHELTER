using UnityEngine;
using UnityEngine.InputSystem;

namespace Shelter.Player
{
    /// <summary>
    /// PC(1인칭) 플레이어 리그: CharacterController + Input System. XR Interaction Toolkit 미사용 (cross-platform-input 스킬).
    /// 이동/시점은 클라이언트 로컬 표현이라 공유 상태를 건드리지 않는다.
    /// 네트워크(Photon Fusion 2) 도입 시 로컬 소유자에서만 이 컴포넌트를 활성화하는 방식으로 확장한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PCPlayerRig : MonoBehaviour, IPlayerRig
    {
        [SerializeField] private PlayerMovementConfig _config;
        // TODO(fusion): 리그별 InputActionAsset 인스턴스 분리 필요
        [Tooltip("SHELTER.inputactions — 'Player' 맵의 Move/Look/Jump/Sprint 액션 사용")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _weaponSocket;

        public Transform HeadTransform => _head;
        public Transform WeaponSocketTransform => _weaponSocket;
        public Vector3 AimDirection => _head.forward;
        public bool IsVR => false;

        // 애니메이션 드라이버 등 외부에서 읽는 이동 상태 (LateUpdate에서 읽을 것)
        public float PlanarSpeed { get; private set; }
        public float MoveInputMagnitude { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool JumpedThisFrame { get; private set; }
        public float VerticalVelocity => _verticalVelocity;

        private CharacterController _controller;
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _sprint;
        private float _pitch;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            var map = _inputActions.FindActionMap("Player", throwIfNotFound: true);
            _move = map.FindAction("Move", throwIfNotFound: true);
            _look = map.FindAction("Look", throwIfNotFound: true);
            _jump = map.FindAction("Jump", throwIfNotFound: true);
            _sprint = map.FindAction("Sprint", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _inputActions.FindActionMap("Player").Enable();
        }

        private void OnDisable()
        {
            _inputActions.FindActionMap("Player").Disable();
        }

        // TODO(fusion): HasInputAuthority 체크 필요
        private void Update()
        {
            UpdateLook();
            UpdateMove();
        }

        private void UpdateLook()
        {
            // raw 마우스 델타 기준. 마우스 위(+y) = 위 보기(피치 감소)가 기본, invertY로 뒤집기 가능.
            Vector2 look = _look.ReadValue<Vector2>() * _config.lookSensitivity;
            float pitchDelta = _config.invertY ? look.y : -look.y;
            transform.Rotate(0f, look.x, 0f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, -_config.pitchClamp, _config.pitchClamp);
            _head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMove()
        {
            Vector2 input = _move.ReadValue<Vector2>();
            float speed = _sprint.IsPressed() ? _config.sprintSpeed : _config.walkSpeed;
            Vector3 planar = (transform.right * input.x + transform.forward * input.y) * speed;

            IsGrounded = _controller.isGrounded;
            JumpedThisFrame = false;
            if (IsGrounded)
            {
                _verticalVelocity = -2f; // 경사면 접지 유지용 소량 하향
                if (_jump.WasPressedThisFrame())
                {
                    _verticalVelocity = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity);
                    JumpedThisFrame = true;
                }
            }
            _verticalVelocity += _config.gravity * Time.deltaTime;

            _controller.Move((planar + Vector3.up * _verticalVelocity) * Time.deltaTime);

            MoveInputMagnitude = Mathf.Clamp01(input.magnitude);
            PlanarSpeed = speed * MoveInputMagnitude;
        }
    }
}
