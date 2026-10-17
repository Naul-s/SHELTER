using System.Collections;
using System.Collections.Generic;
using Shelter.Core;
using Shelter.Player;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Weapon : MonoBehaviour
{
    // ─────────────────────────────────────
    // References
    // ─────────────────────────────────────

    [Header("참조")]
    [SerializeField] private PlayerAttackStats attackStats;
    [SerializeField] private VRController vrController;// 현재 주 손에 연결된 컨트롤러
    [SerializeField] private EquipmentSlot slot;

    private XRGrabInteractable xRGrabInteractable;
    private Rigidbody rigid;


    // ─────────────────────────────────────
    // State
    // ─────────────────────────────────────

    [Header("상태")]
    [SerializeField] private bool isGrab = false;// 적어도 한 손이 총을 잡고 있는지 나타내는 상태
    [SerializeField] private bool isCollisionState = false;// 하위에 여러개의 콜라이더가 있을 시 충돌횟수를 1회로 제한하기 위한 용도 포함

    private Vector3 previousPosition;
    private Vector3 currentVelocity;

    private bool isRight;// 현재 주 손이 오른손인지 왼손인지 구분

    private IXRSelectInteractor primaryInteractor;// 현재 총기 조작 권한을 가진 손


    // ─────────────────────────────────────
    // Weapon Settings
    // ─────────────────────────────────────

    [Header("무기 설정")]

    [Tooltip("움직임이 해당 속도 이상일 때 데미지 적용")]
    [SerializeField] private float trueDamageSpeed = 2.0f;

    [Tooltip("동일 대상 재타격 쿨타임")]
    [SerializeField] private float hitCooldown = 0.3f;

    [Tooltip("무기 데미지")]
    [SerializeField] private int damage = 0;

    [Tooltip("무기 티어")]
    [SerializeField] private int tier = 0;

    private Dictionary<IDamageable, float> hitHistory =
        new Dictionary<IDamageable, float>();


    // ─────────────────────────────────────
    // Haptic
    // ─────────────────────────────────────

    [Header("근접 공격 진동")]

    [SerializeField] private float hapticAmplitude = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;


    // ─────────────────────────────────────
    // Gun Settings
    // ─────────────────────────────────────

    [Header("총 설정")]

    [Tooltip("총이면 체크")]
    [SerializeField] private bool isGun;

    [Tooltip("false = 단발 고정, true = 단발/연사 전환 가능")]
    [SerializeField] private bool isAutoChange = false;

    private bool isAuto = false;// 단발·연사 모드 상태

    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float shotColldown = 0.1f;

    private float lastShotTime = float.NegativeInfinity;

    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bullet;
    [SerializeField] private AudioSource shotSound;

    [SerializeField] private Transform slidePos;
    private Vector3 baseSlidePos;
    [SerializeField] private Vector3 shotSlidePos;
    [SerializeField] private Transform casingPos;

    private int bulletCount;

    // 사격 레이캐스트 대상: 기본 레이어에서 플레이어 차단벽(PlayerBlocker)을 뺀다 — 창문 판자 너머 좀비를 쏠 수 있게.
    private int shotLayerMask;

    // ─────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────

    private void Start()
    {
        xRGrabInteractable =
            GetComponentInParent<XRGrabInteractable>();

        rigid =
            GetComponentInParent<Rigidbody>();

        shotLayerMask =
            Physics.DefaultRaycastLayers & ~LayerMask.GetMask("PlayerBlocker");

        if (xRGrabInteractable == null)
        {
            Debug.LogError(
                $"{name}: XRGrabInteractable을 찾을 수 없습니다.",
                this
            );
            return;
        }

        xRGrabInteractable.selectEntered.AddListener(OnSelectEntered);
        xRGrabInteractable.selectExited.AddListener(OnSelectExited);

        if (slidePos != null)
            baseSlidePos = slidePos.transform.localPosition;
    }

    private void OnDestroy()
    {
        if (xRGrabInteractable != null)
        {
            xRGrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            xRGrabInteractable.selectExited.RemoveListener(OnSelectExited);
        }

        UnsubscribeGunControls();
    }


    // ─────────────────────────────────────
    // Update / Velocity
    // ─────────────────────────────────────

    private void Update()
    {
        if (!isGrab)
            return;

        currentVelocity =
            (transform.position - previousPosition) / Time.deltaTime;

        previousPosition = transform.position;
    }


    // ─────────────────────────────────────
    // Grab Events
    // ─────────────────────────────────────

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isGrab = true;

        // 아직 주 손이 없을 때만 조작 권한 부여
        if (primaryInteractor == null)
        {
            SetPrimaryInteractor(args.interactorObject);
        }
        // 이미 주 손이 있다면 새로 잡은 손은 보조 손
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        bool wasPrimary =
            object.ReferenceEquals(primaryInteractor, args.interactorObject);

        // 놓은 손을 제외하고 아직 잡고 있는 손 찾기
        IXRSelectInteractor remainingInteractor =
            FindRemainingInteractor(args.interactorObject);

        // 주 손이 놓았다면 기존 입력 연결 해제
        if (wasPrimary)
        {
            UnsubscribeGunControls();
            primaryInteractor = null;
        }

        // 다른 손이 여전히 잡고 있다면 완전히 놓은 것이 아님
        if (remainingInteractor != null)
        {
            isGrab = true;

            // 주 손이 놓았을 때만 남은 손으로 권한 이전
            if (wasPrimary)
            {
                SetPrimaryInteractor(remainingInteractor);
            }

            return;
        }

        // 더 이상 잡고 있는 손이 없음
        UnsubscribeGunControls();
        primaryInteractor = null;

        FinishRelease();
    }

    private IXRSelectInteractor FindRemainingInteractor(
    IXRSelectInteractor exitingInteractor)
    {
        foreach (IXRSelectInteractor interactor
                 in xRGrabInteractable.interactorsSelecting)
        {
            if (!object.ReferenceEquals(interactor, exitingInteractor))
            {
                return interactor;
            }
        }

        return null;
    }

    private void SetPrimaryInteractor(IXRSelectInteractor interactor)
    {
        primaryInteractor = interactor;

        isGrab = true;
        previousPosition = transform.position;
        currentVelocity = Vector3.zero;

        attackStats =
            interactor.transform.GetComponentInParent<PlayerAttackStats>();

        if (slot == null)
        {
            xRGrabInteractable.throwOnDetach = true;
        }

        if (isGun)
        {
            SubscribeGunControls(interactor.transform);
        }
    }

    private void FinishRelease()
    {
        isGrab = false;

        bool canEquip =
            isCollisionState &&
            slot != null &&
            slot.weapon == this;

        if (canEquip)
        {
            EquipToSlot();
        }
        else
        {
            if (rigid != null)
            {
                rigid.isKinematic = false;
            }

            if (!isCollisionState)
            {
                hitHistory.Clear();
            }
        }

        attackStats = null;
    }


    // ─────────────────────────────────────
    // Equipment Slot
    // ─────────────────────────────────────

    private void EquipToSlot()
    {
        xRGrabInteractable.throwOnDetach = false;

        rigid.isKinematic = true;

        transform.SetParent(slot.attachPoint);

        transform.SetPositionAndRotation(
            slot.attachPoint.position,
            slot.attachPoint.rotation
        );
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isCollisionState)
            return;

        isCollisionState = true;

        // 근접 무기의 타격 판정
        CheckMeleeDamage(other);

        // 슬롯 확인 및 등록
        CheckEquipmentSlot(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        // 슬롯 안에서 잡았다가 바로 놓는 경우를 위한 확인
        if (isCollisionState)
            return;

        isCollisionState = true;

        CheckEquipmentSlot(other, false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isCollisionState)
            return;

        isCollisionState = false;

        if (slot != null && slot.weapon == this)
        {
            slot.weapon = null;
            slot = null;
        }
    }

    private void CheckEquipmentSlot(Collider other, bool sendHaptic)
    {
        slot = other.GetComponentInParent<EquipmentSlot>();

        if (slot == null)
            return;

        if (slot.weapon == null)
        {
            slot.weapon = this;

            if (sendHaptic)
                SendHaptic();
        }
    }


    // ─────────────────────────────────────
    // Melee Damage
    // ─────────────────────────────────────

    private void CheckMeleeDamage(Collider other)
    {
        IDamageable target = FindDamageTarget(other);

        if (target == null)
            return;

        if (currentVelocity.magnitude < trueDamageSpeed)
            return;

        if (hitHistory.TryGetValue(target, out float lastHitTime))
        {
            if (Time.time - lastHitTime < hitCooldown)
                return;
        }

        target.TakeDamage(CreateDamageInfo(other.ClosestPoint(transform.position)));

        hitHistory[target] = Time.time;

        SendHaptic();
    }

    /// <summary>맞은 콜라이더(또는 그 부모)의 피해 대상. 플레이어 자신·이미 죽은 대상은 제외한다.</summary>
    private static IDamageable FindDamageTarget(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target == null || target is PlayerHealth || target.IsDead)
            return null;

        return target;
    }

    private float CalculateDamage()
    {
        return damage + (attackStats != null ? attackStats.AttackPower : 0f);
    }

    private DamageInfo CreateDamageInfo(Vector3 hitPoint)
    {
        GameObject instigator = attackStats != null ? attackStats.gameObject : gameObject;
        return new DamageInfo(CalculateDamage(), instigator, hitPoint);
    }


    // ─────────────────────────────────────
    // Gun Controls
    // ─────────────────────────────────────

    private void SubscribeGunControls(Transform interactorTransform)
    {
        vrController =
            interactorTransform.GetComponentInParent<VRController>();

        if (vrController == null)
            return;

        if (interactorTransform.CompareTag("Left"))
        {
            isRight = false;

            vrController.On_left_btn_X += ReLoad;
            vrController.On_left_btn_Y += AutoChange;
        }
        else if (interactorTransform.CompareTag("Right"))
        {
            isRight = true;

            vrController.On_right_btn_A += ReLoad;
            vrController.On_right_btn_B += AutoChange;
        }
        else
        {
            return;
        }

        SubscribeShotInput();
    }

    private void SubscribeShotInput()
    {
        if (vrController == null)
            return;

        // 중복 연결 방지
        UnsubscribeShotInput();

        if (isRight)
        {
            if (isAuto)
                vrController.On_right_btn_front_Update += Shot;
            else
                vrController.On_right_btn_front += Shot;
        }
        else
        {
            if (isAuto)
                vrController.On_left_btn_front_Update += Shot;
            else
                vrController.On_left_btn_front += Shot;
        }
    }

    private void UnsubscribeShotInput()
    {
        if (vrController == null)
            return;

        vrController.On_left_btn_front -= Shot;
        vrController.On_left_btn_front_Update -= Shot;

        vrController.On_right_btn_front -= Shot;
        vrController.On_right_btn_front_Update -= Shot;
    }

    private void UnsubscribeGunControls()
    {
        if (vrController == null)
            return;

        UnsubscribeShotInput();

        vrController.On_left_btn_X -= ReLoad;
        vrController.On_left_btn_Y -= AutoChange;

        vrController.On_right_btn_A -= ReLoad;
        vrController.On_right_btn_B -= AutoChange;

        vrController = null;
    }


    // ─────────────────────────────────────
    // Gun Actions
    // ─────────────────────────────────────

    private void Shot()
    {
        if (Time.time - lastShotTime < shotColldown)
            return;

        Ray ray = new Ray(muzzle.position, muzzle.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, shotLayerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log(
                $"맞은 대상: {hit.collider.name}, 위치: {hit.point}"
            );
            CheckGunDamage(hit.collider, hit.point);

            GameObject go = Instantiate(
                bullet,
                hit.point,
                muzzle.rotation
            );
            go.GetComponent<Collider>().enabled = false;

            Destroy(go, 10f);
        }

        Debug.DrawRay(
            ray.origin,
            ray.direction * maxDistance,
            Color.red,
            2f
        );

        SendHaptic(1f, 0.1f);

        if (shotSound != null && shotSound.clip != null)
            shotSound.PlayOneShot(shotSound.clip);

        lastShotTime = Time.time;

        ShotEffect();
    }

    void ShotEffect()
    {
        GameObject casing = Instantiate(bullet, casingPos.position, muzzle.rotation);
        Rigidbody casingRigid = casing.AddComponent<Rigidbody>();
        casingRigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        casingRigid.AddForce(casingPos.transform.forward * Random.Range(50f, 100f));
        casingRigid.AddForce(casingPos.transform.right * Random.Range(-20f, 40f));
        casingRigid.AddTorque(Vector3.up * Random.Range(-1f, 1f), ForceMode.Impulse);
        casing.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(casing, 10f);

        if (slidePos != null)
            StartCoroutine(ShotAnimation());
    }

    IEnumerator ShotAnimation()
    {
        slidePos.transform.localPosition = shotSlidePos;
        yield return new WaitForSeconds(0.02f);
        slidePos.transform.localPosition = baseSlidePos;
    }

    private void AutoChange()
    {
        if (!isAutoChange)
            return;

        isAuto = !isAuto;

        // 현재 손과 발사 모드에 맞춰 입력을 다시 연결
        SubscribeShotInput();
    }

    private void ReLoad()
    {
        // 재장전 기능은 추후 구현
    }

    private void CheckGunDamage(Collider other, Vector3 hitPoint)
    {
        IDamageable target = FindDamageTarget(other);

        if (target == null)
            return;

        target.TakeDamage(CreateDamageInfo(hitPoint));
    }

    // ─────────────────────────────────────
    // Haptic
    // ─────────────────────────────────────

    private void SendHaptic()
    {
        if (xRGrabInteractable == null)
            return;

        if (xRGrabInteractable.interactorsSelecting.Count == 0)
            return;

        IXRSelectInteractor interactor =
            xRGrabInteractable.interactorsSelecting[0];

        if (interactor is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(
                hapticAmplitude,
                hapticDuration
            );
        }
    }

    private void SendHaptic(float _hapticAmplitude, float _hapticDuration)
    {
        if (xRGrabInteractable == null)
            return;

        if (xRGrabInteractable.interactorsSelecting.Count == 0)
            return;

        IXRSelectInteractor interactor =
            xRGrabInteractable.interactorsSelecting[0];

        if (interactor is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(
                _hapticAmplitude,
                _hapticDuration
            );
        }
    }
}
