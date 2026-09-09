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
    public int EXP { get { return _exp; } set { _exp = value; } }
    public int SP { get { return _sp; } set { _sp = value; } }
}

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerLevelData levelData;

    [SerializeField] private int Player_Level = 0;
    [SerializeField] private P_Stats stats_base = new P_Stats();// 레벨별스탯 = 기초스탯
    [SerializeField] private P_Stats stats_total = new P_Stats();// 기초스탯에 무기, 장비 등 부가스탯이 더해진 스탯 = 기본스탯
    [SerializeField] private P_Stats stats_current = new P_Stats();// 체력변화, 경험치획득, 버프등으로 변화한 스탯 = 현재 스탯

    void Start()
    {
        
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 경험치 획득
            stats_current.EXP += 30;
            if (stats_current.EXP >= stats_base.EXP)
            {
                LevelUp();
            }
        }
    }

    void LevelUp()
    {
        if (Player_Level >= 30) return;

        Player_Level += 1;
        SetBaseStats(Player_Level);
    }

    void SetBaseStats(int _level)
    {
        P_Stats _stats = levelData.levels[_level];

        stats_base.HP = _stats.HP;
        stats_base.DEF = _stats.DEF;
        stats_base.STR = _stats.STR;
        stats_base.DEX = _stats.DEX;
        stats_base.Weight = _stats.Weight;

        // 다음 레벨업에 필요한 경험치 획득량 가져오기
        if (_level < levelData.levels.Length) {
            stats_base.EXP = levelData.levels[_level + 1].EXP;
        }

        stats_total.SP += _stats.SP;
        stats_current.SP += _stats.SP;


        stats_current.EXP = 0;// 레벨업 시 경험치 초기화
        // 레벨업 시 늘어난 HP만큼 회복
        int _levelUpHeal = levelData.levels[_level].HP - levelData.levels[_level - 1].HP; Debug.Log(_levelUpHeal);
        if (stats_current.HP + _levelUpHeal > stats_total.HP)
            stats_current.HP = stats_total.HP;
        else
            stats_current.HP += _levelUpHeal;


    }
}
