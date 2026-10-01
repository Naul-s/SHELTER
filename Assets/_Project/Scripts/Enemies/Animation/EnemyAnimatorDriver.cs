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

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
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

        public void ResetForPool()
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
    }
}
