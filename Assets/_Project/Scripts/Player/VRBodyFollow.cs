using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// VR 아바타 바디를 카메라(HMD) 아래로 따라 붙이는 바디-팔로우 드라이버.
    /// CharacterModel(=Animator 보유) 루트에 부착하며, 팔 IK(<see cref="VRHandIK"/>의 OnAnimatorIK)는
    /// 애니메이터 업데이트 중 실행되므로 그 이후인 LateUpdate에서 루트 위치/회전을 카메라에 정렬한다.
    ///
    /// - 수평(X·Z): 매 프레임 카메라 위치로 맞춰 몸이 머리 밑에 오도록(Y는 아래 높이 정렬만 반영).
    /// - Yaw만 추종: 카메라 좌우 방향만 부드럽게 따라가고 Pitch/Roll은 무시(고개를 숙이거나 기울여도
    ///   몸 전체가 같이 기울지 않도록).
    /// - 높이 정렬: 머리 본이 항상 카메라 높이에 오도록 루트 Y를 보정. 하드코딩 신장값 없이
    ///   실제 신장/자세에 자동 대응(앉거나 자세를 낮추면 몸도 함께 낮아진다).
    ///
    /// 참고: WASD/스틱 로코모션은 XR 리그 전체(카메라·컨트롤러·이 바디)를 함께 이동시키므로
    /// 이 드라이버는 "머리가 리그 기준으로 상대 이동"할 때만 루트를 움직인다 → 로코모션 중
    /// 손 IK가 컨트롤러에서 밀리지 않는다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class VRBodyFollow : MonoBehaviour
    {
        [Tooltip("따라갈 카메라(HMD). 미지정 시 부모 VRPlayerRig.HeadTransform 자동 사용")]
        [SerializeField] private Transform _head;

        [Tooltip("몸 Yaw가 시선 방향을 따라가는 부드러움(초). 작을수록 즉각적, 클수록 느긋. 0이면 즉시 붙음.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _yawSmoothTime = 0.12f;

        private Animator _animator;
        private Transform _headBone;
        private float _yawVelocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator != null)
                _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);

            if (_head == null)
            {
                var rig = GetComponentInParent<VRPlayerRig>();
                if (rig != null)
                    _head = rig.HeadTransform;
            }
        }

        private void LateUpdate()
        {
            if (_head == null)
                return;

            // 2) Yaw만 추종(Pitch/Roll 무시) — 시선 방향을 수평면에 투영해 부드럽게 회전.
            Vector3 headForward = _head.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.0001f)
            {
                float targetYaw = Quaternion.LookRotation(headForward, Vector3.up).eulerAngles.y;
                float currentYaw = transform.eulerAngles.y;
                float newYaw = _yawSmoothTime > 0f
                    ? Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _yawVelocity, _yawSmoothTime)
                    : targetYaw;
                transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
            }

            // 3) 높이 정렬: 머리 본이 카메라 높이에 오도록 루트 Y 보정(1프레임 수렴).
            float yCorrection = 0f;
            if (_headBone != null)
                yCorrection = _head.position.y - _headBone.position.y;

            // 1) 수평 위치: 카메라 X·Z로. Y는 위 높이 보정만 반영.
            transform.position = new Vector3(
                _head.position.x,
                transform.position.y + yCorrection,
                _head.position.z);
        }
    }
}
