using System;
using System.Collections;
using System.Collections.Generic;
using Shelter.Core;
using Shelter.Player;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    // ─────────────────────────────────────
    // References
    // ─────────────────────────────────────

    [Header("참조")]
    [SerializeField] private PlayerAttackStats attackStats;
    [SerializeField] private VRController vrController;// 현재 주 손에 연결된 컨트롤러(있으면 A/B·X/Y 버튼으로 연사 전환)
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

    private IXRSelectInteractor primaryInteractor;// 현재 총기 조작 권한을 가진 손
    private GameObject holder;// 총을 잡은 플레이어(처치 보상을 받을 공격자)
    private bool isTriggerHeld;// 주 손 트리거를 당기고 있는지(연사용)


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

    [Tooltip("false = 단발 고정, true = 연사(트리거를 누르고 있으면 계속 발사). 컨트롤러 버튼(VRController)이 있으면 단발/연사 전환 가능")]
    [SerializeField] private bool isAutoChange = false;

    private bool isAuto = false;// 단발·연사 모드 상태

    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float shotColldown = 0.1f;

    private float lastShotTime = float.NegativeInfinity;
    private float lastDryFireTime = float.NegativeInfinity;

    [SerializeField] private Transform muzzle;
    [Tooltip("탄피 배출 연출용(총알 모델의 탄피 부분만 사용).")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private AudioSource shotSound;
    [Tooltip("발사음 후보. 쏠 때마다 하나를 무작위로 고른다. 비우면 shotSound의 클립을 쓴다.")]
    [SerializeField] private AudioClip[] shotClips;
    [Tooltip("총구 화염 이펙트(총구 위치의 자식). 쏠 때마다 재생.")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [SerializeField] private Transform slidePos;
    private Vector3 baseSlidePos;
    [SerializeField] private Vector3 shotSlidePos;
    [SerializeField] private Transform casingPos;

    // 사격 레이캐스트 대상: 기본 레이어에서 플레이어 차단벽(PlayerBlocker)을 뺀다 — 창문 판자 너머 좀비를 쏠 수 있게.
    private int shotLayerMask;


    // ─────────────────────────────────────
    // Ammo
    // ─────────────────────────────────────

    [Header("탄약 (재장전·보충 없음 — 다 쓰면 새 총을 사야 한다)")]
    [Tooltip("이 총의 총 탄약 수. 상점에서 살 때는 상점 품목의 탄약 수로 덮어쓴다.")]
    [SerializeField, Min(0)] private int maxAmmo = 90;
    [Tooltip("남은 탄약 표시(총에 붙은 작은 카운터). 비워도 된다.")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Color ammoColor = new Color(0.85f, 0.95f, 0.85f, 1f);
    [SerializeField] private Color lowAmmoColor = new Color(0.95f, 0.15f, 0.1f, 1f);
    [SerializeField, Min(0)] private int lowAmmoThreshold = 10;

    private int currentAmmo;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsGun => isGun;

    /// <summary>남은 탄약이 바뀌었다(발사·지급).</summary>
    public event Action<int> AmmoChanged;

    // ─────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────

    private void Awake()
    {
        currentAmmo = maxAmmo;
        isAuto = isAutoChange;// 연사 가능한 총은 연사로 시작(전환 버튼이 없는 리그에서도 연사가 된다)
        UpdateAmmoText();
    }

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
        xRGrabInteractable.activated.AddListener(OnActivated);
        xRGrabInteractable.deactivated.AddListener(OnDeactivated);

        if (slidePos != null)
            baseSlidePos = slidePos.transform.localPosition;
    }

    private void OnDestroy()
    {
        if (xRGrabInteractable != null)
        {
            xRGrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            xRGrabInteractable.selectExited.RemoveListener(OnSelectExited);
            xRGrabInteractable.activated.RemoveListener(OnActivated);
            xRGrabInteractable.deactivated.RemoveListener(OnDeactivated);
        }

        UnsubscribeGunControls();
    }

    /// <summary>탄약 수를 정한다(상점 구매 직후 등). 상태 변경 지점 — 네트워킹 도입 시 서버에서만 호출.</summary>
    public void SetAmmo(int ammo)
    {
        maxAmmo = Mathf.Max(0, ammo);
        currentAmmo = maxAmmo;
        UpdateAmmoText();
        AmmoChanged?.Invoke(currentAmmo);
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

        // 연사: 트리거를 누르고 있는 동안 쿨다운마다 발사
        if (isGun && isAuto && isTriggerHeld)
            Shot();
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
            isTriggerHeld = false;
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
        isTriggerHeld = false;

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

        PlayerHealth player = interactor.transform.GetComponentInParent<PlayerHealth>();
        holder = player != null ? player.gameObject : null;

        if (slot == null)
        {
            xRGrabInteractable.throwOnDetach = true;
        }

        if (isGun)
        {
            SubscribeGunControls(interactor);
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
        holder = null;
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

    /// <summary>공격자 = 총을 잡은 플레이어(처치 보상 지급 대상). 없으면 공격 스탯 보유자, 그것도 없으면 무기 자신.</summary>
    private DamageInfo CreateDamageInfo(Vector3 hitPoint)
    {
        GameObject instigator =
            holder != null ? holder :
            attackStats != null ? attackStats.gameObject :
            gameObject;
        return new DamageInfo(CalculateDamage(), instigator, hitPoint);
    }


    // ─────────────────────────────────────
    // Gun Controls
    // ─────────────────────────────────────

    // 트리거: XR Interaction Toolkit 의 Activate(잡은 손의 트리거) — 어떤 XR 리그에서도 동작한다.
    private void OnActivated(ActivateEventArgs args)
    {
        if (!isGun || !object.ReferenceEquals(args.interactorObject, primaryInteractor))
            return;

        isTriggerHeld = true;
        Shot();
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        if (object.ReferenceEquals(args.interactorObject, primaryInteractor))
            isTriggerHeld = false;
    }

    // 컨트롤러 버튼(단발/연사 전환)은 VRController 가 있는 리그에서만 연결한다.
    private void SubscribeGunControls(IXRSelectInteractor interactor)
    {
        Transform interactorTransform = interactor.transform;
        vrController =
            interactorTransform.GetComponentInParent<VRController>();

        if (vrController == null)
            return;

        bool isLeft = interactorTransform.CompareTag("Left") ||
                      interactor.handedness == InteractorHandedness.Left;

        if (isLeft)
        {
            vrController.On_left_btn_X += ReLoad;
            vrController.On_left_btn_Y += AutoChange;
        }
        else
        {
            vrController.On_right_btn_A += ReLoad;
            vrController.On_right_btn_B += AutoChange;
        }
    }

    private void UnsubscribeGunControls()
    {
        if (vrController == null)
            return;

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

        if (currentAmmo <= 0)
        {
            DryFire();
            return;
        }

        lastShotTime = Time.time;
        currentAmmo--;
        UpdateAmmoText();
        AmmoChanged?.Invoke(currentAmmo);

        Ray ray = new Ray(muzzle.position, muzzle.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, shotLayerMask, QueryTriggerInteraction.Ignore))
        {
            CheckGunDamage(hit.collider, hit.point);
        }

        SendHaptic(1f, 0.1f);

        PlayShotSound();

        if (muzzleFlash != null)
            muzzleFlash.Play(true);

        ShotEffect();
    }

    /// <summary>탄약이 없을 때 트리거: 발사 대신 약한 진동만(연사 중에는 0.3초 간격).</summary>
    private void DryFire()
    {
        if (Time.time - lastDryFireTime < 0.3f)
            return;

        lastDryFireTime = Time.time;
        SendHaptic(0.15f, 0.03f);
    }

    private void UpdateAmmoText()
    {
        if (ammoText == null)
            return;

        ammoText.SetText("{0}", currentAmmo);
        ammoText.color = currentAmmo <= lowAmmoThreshold ? lowAmmoColor : ammoColor;
    }

    private void PlayShotSound()
    {
        if (shotSound == null)
            return;

        AudioClip clip = shotClips != null && shotClips.Length > 0
            ? shotClips[Random.Range(0, shotClips.Length)]
            : shotSound.clip;

        if (clip != null)
            shotSound.PlayOneShot(clip);
    }

    void ShotEffect()
    {
        if (bullet != null && casingPos != null)
        {
            GameObject casing = Instantiate(bullet, casingPos.position, muzzle.rotation);
            Rigidbody casingRigid = casing.AddComponent<Rigidbody>();
            casingRigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            casingRigid.AddForce(casingPos.transform.forward * Random.Range(50f, 100f));
            casingRigid.AddForce(casingPos.transform.right * Random.Range(-20f, 40f));
            casingRigid.AddTorque(Vector3.up * Random.Range(-1f, 1f), ForceMode.Impulse);
            casing.transform.GetChild(0).gameObject.SetActive(false);
            Destroy(casing, 10f);
        }

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
    }

    private void ReLoad()
    {
        // 재장전 없음 — 탄약을 다 쓰면 새 총을 산다(기획 2026-10-26).
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
