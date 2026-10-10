using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Weapon : MonoBehaviour
{
    [SerializeField] private bool isGrab = false;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private VRController vrController;

    private XRGrabInteractable xRGrabInteractable;
    private Rigidbody rigid;
    [SerializeField] private EquipmentSlot slot;
    [SerializeField] private bool isCollisionState = false;// 하위에 여러개의 콜라이더가 있을 시 충돌횟수를 1회로 제한하기 위한 용도 포함

    private Vector3 previousPosition;
    private Vector3 currentVelocity;

    [Header("무기 설정")]
    [Tooltip("움직임이 해당 속도 이상일 시 데미지가 들어감")]
    [SerializeField] private float trueDamageSpeed = 2.0f;
    [Tooltip("동일 대상 재타격 쿨타임")]
    [SerializeField] private float hitCooldown = 0.3f;
    private Dictionary<IDamagable, float> hitHistory = new Dictionary<IDamagable, float>();
    [Tooltip("무기 데미지")]
    [SerializeField] private int damage = 0;
    [Tooltip("무기 티어")]
    [SerializeField] private int tier = 0;
    [Tooltip("무기 무게")]
    [SerializeField] private float weight = 0;

    [Header("Haptic")]
    [SerializeField] private float hapticAmplitude = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;


    [Header("총 설정")]
    [SerializeField] private bool isGun;
    [SerializeField] private Transform bulletPos;
    [SerializeField] private GameObject bullet;

    private void Start()
    {
        xRGrabInteractable = GetComponentInParent<XRGrabInteractable>();
        rigid = GetComponentInParent<Rigidbody>();

        xRGrabInteractable.selectEntered.AddListener(OnSelectEnterd);
        xRGrabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void Update()
    {
        if (isGrab)
        {
            currentVelocity = (transform.position - previousPosition) / Time.deltaTime;
            previousPosition = transform.position;
        }
    }

    private void OnSelectEnterd(SelectEnterEventArgs arg0)
    {
        isGrab = true;
        playerStats = arg0.interactorObject.transform.GetComponentInParent<PlayerStats>();
        if (slot == null)
        xRGrabInteractable.throwOnDetach = true;

        if (isGun)
        {
            vrController = playerStats.transform.GetComponentInParent<VRController>();
            if (arg0.interactorObject.transform.gameObject.CompareTag("Left"))
            {
                vrController.On_left_btn_front += Shot;
            }
            else if (arg0.interactorObject.transform.gameObject.CompareTag("Right"))
            {
                vrController.On_right_btn_front += Shot;
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs arg0)
    {
        isGrab = false;

        if (isCollisionState == true)// 총돌 범위 안에 들어와있으면서
        {
            if (slot != null && slot.weapon == this)// 무기 슬롯일 경우 && 장착된 무기가 나 일때
            {
                xRGrabInteractable.throwOnDetach = false;
                rigid.isKinematic = true;

                transform.SetParent(slot.attachPoint);
                transform.SetPositionAndRotation(slot.attachPoint.position, slot.attachPoint.rotation);
            }
            if (slot == null)
                rigid.isKinematic = false;
        }
        else
        {
            rigid.isKinematic = false;
            hitHistory.Clear();
        }

        playerStats = null;

        if (isGun)
        {
            vrController.On_left_btn_front -= Shot;
            vrController.On_right_btn_front -= Shot;
            vrController = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isCollisionState == true) return;
        isCollisionState = true;

        IDamagable target = other.transform.GetComponent<IDamagable>();
        if (target != null && currentVelocity.magnitude >= trueDamageSpeed)
        {
            if (hitHistory.ContainsKey(target))
            {
                float lastHitTime = hitHistory[target];
                if (Time.time - lastHitTime < hitCooldown)
                {
                    return;
                }
            }

            target.TakeDamage(Calculate_Damage());// 대상에게 데미지 주기
            hitHistory[target] = Time.time;
            SendHaptic();

        }

        slot = other.GetComponentInParent<EquipmentSlot>();
        if (slot != null && slot.weapon == null)
        {
            slot.weapon = this;
            SendHaptic();
        }
    }
    private void OnTriggerStay(Collider other)// 무기 슬롯 위치에서 잡았다가 바로 놓아도 슬롯 안으로 들어가도록
    {
        if (isCollisionState == true) return;
        isCollisionState = true;

        slot = other.GetComponentInParent<EquipmentSlot>();
        if (slot != null && slot.weapon == null)
            slot.weapon = this;
    }
    private void OnTriggerExit(Collider other)
    {
        if (isCollisionState == false) return;
        isCollisionState = false;

        if (slot != null && slot.weapon == this)// 무기 슬롯일 경우 && 장착된 무기가 나 일때
        {
            slot.weapon = null;
            slot = null;
        }
    }

    /// <summary> 진동 </summary>
    private void SendHaptic()
    {
        if (xRGrabInteractable.interactorsSelecting.Count == 0)
            return;
        
        IXRSelectInteractor interactor = xRGrabInteractable.interactorsSelecting[0];
        
        if (interactor is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(
                hapticAmplitude,
                hapticDuration
            );
        }
    }

    int Calculate_Damage()
    {
        if (playerStats != null)
        {
            return (int)(playerStats.TotalStats.STR * 0.3f) + damage;
        }

        return 0;
    }

    void Shot()
    {
        Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        SendHaptic();
        Debug.Log("사격");
    }
}
