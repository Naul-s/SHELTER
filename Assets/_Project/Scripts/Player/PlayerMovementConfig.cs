using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// PC(1인칭) 이동/시점 밸런싱 수치. 전부 플레이테스트 후 조정 대상.
    /// VR 이동 수치는 XR 로코모션 프로바이더(Complete XR Origin Set Up) 인스펙터에서 관리한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Shelter/Player Movement Config", fileName = "PlayerMovementConfig")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("이동")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 6f;
        public float jumpHeight = 1.2f;
        public float gravity = -15f;

        [Header("시점")]
        [Tooltip("마우스 델타 1px당 회전 각도(도). 입력 액션에 스케일/반전 프로세서가 없는 raw delta 기준")]
        public float lookSensitivity = 0.08f;
        public float pitchClamp = 89f;
        [Tooltip("체크 시 마우스 위 = 아래 보기 (플라이트 시뮬 스타일)")]
        public bool invertY = false;
    }
}
