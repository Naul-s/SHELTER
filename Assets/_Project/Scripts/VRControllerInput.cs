using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class VRController : MonoBehaviour
{
    [Header("오른쪽 컨트롤러")]
    [SerializeField] private InputActionProperty right_btn_A; public event Action On_right_btn_A;
    [SerializeField] private InputActionProperty right_btn_B; public event Action On_right_btn_B;
    [SerializeField] private InputActionProperty right_stick;
    public Vector2 RightStickInput =>
    right_stick.action != null
    ? right_stick.action.ReadValue<Vector2>()
    : Vector2.zero;

    [SerializeField] private InputActionProperty right_btn_front; public event Action On_right_btn_front; private bool isRight_btn_front_Input;// 단발 구현 용도
    [SerializeField] private InputActionProperty right_btn_side;
    [SerializeField] private InputActionProperty right_btn_front_value; public event Action On_right_btn_front_Update;
    [SerializeField] private InputActionProperty right_btn_side_value;

    [SerializeField] private Transform[] rightHandBone;
    [SerializeField] private Vector3[] right_base;
    [SerializeField] private Vector3[] right_front_grab;
    [SerializeField] private Vector3[] right_side_grab;

    [Header("왼쪽 컨트롤러")]
    [SerializeField] private InputActionProperty left_btn_X; public event Action On_left_btn_X;
    [SerializeField] private InputActionProperty left_btn_Y; public event Action On_left_btn_Y;
    [SerializeField] private InputActionProperty left_stick;
    public Vector2 LeftStickInput =>
    left_stick.action != null
    ? left_stick.action.ReadValue<Vector2>()
    : Vector2.zero;
    [SerializeField] private InputActionProperty left_btn_menu; public event Action On_left_btn_menu;// 게임 메뉴

    [SerializeField] private InputActionProperty left_btn_front; public event Action On_left_btn_front; private bool isLeft_btn_front_Input;// 단발 구현 용도
    [SerializeField] private InputActionProperty left_btn_side;
    [SerializeField] private InputActionProperty left_btn_front_value; public event Action On_left_btn_front_Update;
    [SerializeField] private InputActionProperty left_btn_side_value;
    [SerializeField] private Transform[] leftHandBone;
    [SerializeField] private Vector3[] left_base;
    [SerializeField] private Vector3[] left_front_grab;
    [SerializeField] private Vector3[] left_side_grab;

    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    [SerializeField] private GameMenuUI gameOverUI;

    public event Action On_DifficultyCustom;// 난이도 커스텀 창

    void Update()
    {
        if (right_btn_A.action != null && right_btn_A.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 A키 입력");
            On_right_btn_A?.Invoke();
        }
        if (right_btn_B.action != null && right_btn_B.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 B키 입력");
            On_right_btn_B?.Invoke();
        }
        if (right_stick.action != null)
        {
            Vector2 inputVector = right_stick.action.ReadValue<Vector2>();

            if (inputVector.magnitude > 0.1f)// 스틱을 일정 이상 밀었을 때만 작동 (Deadzone 처리)
            {
                //Debug.Log("오른쪽 스틱 입력" + inputVector);
            }
        }

        if (right_btn_front.action != null && right_btn_front.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 정면키 입력");
            //for (int i = 0; i < rightHandBone.Length; i++)
            //{
            //    right_side_grab[i] = rightHandBone[i].localEulerAngles;
            //}
        }
        if (right_btn_front_value.action != null)
        {
            float inputValue = right_btn_front_value.action.ReadValue<float>();
            for (int i = 0; i < 3; i++)// 검지손가락만
            {
                Vector3 baseRot = right_base[i];
                Vector3 grabRot = right_front_grab[i];
        
                Vector3 result = new Vector3(
                    Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
                    Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
                    Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
                );
        
                rightHandBone[i].localEulerAngles = result;
            }

            if (inputValue > 0.8f)
            {
                On_right_btn_front_Update?.Invoke();

                if (isRight_btn_front_Input == false)
                {
                    isRight_btn_front_Input = true;
                    On_right_btn_front?.Invoke();
                }
            }
            else
            {
                isRight_btn_front_Input = false;
            }

            //Debug.Log("오른쪽 정면키 입력값" + inputValue);
        }
        if (right_btn_side.action != null && right_btn_side.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 측면키 입력");
        }
        if (right_btn_side_value.action != null)
        {
            float inputValue = right_btn_side_value.action.ReadValue<float>();
            for (int i = 3; i < rightHandBone.Length; i++)// 검지손가락 제외
            {
                Vector3 baseRot = right_base[i];
                Vector3 grabRot = right_side_grab[i];
        
                Vector3 result = new Vector3(
                    Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
                    Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
                    Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
                );
        
                rightHandBone[i].localEulerAngles = result;
            }
            //Debug.Log("오른쪽 측면키 입력값" + inputValue);
        }




        if (left_btn_X.action != null && left_btn_X.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 X키 입력");

            On_left_btn_X?.Invoke();
        }
        if (left_btn_Y.action != null && left_btn_Y.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 Y키 입력");

            On_left_btn_Y?.Invoke();
        }
        if (left_stick.action != null)
        {
            Vector2 inputVector = left_stick.action.ReadValue<Vector2>();

            if (inputVector.magnitude > 0.1f)// 스틱을 일정 이상 밀었을 때만 작동 (Deadzone 처리)
            {
                //Debug.Log("왼쪽 스틱 입력" + inputVector);
            }
        }
        if (left_btn_menu.action != null && left_btn_menu.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 메뉴키 입력");
            if (gameOverUI != null && gameOverUI.isDefeated == false)//게임오버가 되지 않았을 때
            {
                gameOverUI.MenuSetActive();
            }
        }

        if (left_btn_front.action != null && left_btn_front.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 정면키 입력");
            //for (int i = 0; i < leftHandBone.Length; i++)
            //{
            //    left_base[i] = leftHandBone[i].localEulerAngles;
            //}
        }
        if (left_btn_front_value.action != null)
        {
            float inputValue = left_btn_front_value.action.ReadValue<float>();
            for (int i = 0; i < 3; i++)// 검지손가락만
            {
                Vector3 baseRot = left_base[i];
                Vector3 grabRot = left_front_grab[i];
        
                Vector3 result = new Vector3(
                    Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
                    Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
                    Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
                );
        
                leftHandBone[i].localEulerAngles = result;
            }

            if (inputValue > 0.8f)
            {
                On_left_btn_front_Update?.Invoke();

                if (isLeft_btn_front_Input == false)
                {
                    isLeft_btn_front_Input = true;
                    On_left_btn_front?.Invoke();
                }
            }
            else
            {
                isLeft_btn_front_Input = false;
            }


            //Debug.Log("왼쪽 정면키 입력값" + inputValue);
        }
        if (left_btn_side.action != null && left_btn_side.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 측면키 입력");

        }
        if (left_btn_side_value.action != null)
        {
            float inputValue = left_btn_side_value.action.ReadValue<float>();
            for (int i = 3; i < leftHandBone.Length; i++)// 검지손가락 제외
            {
                Vector3 baseRot = left_base[i];
                Vector3 grabRot = left_side_grab[i];
        
                Vector3 result = new Vector3(
                    Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
                    Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
                    Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
                );
        
                leftHandBone[i].localEulerAngles = result;
            }
            //Debug.Log("왼쪽 측면키 입력값" + inputValue);
        }




        if (right_btn_front_value.action != null && right_stick.action != null && (left_btn_Y.action != null && left_btn_Y.action.WasPressedThisFrame()))// 오른쪽 정면 키+오른쪽 스틱 아래로+왼쪽 Y키
        {
            Vector2 inputVector = right_stick.action.ReadValue<Vector2>();

            if (inputVector.y < 0.9f)
            {
                On_DifficultyCustom?.Invoke();
            }
        }
    }
}