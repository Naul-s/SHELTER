using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRController : MonoBehaviour
{
    [Header("오른쪽 컨트롤러")]
    [SerializeField] private InputActionProperty right_btn_A;
    [SerializeField] private InputActionProperty right_btn_B;
    [SerializeField] private InputActionProperty right_stick;

    [SerializeField] private InputActionProperty right_btn_front; public event Action On_right_btn_front;
    [SerializeField] private InputActionProperty right_btn_side;
    [SerializeField] private InputActionProperty right_btn_front_value;
    [SerializeField] private InputActionProperty right_btn_side_value;
    [SerializeField] private Transform[] rightHandBone;
    [SerializeField] private Vector3[] right_base;
    [SerializeField] private Vector3[] right_front_grab;
    [SerializeField] private Vector3[] right_side_grab;

    [Header("왼쪽 컨트롤러")]
    [SerializeField] private InputActionProperty left_btn_X;
    [SerializeField] private InputActionProperty left_btn_Y;
    [SerializeField] private InputActionProperty left_stick;
    [SerializeField] private InputActionProperty left_btn_menu;

    [SerializeField] private InputActionProperty left_btn_front; public event Action On_left_btn_front;
    [SerializeField] private InputActionProperty left_btn_side;
    [SerializeField] private InputActionProperty left_btn_front_value;
    [SerializeField] private InputActionProperty left_btn_side_value;
    [SerializeField] private Transform[] leftHandBone;
    [SerializeField] private Vector3[] left_base;
    [SerializeField] private Vector3[] left_front_grab;
    [SerializeField] private Vector3[] left_side_grab;

    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;




    [SerializeField] private GameObject UI_SKill;
    [SerializeField] private GameObject UI_Inventory;

    void Awake()
    {
        UI_SKill.gameObject.SetActive(false);
        UI_Inventory.gameObject.SetActive(false);
    }

    void Update()
    {
        if (right_btn_A.action != null && right_btn_A.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 A키 입력");
        }
        if (right_btn_B.action != null && right_btn_B.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 B키 입력");
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
            On_right_btn_front?.Invoke();
            for (int i = 0; i < rightHandBone.Length; i++)
            {
                right_front_grab[i] = rightHandBone[i].localEulerAngles;
            }
        }
        //if (right_btn_front_value.action != null)
        //{
        //    float inputValue = right_btn_front_value.action.ReadValue<float>();
        //    for (int i = 0; i < 3; i++)// 검지손가락만
        //    {
        //        Vector3 baseRot = right_base[i];
        //        Vector3 grabRot = right_front_grab[i];
        //
        //        Vector3 result = new Vector3(
        //            Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
        //            Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
        //            Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
        //        );
        //
        //        rightHandBone[i].localEulerAngles = result;
        //    }
        //
        //    //Debug.Log("오른쪽 정면키 입력값" + inputValue);
        //}
        if (right_btn_side.action != null && right_btn_side.action.WasPressedThisFrame())
        {
            //Debug.Log("오른쪽 측면키 입력");
        }
        //if (right_btn_side_value.action != null)
        //{
        //    float inputValue = right_btn_side_value.action.ReadValue<float>();
        //    for (int i = 3; i < rightHandBone.Length; i++)// 검지손가락 제외
        //    {
        //        Vector3 baseRot = right_base[i];
        //        Vector3 grabRot = right_side_grab[i];
        //
        //        Vector3 result = new Vector3(
        //            Mathf.LerpAngle(baseRot.x, grabRot.x, inputValue),
        //            Mathf.LerpAngle(baseRot.y, grabRot.y, inputValue),
        //            Mathf.LerpAngle(baseRot.z, grabRot.z, inputValue)
        //        );
        //
        //        rightHandBone[i].localEulerAngles = result;
        //    }
        //    //Debug.Log("오른쪽 측면키 입력값" + inputValue);
        //}




        if (left_btn_X.action != null && left_btn_X.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 X키 입력");

            // Skill 창 활성화
            Vector3 spawnPosition = mainCamera.position + (mainCamera.forward * 0.8f);
            spawnPosition.y += -0.2f;
            UI_SKill.transform.position = spawnPosition;
            UI_SKill.transform.rotation = mainCamera.rotation;

            UI_SKill.gameObject.SetActive(!UI_SKill.gameObject.activeSelf);
        }
        if (left_btn_Y.action != null && left_btn_Y.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 Y키 입력");

            // Inventory 창 활성화
            Vector3 spawnPosition = mainCamera.position + (mainCamera.forward * 0.8f);
            spawnPosition.y += -0.2f;
            UI_Inventory.transform.position = spawnPosition;
            UI_Inventory.transform.rotation = mainCamera.rotation;

            UI_Inventory.gameObject.SetActive(!UI_Inventory.gameObject.activeSelf);
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
        }

        if (left_btn_front.action != null && left_btn_front.action.WasPressedThisFrame())
        {
            //Debug.Log("왼쪽 정면키 입력");
            On_left_btn_front?.Invoke();
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
    }
}