using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// VR 아바타 바디의 양손을 XR 컨트롤러 위치로 붙이는 팔 IK.
    /// Humanoid 아바타는 Animation Rigging(커스텀 Rig 트랜스폼을 애니메이션 스트림에서 못 찾음)과
    /// 호환되지 않으므로, Unity 내장 Humanoid IK(<see cref="Animator.OnAnimatorIK"/>)를 사용한다.
    /// 로코모션 클립/컨트롤러(StarterAssetsThirdPerson)를 그대로 유지하면서 손만 컨트롤러를 추적한다.
    ///
    /// 전제조건: 이 컴포넌트가 붙은 GameObject의 Animator가 쓰는 컨트롤러의 레이어에
    /// **IK Pass가 켜져 있어야** OnAnimatorIK가 호출된다.
    ///
    /// 손은 컨트롤러의 트래킹 원점이 아니라, 컨트롤러 로컬 기준 <see cref="_gripPositionOffset"/>/
    /// <see cref="_gripRotationOffset"/>만큼 보정된 지점(손잡이를 감싸 쥔 위치)을 weight 1로 추종한다.
    /// 왼손은 <see cref="_mirrorForLeftHand"/>가 켜져 있으면 오프셋을 좌우 대칭으로 적용한다.
    /// 오프셋 값은 Scene 뷰에서 컨트롤러-손 관계를 보며 맞춘다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class VRHandIK : MonoBehaviour
    {
        [Tooltip("오른손 IK 타깃 — 미지정 시 부모의 VRPlayerRig.RightController를 자동 사용")]
        [SerializeField] private Transform _rightHandTarget;
        [Tooltip("왼손 IK 타깃 — 미지정 시 부모의 VRPlayerRig.LeftController를 자동 사용")]
        [SerializeField] private Transform _leftHandTarget;

        [Tooltip("손 위치/회전 추종 가중치. VR 바디 프레즌스는 항상 1.")]
        [Range(0f, 1f)]
        [SerializeField] private float _weight = 1f;

        [Header("그립 오프셋 (오른손 컨트롤러 로컬 기준 — 손이 손잡이를 감싸 쥐도록)")]
        [Tooltip("컨트롤러 로컬 좌표계에서의 손 위치 오프셋(m). 손잡이 쪽으로 내려/뒤로 보낸다.")]
        [SerializeField] private Vector3 _gripPositionOffset = new Vector3(0.01f, 0.01f, 0f);
        [Tooltip("컨트롤러 로컬 회전 오프셋(도). 손바닥이 손잡이를 향하도록 손을 돌린다.")]
        [SerializeField] private Vector3 _gripRotationOffset = new Vector3(-18f, 0f, -90f);
        [Tooltip("왼손은 위 오프셋을 좌우 대칭(sagittal mirror)으로 적용")]
        [SerializeField] private bool _mirrorForLeftHand = true;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            // 타깃 미지정 시 부모 리그의 컨트롤러 트랜스폼을 사용(느슨한 결합).
            if (_rightHandTarget == null || _leftHandTarget == null)
            {
                var playerRig = GetComponentInParent<VRPlayerRig>();
                if (playerRig != null)
                {
                    if (_rightHandTarget == null) _rightHandTarget = playerRig.RightController;
                    if (_leftHandTarget == null) _leftHandTarget = playerRig.LeftController;
                }
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null)
                return;

            ApplyHand(AvatarIKGoal.RightHand, _rightHandTarget, false);
            ApplyHand(AvatarIKGoal.LeftHand, _leftHandTarget, true);
        }

        private void ApplyHand(AvatarIKGoal goal, Transform target, bool isLeft)
        {
            if (target == null)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }

            // 그립 오프셋을 컨트롤러 로컬 기준으로 적용. 왼손은 좌우 대칭.
            Vector3 posOffset = _gripPositionOffset;
            Vector3 rotOffset = _gripRotationOffset;
            if (isLeft && _mirrorForLeftHand)
            {
                posOffset = new Vector3(-posOffset.x, posOffset.y, posOffset.z);
                rotOffset = new Vector3(rotOffset.x, -rotOffset.y, -rotOffset.z);
            }

            Vector3 worldPos = target.TransformPoint(posOffset);
            Quaternion worldRot = target.rotation * Quaternion.Euler(rotOffset);

            _animator.SetIKPositionWeight(goal, _weight);
            _animator.SetIKRotationWeight(goal, _weight);
            _animator.SetIKPosition(goal, worldPos);
            _animator.SetIKRotation(goal, worldRot);
        }
    }
}
