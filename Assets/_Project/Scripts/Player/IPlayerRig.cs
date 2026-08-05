using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 게임 로직이 참조하는 플레이어 리그 인터페이스.
    /// 게임 로직은 입력 소스를 몰라야 하며, 이 인터페이스만 통해 플레이어를 다룬다.
    /// </summary>
    public interface IPlayerRig
    {
        /// <summary>HMD(헤드셋) 트랜스폼</summary>
        Transform HeadTransform { get; }

        /// <summary>손에 쥔 무기 위치</summary>
        Transform WeaponSocketTransform { get; }

        /// <summary>컨트롤러/조준 방향</summary>
        Vector3 AimDirection { get; }

        bool IsVR { get; }
    }
}
