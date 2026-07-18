using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// PCPlayerRig의 이동 상태를 캐릭터 모델 Animator(StarterAssetsThirdPerson 파라미터 규격)로 전달한다.
    /// 파라미터: Speed(float), MotionSpeed(float), Grounded(bool), Jump(bool), FreeFall(bool).
    /// 애니메이션은 클라이언트 로컬 표현이므로 공유 상태를 건드리지 않는다.
    /// </summary>
    public class PlayerAnimationDriver : MonoBehaviour
    {
        [SerializeField] private PCPlayerRig _rig;
        [SerializeField] private Animator _animator;
        [Tooltip("Speed 파라미터 감쇠 시간(초) — 블렌드 트리 전환을 부드럽게")]
        [SerializeField] private float _speedDampTime = 0.1f;
        [Tooltip("이 낙하 속도(음수, m/s)보다 빠르게 떨어지면 FreeFall 상태로 전환")]
        [SerializeField] private float _freeFallVelocityThreshold = -4f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");

        // 리그(PCPlayerRig.Update)가 상태를 갱신한 뒤에 읽도록 LateUpdate 사용
        private void LateUpdate()
        {
            if (_rig == null || _animator == null)
                return;

            _animator.SetFloat(SpeedHash, _rig.PlanarSpeed, _speedDampTime, Time.deltaTime);
            // 키보드(디지털) 입력 기준 항상 1 — 0이면 블렌드 트리 재생 속도가 0이 되어 대기 모션까지 멈춘다
            _animator.SetFloat(MotionSpeedHash, 1f);
            _animator.SetBool(GroundedHash, _rig.IsGrounded);

            if (_rig.JumpedThisFrame)
                _animator.SetBool(JumpHash, true);
            if (_rig.IsGrounded)
            {
                _animator.SetBool(JumpHash, false);
                _animator.SetBool(FreeFallHash, false);
            }
            else if (_rig.VerticalVelocity < _freeFallVelocityThreshold)
            {
                _animator.SetBool(FreeFallHash, true);
            }
        }
    }
}
