using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[System.Serializable]
public class P_Stats
{
    public event Action OnStatChanged;

    [SerializeField] private int _hp = 0;
    [SerializeField] private int _def = 0;
    [SerializeField] private int _str = 0;
    [SerializeField] private int _dex = 0;
    [SerializeField] private int _int = 0;
    [SerializeField] private float _speed = 0;
    [SerializeField] private float _weight = 0;
    [SerializeField] private int _exp = 0;
    [SerializeField] private int _sp = 0;

    public int HP
    {
        get => _hp;
        set { if (_hp != value) { _hp = Mathf.Max(0, value); OnStatChanged?.Invoke(); } }
    }
    public int DEF
    {
        get => _def;
        set { if (_def != value) { _def = value; OnStatChanged?.Invoke(); } }
    }
    public int STR
    {
        get => _str;
        set { if (_str != value) { _str = value; OnStatChanged?.Invoke(); } }
    }
    public int DEX
    {
        get => _dex;
        set { if (_dex != value) { _dex = value; OnStatChanged?.Invoke(); } }
    }
    public int INT
    {
        get => _int;
        set { if (_int != value) { _int = value; OnStatChanged?.Invoke(); } }
    }
    public float Speed
    {
        get => _speed;
        set { if (_speed != value) { _speed = value; OnStatChanged?.Invoke(); } }
    }
    public float Weight
    {
        get => _weight;
        set { if (_weight != value) { _weight = value; OnStatChanged?.Invoke(); } }
    }
    public int EXP
    {
        get => _exp;
        set { if (_exp != value) { _exp = value; OnStatChanged?.Invoke(); } }
    }
    public int SP
    {
        get => _sp;
        set { if (_sp != value) { _sp = value; OnStatChanged?.Invoke(); } }
    }
}

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerLevelData levelData;
    [SerializeField] private PlayerSkillManager skillManager;
    [SerializeField] private PlayerInventoryManager inventoryManager;
    [SerializeField] private DynamicMoveProvider dynamicMoveProvider;

    public const int MaxLevel = 30;
    [SerializeField] private int _level = 0;
    [SerializeField] private P_Stats stats_base = new P_Stats();// 레벨별스탯 = 기초스탯
    [SerializeField] private P_Stats stats_total = new P_Stats();// 기초스탯에 스킬, 장비 등 부가스탯이 더해진 스탯 = 기본스탯
    [SerializeField] private P_Stats stats_current = new P_Stats();// 체력변화, 경험치획득, 버프등으로 변화한 스탯 = 현재 스탯

    public P_Stats BaseStats => stats_base;
    public P_Stats TotalStats => stats_total;
    public P_Stats CurrentStats => stats_current;

    public int Level
    {
        get => _level;
        set
        {
            int clampedLevel = Mathf.Min(30, value);
            if (_level != clampedLevel)
            {
                _level = clampedLevel;
                OnStatChanged?.Invoke(); // 레벨이 올라도 같은 통합 이벤트 발송!
            }
        }
    }
    public event Action OnStatChanged;

    // 가방 최대 용량 같은 공식을 프로퍼티로 선언 (읽을 때 자동 계산)
    public float MaxCarryWeight => stats_total.Weight + (stats_total.STR * 2.0f);

    private void OnEnable()
    {
        stats_current.OnStatChanged += HandleStatChanged;
        stats_total.OnStatChanged += HandleStatChanged;
    }
    private void OnDisable()
    {
        stats_current.OnStatChanged -= HandleStatChanged;
        stats_total.OnStatChanged -= HandleStatChanged;
    }

    private void HandleStatChanged()
    {
        OnStatChanged?.Invoke();
    }

    void Start()
    {
        AddExp(0);
        stats_current.HP = stats_total.HP;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            //AddExp(100);
            LevelUp();
        }
    }

    /// <summary> 경험치 획득은 무조건 이 함수로 </summary>
    public void AddExp(int amount)
    {
        stats_current.EXP += amount;
        stats_total.EXP += amount;

        // 현재 경험치가 요구 경험치 이상이면 레벨업 처리
        while (stats_current.EXP >= stats_base.EXP)
        {
            stats_current.EXP -= stats_base.EXP; // 남은 경험치 이월
            LevelUp();
        }
    }
    private void LevelUp()
    {
        if (_level >= levelData.levels.Length - 1) return;

        Level += 1;

        P_Stats _stats = levelData.levels[_level];

        stats_base.HP = _stats.HP;
        stats_base.DEF = _stats.DEF;
        stats_base.STR = _stats.STR;
        stats_base.DEX = _stats.DEX;
        stats_base.INT = _stats.INT;
        stats_base.Speed = _stats.Speed;
        stats_base.Weight = _stats.Weight;

        if (_level < MaxLevel)
        {
            stats_base.EXP = levelData.levels[_level + 1].EXP;// 다음 레벨업에 필요한 경험치 획득량 가져오기
        }

        Calculate_Stats();

        stats_total.SP += _stats.SP;
        stats_current.SP += _stats.SP;

        // 레벨업 시 늘어난 HP만큼 회복(스킬이나 장비로 늘어나는 HP는 회복 보너스 없음)
        int _levelUpHeal = levelData.levels[_level].HP - levelData.levels[_level - 1].HP;
        if (stats_current.HP + _levelUpHeal > stats_total.HP)
            stats_current.HP = stats_total.HP;
        else
            stats_current.HP += _levelUpHeal;
    }

    public void Calculate_Stats()
    {
        // 계산의 기초스탯이 될 스탯 합산
        int _HP = stats_base.HP + skillManager.total_stats.HP;
        int _DEF = stats_base.DEF + skillManager.total_stats.DEF;
        int _STR = stats_base.STR + skillManager.total_stats.STR;
        int _DEX = stats_base.DEX + skillManager.total_stats.DEX;
        int _INT = stats_base.INT + skillManager.total_stats.INT;
        float _Speed = skillManager.total_stats.Speed;
        float _Weight = stats_base.Weight + skillManager.total_stats.Weight;

        // current 스탯 계산
        float resourceWeight = 0;
        resourceWeight += inventoryManager.playerResource.wood * inventoryManager.playerResource.weightOneWood;
        resourceWeight += inventoryManager.playerResource.iron * inventoryManager.playerResource.weightOneIron;
        resourceWeight += inventoryManager.playerResource.steel * inventoryManager.playerResource.weightOneSteel;
        resourceWeight += inventoryManager.playerResource.gold * inventoryManager.playerResource.weightOneGold;
        resourceWeight += inventoryManager.playerResource.money * inventoryManager.playerResource.weightOneMoney;
        stats_current.Weight = resourceWeight;

        // total 스탯의 계산식 적용
        stats_total.HP = _HP;
        stats_total.DEF = _DEF;
        stats_total.STR = _STR;
        stats_total.DEX = _DEX;
        stats_total.INT = _INT;
        stats_total.Weight = _Weight + (_STR * 2);
        if (stats_current.Weight <= stats_total.Weight)
            stats_total.Speed = stats_base.Speed * (1 + _Speed * 0.01f);
        else
        {
            float overWeight = stats_current.Weight - stats_total.Weight;
            float overRate = overWeight / stats_total.Weight;
            float penalty = Mathf.Max(0f, 1 - (overRate * 0.5f));// 초과율 10%당 이동 속도 5% 감소
            stats_total.Speed = stats_base.Speed * (1 + _Speed * 0.01f) * penalty;
        }
        dynamicMoveProvider.moveSpeed = stats_total.Speed;// 이동속도 변경
    }
}
