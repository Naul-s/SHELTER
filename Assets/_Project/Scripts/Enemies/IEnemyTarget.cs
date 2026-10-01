using Shelter.Core;
using UnityEngine;

namespace Shelter.Enemies
{
    /// <summary>적이 추적/공격할 수 있는 대상(쉘터 타겟 포인트 또는 플레이어)에 대한 시점.</summary>
    public interface IEnemyTarget
    {
        Transform Transform { get; }
        IDamageable Damageable { get; }
        EnemyTargetKind Kind { get; }
    }
}
