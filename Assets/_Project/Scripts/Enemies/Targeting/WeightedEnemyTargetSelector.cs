using Shelter.Enemies;

namespace Shelter.Enemies.Targeting
{
    /// <summary>
    /// 기본값: 쉘터로 이동. 어그로 반경 안에 플레이어가 있으면 EnemyData.PlayerPreferenceWeight 확률로 플레이어를 우선한다.
    /// 한 번 플레이어를 타겟으로 잡으면, 시야(LoseTargetRadius)를 벗어나도 LoseTargetTime 동안은 계속 쫓는다(사라짐 감지 전 유예).
    /// </summary>
    public sealed class WeightedEnemyTargetSelector : IEnemyTargetSelector
    {
        private readonly IPlayerTargetProvider _playerProvider;
        private readonly IShelterTargetProvider _shelterProvider;

        public WeightedEnemyTargetSelector(IPlayerTargetProvider playerProvider, IShelterTargetProvider shelterProvider)
        {
            _playerProvider = playerProvider;
            _shelterProvider = shelterProvider;
        }

        public EnemyTargetDecision SelectTarget(in EnemyTargetContext context)
        {
            IEnemyTarget current = context.CurrentTarget;
            if (current != null && current.Kind == EnemyTargetKind.Player && !current.Damageable.IsDead)
            {
                float loseRadius = context.Data.LoseTargetRadius;
                float distanceSqr = (current.Transform.position - context.Origin).sqrMagnitude;
                bool withinLoseRadius = distanceSqr <= loseRadius * loseRadius;

                if (withinLoseRadius || context.TimeSinceTargetVisible < context.Data.LoseTargetTime)
                    return new EnemyTargetDecision(current);
            }

            bool playerFound = _playerProvider.TryGetNearestPlayer(context.Origin, context.Data.AggroRadius, out IEnemyTarget player);
            if (playerFound && UnityEngine.Random.value < context.Data.PlayerPreferenceWeight)
                return new EnemyTargetDecision(player);

            _shelterProvider.TryGetNearestShelterTarget(context.Origin, out IEnemyTarget shelter);
            return new EnemyTargetDecision(shelter);
        }
    }
}
