using UnityEngine;

namespace Shelter.Enemies.Animation
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimatorDriver : MonoBehaviour, IEnemyAnimationDriver
    {
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeadHash = Animator.StringToHash("Dead");
        private static readonly int VaultHash = Animator.StringToHash("Vault");

        private Animator _animator;
        private bool _hasVault;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
                if (parameter.nameHash == VaultHash) _hasVault = true;
        }

        /// <summary>
        /// 위치는 NavMeshAgent(또는 창문 넘기 스크립트)가 결정한다. 이 콜백을 두면 Animator가 루트 모션을 transform에
        /// 직접 적용하지 않아, 에이전트 이동과 루트 모션이 겹쳐 미끄러지거나 떨리는 일이 없다.
        /// </summary>
        private void OnAnimatorMove()
        {
        }

        public void SetMoveSpeed01(float normalized01)
        {
            _animator.SetFloat(MoveSpeedHash, Mathf.Clamp01(normalized01));
        }

        public void PlayAttack()
        {
            _animator.SetTrigger(AttackHash);
        }

        public void PlayHitReaction()
        {
            _animator.SetTrigger(HitHash);
        }

        public void PlayDeath()
        {
            _animator.SetTrigger(DeadHash);
        }

        public void PlayVault()
        {
            if (_hasVault) _animator.SetTrigger(VaultHash);
        }

        public void ResetForPool()
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
    }
}
