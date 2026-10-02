using System;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "SO/Player Skill Data")]
public class PlayerSkillData : ScriptableObject
{
    public enum Tier
    {
        tier1, tier2, tier3, tier4
    }
    [Header("스킬 티어")]
    public Tier tier;

    [Header("스킬 이름")]
    public string skillName = string.Empty;

    [Header("스킬 활성화 시 스탯 증가량")]
    public P_Stats stats;
    [Header("스킬 활성화 시 효과, 스킬 이름에 '(%)'가 붙어있지 않은 스킬은 value값을 1로 지정")]
    public SkillEffect[] effects;

    [Header("스킬 활성화 비용")]
    public int SP_cost = 0;
}




[System.Serializable]
public class SkillEffect
{
    public EffectType effect;
    public float value;

    public enum EffectType
    {
        // 건축가
        [InspectorName("건축 소모 자재 (%)")] BUILDING_MATERIAL_CONSUMPTION_REDUCTION_percent,
        [InspectorName("해체 시 자재 회수율 (%)")] DISMANTLE_RESOURCE_RECOVERY_RATE_percent,
        [InspectorName("오브젝트 수리 효율 (%)")] OBJECT_REPAIR_EFFICIENCY_percent,
        [InspectorName("오브젝트 전력 소모 (%)")] OBJECT_POWER_CONSUMPTION_percent,
        [InspectorName("오브젝트 내구도 (%)")] OBJECT_DURABILITY_percent,
        [InspectorName("5초마다 오브젝트 체력 회복 (%)")] OBJECT_HEALTH_REGENERATION_percent,

        // 채집가
        [InspectorName("채집 보너스 (%)")] GATHERING_BONUS_percent,
        [InspectorName("채집 보너스 (%) (나무, 광물 한정)")] WOOD_MINERAL_GATHERING_BONUS_percent,
        [InspectorName("10초간 이동속도 25% (재사용 대기시간 2분)")] TEMPORARY_SPEED_BOOST_ON_USE,
        [InspectorName("주변 채집 가능 자원 위치 표시")] NEARBY_RESOURCE_MAP_MARKER,
        [InspectorName("맵 내 희귀자원, 특수 채집 포인트 표시")] RARE_RESOURCE_MAP_MARKER,
        [InspectorName("자원 무게 (%)")] RESOURCE_WEIGHT_REDUCTION_percent,

        // 사냥꾼
        [InspectorName("좀비에게 발각될 확률 감소")] ZOMBIE_DETECTION_CHANCE_REDUCTION,
        [InspectorName("일반 등급 좀비에 대한 데미지 (%)")] NORMAL_ZOMBIE_DAMAGE_BONUS_percent,
        [InspectorName("8초간 STR, DEX +15, 이동속도 15% (재사용 대기 90초)")] TEMPORARY_STATS_AND_SPEED_BUFF,
        [InspectorName("체력 20% 이하인 적 즉사 확률 부여")] LOW_HEALTH_EXECUTION_CHANCE,
        [InspectorName("다수의 적에게 둘러싸일 때 피해량, DEF 보너스")] SURROUNDED_DAMAGE_DEFENSE_BONUS,
        [InspectorName("야간 전투 데미지 (%)")] NIGHT_COMBAT_DAMAGE_BONUS_percent,
        [InspectorName("처치 누적 시 STR, DEX +5, 이동속도 +3% 중첩 버프(최대 5중첩)")] KILL_STACK_STATS_AND_SPEED_BUFF,
        [InspectorName("치명타 확률 (%)")] Critical_Rate_percent,

        // 해금
        [InspectorName("2티어 작업대 해금")] UNLOCK_WORKBENCH_TIER_2,
        [InspectorName("3티어 작업대 해금")] UNLOCK_WORKBENCH_TIER_3,
        [InspectorName("4티어 작업대 해금")] UNLOCK_WORKBENCH_TIER_4,

        [InspectorName("쉘터 업그레이드(1차) 해금")] UNLOCK_SHELTER_UPGRADE_STAGE_1,
        [InspectorName("쉘터 업그레이드(2차) 해금")] UNLOCK_SHELTER_UPGRADE_STAGE_2,

        [InspectorName("2티어 채집 무기 해금")] UNLOCK_GATHERING_TOOL_TIER_2,
        [InspectorName("3티어 채집 무기 해금")] UNLOCK_GATHERING_TOOL_TIER_3,

        [InspectorName("2티어 근접 무기 해금")] UNLOCK_MELEE_WEAPON_TIER_2,
        [InspectorName("3티어 근접 무기 해금")] UNLOCK_MELEE_WEAPON_TIER_3
    }
}

public static class EnumExtensions
{
    public static string GetInspectorName(this Enum enumValue)
    {
        Type enumType = enumValue.GetType();
        MemberInfo[] memberInfo = enumType.GetMember(enumValue.ToString());

        if (memberInfo.Length > 0)
        {
            var attrs = memberInfo[0].GetCustomAttributes(typeof(InspectorNameAttribute), false);
            if (attrs.Length > 0)
            {
                return ((InspectorNameAttribute)attrs[0]).displayName;
            }
        }
        return enumValue.ToString();
    }
}