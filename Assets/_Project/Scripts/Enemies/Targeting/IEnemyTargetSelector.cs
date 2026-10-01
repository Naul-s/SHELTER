namespace Shelter.Enemies.Targeting
{
    public interface IEnemyTargetSelector
    {
        EnemyTargetDecision SelectTarget(in EnemyTargetContext context);
    }
}
