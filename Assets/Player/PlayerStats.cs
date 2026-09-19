using System;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class P_Stats
{
    [SerializeField] private int _hp = 0;
    [SerializeField] private int _def = 0;
    [SerializeField] private int _str = 0;
    [SerializeField] private int _dex = 0;
    [SerializeField] private int _int = 0;
    [SerializeField] private float _speed = 0;
    [SerializeField] private float _weight = 0;
    [SerializeField] private int _exp = 0;
    [SerializeField] private int _sp = 0;

    public int HP { get { return _hp; } set { _hp = Mathf.Max(0, value); } }
    public int DEF { get { return _def; } set { _def = value; } }
    public int STR { get { return _str; } set { _str = value; } }
    public int DEX { get { return _dex; } set { _dex = value; } }
    public int INT { get { return _int; } set { _int = value; } }
    public float Speed { get { return _speed; } set { _speed = value; } }
    public float Weight { get { return _weight; } set { _weight = value; } }
    public int _EXP { get { return _exp; } set { _exp = value; } }
    public int _SP { get { return _sp; } set { _sp = value; } }
}

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerLevelData levelData;
    [SerializeField] private PlayerSkillManager skillManager;

    [SerializeField] private int _level = 0;
    [SerializeField] private P_Stats stats_base = new P_Stats();// 레벨별스탯 = 기초스탯
    [SerializeField] private P_Stats stats_total = new P_Stats();// 기초스탯에 스킬, 장비 등 부가스탯이 더해진 스탯 = 기본스탯
    [SerializeField] private P_Stats stats_current = new P_Stats();// 체력변화, 경험치획득, 버프등으로 변화한 스탯 = 현재 스탯

    public int SP
    {
        get { return stats_current._SP; }
        set { 
            stats_current._SP = value;
            OnSPChanged?.Invoke(stats_current._SP);
        }
    }
    public event Action<int> OnSPChanged;

    public int Level
    {
        get { return _level; }
        set
        {
            _level = value;
            if (_level > 30) _level = 30;
            ONLevelChanged?.Invoke(_level);
        }
    }
    public event Action<int> ONLevelChanged;

    public int Exp
    {
        get { return stats_current._EXP; }
        set
        {
            stats_current._EXP = value;
            stats_total._EXP += value;
            if (stats_current._EXP >= stats_base._EXP)
            {
                Level += 1;
                stats_current._EXP = 0;// 레벨업 시 경험치 초기화
            }
        }
    }

    private void OnEnable()
    {
        ONLevelChanged += LevelUp;
    }
    private void OnDisable()
    {
        ONLevelChanged -= LevelUp;
    }

    void Start()
    {
        Exp += 100;
        stats_current.HP = stats_total.HP;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Exp += 1000;
        }
    }

    void LevelUp(int _level)
    {
        SetBaseStats(_level);
    }

    void SetBaseStats(int _level)
    {
        if (_level >= levelData.levels.Length - 1) return;

        P_Stats _stats = levelData.levels[_level];

        stats_base.HP = _stats.HP;
        stats_base.DEF = _stats.DEF;
        stats_base.STR = _stats.STR;
        stats_base.DEX = _stats.DEX;
        stats_base.Weight = _stats.Weight;

        // 다음 레벨업에 필요한 경험치 획득량 가져오기
        stats_base._EXP = levelData.levels[_level + 1]._EXP;

        Calculate_TotalStats();

        stats_total._SP += _stats._SP;
        SP += _stats._SP;

        // 레벨업 시 늘어난 HP만큼 회복(스킬이나 장비로 늘어나는 HP는 회복 보너스 없음)
        int _levelUpHeal = levelData.levels[_level].HP - levelData.levels[_level - 1].HP;
        if (stats_current.HP + _levelUpHeal > stats_total.HP)
            stats_current.HP = stats_total.HP;
        else
            stats_current.HP += _levelUpHeal;
    }

    public void Calculate_TotalStats()
    {
        stats_total.HP = stats_base.HP + skillManager.total_stats.HP;
        stats_total.DEF = stats_base.DEF + skillManager.total_stats.DEF;
        stats_total.STR = stats_base.STR + skillManager.total_stats.STR;
        stats_total.DEX = stats_base.DEX + skillManager.total_stats.DEX;
        stats_total.INT = stats_base.INT + skillManager.total_stats.INT;
        stats_total.Speed = stats_base.Speed + skillManager.total_stats.Speed;
        stats_total.Weight = stats_base.Weight + skillManager.total_stats.Weight;
    }
}
