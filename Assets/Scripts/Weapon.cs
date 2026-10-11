using RootMotion.Demos;
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
    [Tooltip("총이면 체크")]
    [SerializeField] private bool isGun;
    [Tooltip("flase = 단발, true = 단발<->연사 전환가능 / 총 소환시 기본 상태는 단발")]
    [SerializeField] private bool isAutoChange = false; private bool isAuto = false;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float shotColldown = 0.1f;
    private float lastShotTime;
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bullet;
    [SerializeField] private AudioSource shotSound;

    bool isRight;

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
                isRight = false;
                if (isAuto == false) vrController.On_left_btn_front += Shot;
                else vrController.On_left_btn_front_Update += Shot;

                vrController.On_left_btn_X += ReLoad;
                vrController.On_left_btn_Y += AutoChange;
            }
            else if (arg0.interactorObject.transform.gameObject.CompareTag("Right"))
            {
                isRight = true;
                if (isAuto == false) vrController.On_right_btn_front += Shot;
                else vrController.On_right_btn_front_Update += Shot;

                vrController.On_right_btn_A += ReLoad;
                vrController.On_right_btn_B += AutoChange;
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
            vrController.On_left_btn_front_Update -= Shot;
            vrController.On_left_btn_X -= ReLoad;
            vrController.On_left_btn_Y -= AutoChange;
            vrController.On_right_btn_front -= Shot;
            vrController.On_right_btn_front_Update -= Shot;
            vrController.On_right_btn_A -= ReLoad;
            vrController.On_right_btn_B -= AutoChange;
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
        if (MathF.Abs(Time.time - lastShotTime) < shotColldown) return;

        Ray ray = new Ray(muzzle.position, muzzle.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Debug.Log($"맞은 대상: {hit.collider.name}, 위치: {hit.point}");
            GameObject go = Instantiate(bullet, hit.point, muzzle.rotation);
            Destroy(go, 10f);
        }

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 2f);

        SendHaptic();
        shotSound.PlayOneShot(shotSound.clip);


        lastShotTime = Time.time;
    }

    void AutoChange()
    {
        if (isAutoChange == false) return;

        isAuto = !isAuto;

        vrController.On_left_btn_front -= Shot;
        vrController.On_right_btn_front -= Shot;
        vrController.On_left_btn_front_Update -= Shot;
        vrController.On_right_btn_front_Update -= Shot;

        if (isAuto == false)
        {
            if (isRight == false)
            {
                vrController.On_left_btn_front += Shot;
            }
            else
            {
                vrController.On_right_btn_front += Shot;
            }
        }
        else
        {
            if (isRight == false)
            {
                vrController.On_left_btn_front_Update += Shot;
            }
            else
            {
                vrController.On_right_btn_front_Update += Shot;
            }
        }
    }

    void ReLoad()
    {

    }
}
