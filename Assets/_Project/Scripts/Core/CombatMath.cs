using UnityEngine;

namespace Shelter.Core
{
    /// <summary>
    /// 플레이어-스탯 기획 문서의 방어력 공식을 코드 전역에서 공유하기 위한 유틸.
    /// 최종 피해량 = 공격 데미지 × 100 / (100 + 대상 방어력).
    /// </summary>
    public static class CombatMath
    {
        public static float ApplyDefense(float rawDamage, float defense)
        {
            if (rawDamage <= 0f) return 0f;
            return rawDamage * 100f / (100f + Mathf.Max(0f, defense));
        }
    }
}
