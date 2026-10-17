using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Weapon : MonoBehaviour
{
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // References
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("ÂüÁ¶")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private VRController vrController;// ÇöÀç ÁÖ ¼Õ¿¡ ¿¬°áµÈ ÄÁÆ®·Ñ·¯
    [SerializeField] private EquipmentSlot slot;

    private XRGrabInteractable xRGrabInteractable;
    private Rigidbody rigid;


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // State
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("»óÅÂ")]
    [SerializeField] private bool isGrab = false;// Àû¾îµµ ÇÑ ¼ÕÀÌ ÃÑÀ» Àâ°í ÀÖ´ÂÁö ³ªÅ¸³»´Â »óÅÂ
    [SerializeField] private bool isCollisionState = false;

    private Vector3 previousPosition;
    private Vector3 currentVelocity;

    private bool isRight;// ÇöÀç ÁÖ ¼ÕÀÌ ¿À¸¥¼ÕÀÎÁö ¿Þ¼ÕÀÎÁö ±¸ºÐ

    private IXRSelectInteractor primaryInteractor;// ÇöÀç ÃÑ±â Á¶ÀÛ ±ÇÇÑÀ» °¡Áø ¼Õ


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Weapon Settings
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("¹«±â ¼³Á¤")]

    [Tooltip("¿òÁ÷ÀÓÀÌ ÇØ´ç ¼Óµµ ÀÌ»óÀÏ ¶§ µ¥¹ÌÁö Àû¿ë")]
    [SerializeField] private float trueDamageSpeed = 2.0f;

    [Tooltip("µ¿ÀÏ ´ë»ó ÀçÅ¸°Ý ÄðÅ¸ÀÓ")]
    [SerializeField] private float hitCooldown = 0.3f;

    [Tooltip("¹«±â µ¥¹ÌÁö")]
    [SerializeField] private int damage = 0;

    [Tooltip("¹«±â Æ¼¾î")]
    [SerializeField] private int tier = 0;

    [Tooltip("¹«±â ¹«°Ô")]
    [SerializeField] private float weight = 0;

    private Dictionary<IDamagable, float> hitHistory =
        new Dictionary<IDamagable, float>();


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Haptic
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("±ÙÁ¢ °ø°Ý Áøµ¿")]

    [SerializeField] private float hapticAmplitude = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Gun Settings
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    [Header("ÃÑ ¼³Á¤")]

    [Tooltip("ÃÑÀÌ¸é Ã¼Å©")]
    [SerializeField] private bool isGun;

    [Tooltip("false = ´Ü¹ß °íÁ¤, true = ´Ü¹ß/¿¬»ç ÀüÈ¯ °¡´É")]
    [SerializeField] private bool isAutoChange = false;

    private bool isAuto = false;// ´Ü¹ß¡¤¿¬»ç ¸ðµå »óÅÂ

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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Initialization
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void Start()
    {
        xRGrabInteractable =
            GetComponentInParent<XRGrabInteractable>();

        rigid =
            GetComponentInParent<Rigidbody>();

        if (xRGrabInteractable == null)
        {
            Debug.LogError(
                $"{name}: XRGrabInteractableÀ» Ã£À» ¼ö ¾ø½À´Ï´Ù.",
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


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Update / Velocity
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void Update()
    {
        if (!isGrab)
            return;

        currentVelocity =
            (transform.position - previousPosition) / Time.deltaTime;

        previousPosition = transform.position;
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Grab Events
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isGrab = true;

        // ¾ÆÁ÷ ÁÖ ¼ÕÀÌ ¾øÀ» ¶§¸¸ Á¶ÀÛ ±ÇÇÑ ºÎ¿©
        if (primaryInteractor == null)
        {
            SetPrimaryInteractor(args.interactorObject);
        }
        // ÀÌ¹Ì ÁÖ ¼ÕÀÌ ÀÖ´Ù¸é »õ·Î ÀâÀº ¼ÕÀº º¸Á¶ ¼Õ
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        bool wasPrimary =
            object.ReferenceEquals(primaryInteractor, args.interactorObject);

        // ³õÀº ¼ÕÀ» Á¦¿ÜÇÏ°í ¾ÆÁ÷ Àâ°í ÀÖ´Â ¼Õ Ã£±â
        IXRSelectInteractor remainingInteractor =
            FindRemainingInteractor(args.interactorObject);

        // ÁÖ ¼ÕÀÌ ³õ¾Ò´Ù¸é ±âÁ¸ ÀÔ·Â ¿¬°á ÇØÁ¦
        if (wasPrimary)
        {
            UnsubscribeGunControls();
            primaryInteractor = null;
        }

        // ´Ù¸¥ ¼ÕÀÌ ¿©ÀüÈ÷ Àâ°í ÀÖ´Ù¸é ¿ÏÀüÈ÷ ³õÀº °ÍÀÌ ¾Æ´Ô
        if (remainingInteractor != null)
        {
            isGrab = true;

            // ÁÖ ¼ÕÀÌ ³õ¾ÒÀ» ¶§¸¸ ³²Àº ¼ÕÀ¸·Î ±ÇÇÑ ÀÌÀü
            if (wasPrimary)
            {
                SetPrimaryInteractor(remainingInteractor);
            }

            return;
        }

        // ´õ ÀÌ»ó Àâ°í ÀÖ´Â ¼ÕÀÌ ¾øÀ½
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

        playerStats =
            interactor.transform.GetComponentInParent<PlayerStats>();

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

        playerStats = null;
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Equipment Slot
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

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

        // ±ÙÁ¢ ¹«±âÀÇ Å¸°Ý ÆÇÁ¤
        CheckMeleeDamage(other);

        // ½½·Ô È®ÀÎ ¹× µî·Ï
        CheckEquipmentSlot(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        // ½½·Ô ¾È¿¡¼­ Àâ¾Ò´Ù°¡ ¹Ù·Î ³õ´Â °æ¿ì¸¦ À§ÇÑ È®ÀÎ
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


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Melee Damage
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void CheckMeleeDamage(Collider other)
    {
        IDamagable target =
            other.transform.GetComponent<IDamagable>();

        if (target == null)
            return;

        if (currentVelocity.magnitude < trueDamageSpeed)
            return;

        if (hitHistory.TryGetValue(target, out float lastHitTime))
        {
            if (Time.time - lastHitTime < hitCooldown)
                return;
        }

        target.TakeDamage(CalculateDamage());

        hitHistory[target] = Time.time;

        SendHaptic();
    }

    private int CalculateDamage()
    {
        if (playerStats == null)
            return 0;

        return (int)(playerStats.TotalStats.STR * 0.3f) + damage;
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Gun Controls
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void SubscribeGunControls(Transform interactorTransform)
    {
        if (playerStats == null)
            return;

        vrController =
            playerStats.GetComponentInParent<VRController>();

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

        // Áßº¹ ¿¬°á ¹æÁö
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


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Gun Actions
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void Shot()
    {
        if (Time.time - lastShotTime < shotColldown)
            return;

        Ray ray = new Ray(muzzle.position, muzzle.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Debug.Log(
                $"¸ÂÀº ´ë»ó: {hit.collider.name}, À§Ä¡: {hit.point}"
            );
            CheckGunDamage(hit.collider);

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

        // ÇöÀç ¼Õ°ú ¹ß»ç ¸ðµå¿¡ ¸ÂÃç ÀÔ·ÂÀ» ´Ù½Ã ¿¬°á
        SubscribeShotInput();
    }

    private void ReLoad()
    {
        // ÀçÀåÀü ±â´ÉÀº ÃßÈÄ ±¸Çö
    }

    private void CheckGunDamage(Collider other)
    {
        IDamagable target =
            other.transform.GetComponent<IDamagable>();

        if (target == null)
            return;

        target.TakeDamage(CalculateDamage());
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Haptic
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

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