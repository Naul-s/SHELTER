using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// VR 플레이어 리그. 이동/회전/텔레포트는 XR Interaction Toolkit 로코모션 프로바이더가 담당하고
    /// (VR 템플릿 Complete XR Origin Set Up: 스무스 이동 + 스냅 턴 + 텔레포트 + 이동 중 터널링 비네트 — vr-performance-comfort 준수),
    /// 이 컴포넌트는 게임 로직에 IPlayerRig 관점만 노출한다.
    /// </summary>
    public class VRPlayerRig : MonoBehaviour, IPlayerRig
    {
        [Tooltip("XR Origin의 Main Camera(HMD)")]
        [SerializeField] private Transform _head;
        [Tooltip("무기/도구 기준점 — 기본은 오른손 컨트롤러")]
        [SerializeField] private Transform _weaponSocket;

        public Transform HeadTransform => _head;
        public Transform WeaponSocketTransform => _weaponSocket;
        public Vector3 AimDirection => _weaponSocket != null ? _weaponSocket.forward : _head.forward;
        public bool IsVR => true;
    }
}
