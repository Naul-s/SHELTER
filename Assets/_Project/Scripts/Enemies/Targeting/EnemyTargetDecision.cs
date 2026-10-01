using Shelter.Enemies;

namespace Shelter.Enemies.Targeting
{
    public readonly struct EnemyTargetDecision
    {
        public readonly IEnemyTarget Target;

        public EnemyTargetDecision(IEnemyTarget target)
        {
            Target = target;
        }

        public bool HasTarget => Target != null;
    }
}
