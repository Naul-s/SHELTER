using System.Net.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRController : MonoBehaviour
{
    [Header("오른쪽 컨트롤러")]
    [SerializeField] private InputActionProperty right_btn_A;
    [SerializeField] private InputActionProperty right_btn_B;
    [SerializeField] private InputActionProperty right_stick;

    [SerializeField] private InputActionProperty right_btn_front;
    [SerializeField] private InputActionProperty right_btn_side;

    [Header("왼쪽 컨트롤러")]
    [SerializeField] private InputActionProperty left_btn_X;
    [SerializeField] private InputActionProperty left_btn_Y;
    [SerializeField] private InputActionProperty left_stick;
    [SerializeField] private InputActionProperty left_btn_menu;

    [SerializeField] private InputActionProperty left_btn_front;
    [SerializeField] private InputActionProperty left_btn_side;

    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    [SerializeField] private GameObject UI_SKill;
    void Awake()
    {
        UI_SKill.gameObject.SetActive(false);
    }

    void Update()
    {
        if (right_btn_A.action != null && right_btn_A.action.WasPressedThisFrame())
        {
            Debug.Log("오른쪽 A키 입력");
        }
        if (right_btn_B.action != null && right_btn_B.action.WasPressedThisFrame())
        {
            Debug.Log("오른쪽 B키 입력");
        }
        if (right_stick.action != null)
        {
            Vector2 inputVector = right_stick.action.ReadValue<Vector2>();

            if (inputVector.magnitude > 0.1f)// 스틱을 일정 이상 밀었을 때만 작동 (Deadzone 처리)
            {
                Debug.Log("오른쪽 스틱 입력" + inputVector);
            }
        }

        if (right_btn_front.action != null && right_btn_front.action.WasPressedThisFrame())
        {
            Debug.Log("오른쪽 정면키 입력");
        }
        if (right_btn_side.action != null && right_btn_side.action.WasPressedThisFrame())
        {
            Debug.Log("오른쪽 측면키 입력");
        }




        if (left_btn_X.action != null && left_btn_X.action.WasPressedThisFrame())
        {
            Debug.Log("왼쪽 X키 입력");

            // Skill 창 활성화
            Vector3 spawnPosition = mainCamera.position + (mainCamera.forward * 0.8f);
            spawnPosition.y += -0.2f;
            UI_SKill.transform.position = spawnPosition;
            UI_SKill.transform.rotation = mainCamera.rotation;

            UI_SKill.gameObject.SetActive(!UI_SKill.gameObject.activeSelf);
        }
        if (left_btn_Y.action != null && left_btn_Y.action.WasPressedThisFrame())
        {
            Debug.Log("왼쪽 Y키 입력");
        }
        if (left_stick.action != null)
        {
            Vector2 inputVector = left_stick.action.ReadValue<Vector2>();

            if (inputVector.magnitude > 0.1f)// 스틱을 일정 이상 밀었을 때만 작동 (Deadzone 처리)
            {
                Debug.Log("왼쪽 스틱 입력" + inputVector);
            }
        }
        if (left_btn_menu.action != null && left_btn_menu.action.WasPressedThisFrame())
        {
            Debug.Log("왼쪽 메뉴키 입력");
        }

        if (left_btn_front.action != null && left_btn_front.action.WasPressedThisFrame())
        {
            Debug.Log("왼쪽 정면키 입력");
        }
        if (left_btn_side.action != null && left_btn_side.action.WasPressedThisFrame())
        {
            Debug.Log("왼쪽 측면키 입력");
        }
    }
}
