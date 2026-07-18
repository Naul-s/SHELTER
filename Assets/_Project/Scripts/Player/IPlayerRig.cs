using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 게임 로직이 참조하는 플랫폼 공통 플레이어 리그 인터페이스 (cross-platform-input 스킬).
    /// 게임 로직은 입력 소스(VR/PC)를 몰라야 하며, 이 인터페이스만 통해 플레이어를 다룬다.
    /// </summary>
    public interface IPlayerRig
    {
        /// <summary>VR: HMD, PC: 1인칭 카메라</summary>
        Transform HeadTransform { get; }

        /// <summary>VR: 손에 쥔 무기 위치, PC: 화면 하단 무기 모델 소켓</summary>
        Transform WeaponSocketTransform { get; }

        /// <summary>VR: 컨트롤러 방향, PC: 카메라 forward</summary>
        Vector3 AimDirection { get; }

        bool IsVR { get; }
    }
}
