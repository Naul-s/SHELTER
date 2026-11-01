using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Shelter.Enemies.Combat;
using Shelter.Enemies.Movement;

namespace Shelter.Enemies.Ragdoll
{
    /// <summary>
    /// 휴머노이드 본 이름(pelvis, spine_03, head, upperarm_l/r, lowerarm_l/r, thigh_l/r, calf_l/r)을 기준으로
    /// 런타임에 11개 파츠짜리 랙돌(Rigidbody+Collider+CharacterJoint)을 자동 구성한다. Unity 기본 Ragdoll Wizard가
    /// 만드는 구성과 동일한 파츠 수다.
    ///
    /// 평소엔 모든 Rigidbody를 Kinematic으로 꺼둬 Animator/NavMeshAgent가 그대로 움직임을 제어하게 하고,
    /// EnemyBase.Died 이벤트가 발생하는 순간에만 물리를 켜서 쓰러진다.
    ///
    /// 본 이름은 UE4 매니퀸 리타겟 스켈레톤 규약을 따른다 — 이 이름 체계를 쓰지 않는 모델에 붙이면 해당 본을
    /// 못 찾아 그 파츠만 조용히 건너뛴다(예외를 던지지 않는다).
    ///
    /// 주의: CharacterJoint 축/각도 제한은 범용적으로 느슨하게 잡은 근사치다. 본 하나하나의 정확한 해부학적
    /// 회전축까지 맞춘 값은 아니므로, 쓰러지는 모습이 부자연스러우면 Inspector에서 생성된 CharacterJoint들의
    /// axis/swingAxis/limit을 직접 다듬으면 된다.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyRagdoll : MonoBehaviour
    {
        private const string Pelvis = "pelvis";
        private const string Spine = "spine_03";
        private const string Head = "head";
        private const string UpperArmL = "upperarm_l";
        private const string LowerArmL = "lowerarm_l";
        private const string UpperArmR = "upperarm_r";
        private const string LowerArmR = "lowerarm_r";
        private const string ThighL = "thigh_l";
        private const string CalfL = "calf_l";
        private const string ThighR = "thigh_r";
        private const string CalfR = "calf_r";

        private Animator _animator;
        private NavMeshAgent _agent;
        private Collider _mainCollider;
        private MonoBehaviour[] _gameplayScripts;

        private readonly List<Rigidbody> _ragdollBodies = new List<Rigidbody>();
        private readonly List<Collider> _ragdollColliders = new List<Collider>();

        private bool _built;
        private bool _ragdollActive;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _mainCollider = GetComponent<CapsuleCollider>();

            _gameplayScripts = new MonoBehaviour[]
            {
                GetComponent<NavMeshEnemyMovement>(),
                GetComponent<MeleeEnemyAttack>()
            };

            BuildRagdoll();
            SetRagdollPhysicsActive(false);

            var enemy = GetComponent<EnemyBase>();
            if (enemy != null) enemy.Died += ActivateRagdoll;
        }

        private void OnDestroy()
        {
            var enemy = GetComponent<EnemyBase>();
            if (enemy != null) enemy.Died -= ActivateRagdoll;
        }

        /// <summary>풀에서 재사용하기 직전(EnemyBase.Spawn)에 호출 — 랙돌을 끄고 다시 애니메이션/내비게이션 제어로 되돌린다.</summary>
        public void ResetRagdoll()
        {
            if (!_built) return;

            SetRagdollPhysicsActive(false);

            if (_animator != null) _animator.enabled = true;
            if (_agent != null) _agent.enabled = true;
            if (_mainCollider != null) _mainCollider.enabled = true;

            foreach (MonoBehaviour script in _gameplayScripts)
                if (script != null) script.enabled = true;
        }

        private void ActivateRagdoll()
        {
            if (_ragdollActive) return;

            if (_animator != null) _animator.enabled = false;
            if (_agent != null) _agent.enabled = false;
            if (_mainCollider != null) _mainCollider.enabled = false;

            foreach (MonoBehaviour script in _gameplayScripts)
                if (script != null) script.enabled = false;

            SetRagdollPhysicsActive(true);
        }

        private void SetRagdollPhysicsActive(bool active)
        {
            _ragdollActive = active;

            foreach (Rigidbody body in _ragdollBodies)
            {
                if (body == null) continue;
                body.isKinematic = !active;
                body.detectCollisions = active;
            }

            foreach (Collider collider in _ragdollColliders)
                if (collider != null) collider.enabled = active;
        }

        private void BuildRagdoll()
        {
            if (_built) return;
            _built = true;

            Transform pelvis = FindBone(Pelvis);
            Transform spine = FindBone(Spine);
            Transform head = FindBone(Head);
            Transform upperArmL = FindBone(UpperArmL);
            Transform lowerArmL = FindBone(LowerArmL);
            Transform upperArmR = FindBone(UpperArmR);
            Transform lowerArmR = FindBone(LowerArmR);
            Transform thighL = FindBone(ThighL);
            Transform calfL = FindBone(CalfL);
            Transform thighR = FindBone(ThighR);
            Transform calfR = FindBone(CalfR);

            if (pelvis == null)
            {
                Debug.LogWarning($"{name}: 랙돌 루트 본('{Pelvis}')을 찾지 못해 EnemyRagdoll을 구성하지 않습니다.", this);
                return;
            }

            Rigidbody pelvisBody = AddBody(pelvis, 8f);
            AddBoxCollider(pelvis, new Vector3(0.25f, 0.2f, 0.3f));

            Rigidbody spineBody = AddLimbPart(spine, head, 10f, pelvisBody);
            Rigidbody chestOrPelvis = spineBody != null ? spineBody : pelvisBody;

            AddLimbPart(head, null, 3f, chestOrPelvis, fallbackRadius: 0.12f);

            Rigidbody upperArmLBody = AddLimbPart(upperArmL, lowerArmL, 2.5f, chestOrPelvis);
            AddLimbPart(lowerArmL, null, 1.5f, upperArmLBody, fallbackRadius: 0.05f);

            Rigidbody upperArmRBody = AddLimbPart(upperArmR, lowerArmR, 2.5f, chestOrPelvis);
            AddLimbPart(lowerArmR, null, 1.5f, upperArmRBody, fallbackRadius: 0.05f);

            Rigidbody thighLBody = AddLimbPart(thighL, calfL, 5f, pelvisBody);
            AddLimbPart(calfL, null, 3f, thighLBody, fallbackRadius: 0.08f);

            Rigidbody thighRBody = AddLimbPart(thighR, calfR, 5f, pelvisBody);
            AddLimbPart(calfR, null, 3f, thighRBody, fallbackRadius: 0.08f);
        }

        /// <summary>distal 본이 있으면 그 방향으로 캡슐 콜라이더를, 없으면(머리/손끝처럼 말단) 구체 콜라이더를 붙인다.</summary>
        private Rigidbody AddLimbPart(Transform bone, Transform distal, float mass, Rigidbody connectedBody, float fallbackRadius = 0.08f)
        {
            if (bone == null) return null;

            Rigidbody body = AddBody(bone, mass);

            if (distal != null) AddCapsuleCollider(bone, distal);
            else AddSphereCollider(bone, fallbackRadius);

            if (connectedBody != null) AddJoint(bone, connectedBody);

            return body;
        }

        private Rigidbody AddBody(Transform bone, float mass)
        {
            Rigidbody body = bone.GetComponent<Rigidbody>();
            if (body == null) body = bone.gameObject.AddComponent<Rigidbody>();

            body.mass = mass;
            body.isKinematic = true;
            _ragdollBodies.Add(body);
            return body;
        }

        private void AddCapsuleCollider(Transform bone, Transform distal)
        {
            if (bone.GetComponent<Collider>() != null) return;

            Vector3 localDistal = bone.InverseTransformPoint(distal.position);
            float length = localDistal.magnitude;
            if (length < 0.01f) length = 0.1f;

            var capsule = bone.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = Mathf.Clamp(length * 0.25f, 0.03f, 0.12f);
            capsule.height = Mathf.Max(length, capsule.radius * 2f);
            capsule.center = localDistal * 0.5f;
            capsule.direction = ClosestAxis(localDistal);

            _ragdollColliders.Add(capsule);
        }

        private void AddSphereCollider(Transform bone, float radius)
        {
            if (bone.GetComponent<Collider>() != null) return;

            var sphere = bone.gameObject.AddComponent<SphereCollider>();
            sphere.radius = radius;
            _ragdollColliders.Add(sphere);
        }

        private void AddBoxCollider(Transform bone, Vector3 size)
        {
            if (bone.GetComponent<Collider>() != null) return;

            var box = bone.gameObject.AddComponent<BoxCollider>();
            box.size = size;
            _ragdollColliders.Add(box);
        }

        private static void AddJoint(Transform bone, Rigidbody connectedBody)
        {
            var joint = bone.gameObject.AddComponent<CharacterJoint>();
            joint.connectedBody = connectedBody;
            joint.axis = new Vector3(1f, 0f, 0f);
            joint.swingAxis = new Vector3(0f, 1f, 0f);
            joint.enableProjection = true;

            joint.lowTwistLimit = new SoftJointLimit { limit = -20f };
            joint.highTwistLimit = new SoftJointLimit { limit = 20f };
            joint.swing1Limit = new SoftJointLimit { limit = 40f };
            joint.swing2Limit = new SoftJointLimit { limit = 40f };
        }

        private static int ClosestAxis(Vector3 localDirection)
        {
            localDirection = new Vector3(Mathf.Abs(localDirection.x), Mathf.Abs(localDirection.y), Mathf.Abs(localDirection.z));

            if (localDirection.x >= localDirection.y && localDirection.x >= localDirection.z) return 0;
            if (localDirection.y >= localDirection.x && localDirection.y >= localDirection.z) return 1;
            return 2;
        }

        private Transform FindBone(string boneName)
        {
            return FindRecursive(transform, boneName);
        }

        private static Transform FindRecursive(Transform current, string boneName)
        {
            if (current.name == boneName) return current;

            for (int i = 0; i < current.childCount; i++)
            {
                Transform found = FindRecursive(current.GetChild(i), boneName);
                if (found != null) return found;
            }

            return null;
        }
    }
}
