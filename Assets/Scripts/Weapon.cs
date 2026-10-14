using System;
using System.Collections.Generic;
using Shelter.Core;
using Shelter.Player;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Weapon : MonoBehaviour
{
    [SerializeField] private bool isGrab = false;
    [SerializeField] private PlayerAttackStats attackStats;

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
    private Dictionary<IDamageable, float> hitHistory = new Dictionary<IDamageable, float>();
    [Tooltip("무기 데미지")]
    [SerializeField] private int damage = 0;
    [Tooltip("무기 티어")]
    [SerializeField] private int tier = 0;

    [Header("Haptic")]
    [SerializeField] private float hapticAmplitude = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;

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
        attackStats = arg0.interactorObject.transform.GetComponentInParent<PlayerAttackStats>();
        if (slot == null)
        xRGrabInteractable.throwOnDetach = true;
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

        attackStats = null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isCollisionState == true) return;
        isCollisionState = true;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null && !(target is PlayerHealth) && !target.IsDead && currentVelocity.magnitude >= trueDamageSpeed)
        {
            if (hitHistory.ContainsKey(target))
            {
                float lastHitTime = hitHistory[target];
                if (Time.time - lastHitTime < hitCooldown)
                {
                    return;
                }
            }

            target.TakeDamage(new DamageInfo(Calculate_Damage(), attackStats != null ? attackStats.gameObject : gameObject, other.ClosestPoint(transform.position)));// 대상에게 데미지 주기
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

    float Calculate_Damage()
    {
        return damage + (attackStats != null ? attackStats.AttackPower : 0f);
    }
}
