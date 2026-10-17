using Shelter.Enemies;
using Shelter.ShelterDefense;

namespace Shelter.Enemies.Targeting
{
    /// <summary>
    /// 쉘터 공략 타겟 선택(2026-09-30 기획): 쉘터 밖에서는 배정된(없으면 가장 가까운) 바리케이드의 판자만 노리고,
    /// 넘어 들어온 뒤에는 가장 가까운 살아 있는 플레이어를 끝까지 쫓는다. 밖에서 판자 너머 플레이어를 직접 공격하지 않는다.
    /// </summary>
    public sealed class ShelterAssaultTargetSelector : IEnemyTargetSelector
    {
        private readonly IPlayerTargetProvider _playerProvider;
        private readonly IBarricadeProvider _barricadeProvider;

        public ShelterAssaultTargetSelector(IPlayerTargetProvider playerProvider, IBarricadeProvider barricadeProvider)
        {
            _playerProvider = playerProvider;
            _barricadeProvider = barricadeProvider;
        }

        public EnemyTargetDecision SelectTarget(in EnemyTargetContext context)
        {
            if (context.IsInsideShelter)
            {
                _playerProvider.TryGetNearestPlayer(context.Origin, float.PositiveInfinity, out IEnemyTarget player);
                return new EnemyTargetDecision(player);
            }

            if (context.CurrentTarget is BarricadeTarget current && current.Barricade != null && current.Barricade.isActiveAndEnabled)
                return new EnemyTargetDecision(current);

            Barricade barricade = context.AssignedBarricade;
            if ((barricade == null || !barricade.isActiveAndEnabled) &&
                !_barricadeProvider.TryGetNearestBarricade(context.Origin, out barricade))
                return default;

            return new EnemyTargetDecision(new BarricadeTarget(barricade, barricade.ClaimAttackSlot(context.Origin)));
        }
    }
}
