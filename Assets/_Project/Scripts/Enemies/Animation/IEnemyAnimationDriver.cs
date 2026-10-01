namespace Shelter.Enemies.Animation
{
    /// <summary>AI 로직과 Animator 파라미터 이름을 분리하기 위한 경계.</summary>
    public interface IEnemyAnimationDriver
    {
        void SetMoveSpeed01(float normalized01);
        void PlayAttack();
        void PlayHitReaction();
        void PlayDeath();
        void ResetForPool();
    }
}
