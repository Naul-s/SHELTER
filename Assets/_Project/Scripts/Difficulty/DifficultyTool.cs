using Shelter.Difficulty;
using Shelter.Economy;
using Shelter.Enemies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyToll : MonoBehaviour
{
    [Header("게임설정")]
    [SerializeField] private EconomyConfig economyConfig;// 시작 돈, 회복비용, 회복량
    [SerializeField] private TMP_InputField input_StartingMoney;
    [SerializeField] private TMP_InputField input_HealCost;
    [SerializeField] private Slider slider_HealFraction;

    [Header("난이도")]
    [SerializeField] private DifficultyConfig difficultyConfig;// 노말 하드의 배율
    [Header("Normal")]
    [SerializeField] private TMP_InputField input_Normal_EnemyHealth;
    [SerializeField] private TMP_InputField input_Normal_SpawnCount;
    [SerializeField] private TMP_InputField input_Normal_MaxAlive;
    [SerializeField] private TMP_InputField input_Normal_KillReward;
    [Header("Hard")]
    [SerializeField] private TMP_InputField input_Hard_EnemyHealth;
    [SerializeField] private TMP_InputField input_Hard_SpawnCount;
    [SerializeField] private TMP_InputField input_Hard_MaxAlive;
    [SerializeField] private TMP_InputField input_Hard_KillReward;

    [Header("무기")]
    [SerializeField] private ShopCatalog shopCatalog;// 상점가격, 탄약개수
    [SerializeField] private Weapon[] weapons;// 무기 데미지, 장전가격
    [Header("Dagger")]
    [SerializeField] private TMP_InputField input_Dagger_Damage;
    [Header("Pistol")]
    [SerializeField] private TMP_InputField input_Pistol_Damage;
    [SerializeField] private TMP_InputField input_Pistol_Ammo;
    [SerializeField] private TMP_InputField input_Pistol_Price;
    [SerializeField] private TMP_InputField input_Pistol_ReloadPrice;
    [Header("AR")]
    [SerializeField] private TMP_InputField input_AR_Damage;
    [SerializeField] private TMP_InputField input_AR_Ammo;
    [SerializeField] private TMP_InputField input_AR_Price;
    [SerializeField] private TMP_InputField input_AR_ReloadPrice;
    [Header("SMG")]
    [SerializeField] private TMP_InputField input_SMG_Damage;
    [SerializeField] private TMP_InputField input_SMG_Ammo;
    [SerializeField] private TMP_InputField input_SMG_Price;
    [SerializeField] private TMP_InputField input_SMG_ReloadPrice;

    [Header("좀비")]
    [SerializeField] private EnemyData[] enemyDatas;// 최대체력, 방어력, 이동속도, 공격력, 보상
    [Header("Walker")]
    [SerializeField] private TMP_InputField input_Walker_MaxHealth;
    [SerializeField] private TMP_InputField input_Walker_Defense;
    [SerializeField] private TMP_InputField input_Walker_MoveSpeed;
    [SerializeField] private TMP_InputField input_Walker_Attack;
    [SerializeField] private TMP_InputField input_Walker_KillReward;
    [Header("Runner")]
    [SerializeField] private TMP_InputField input_Runner_MaxHealth;
    [SerializeField] private TMP_InputField input_Runner_Defense;
    [SerializeField] private TMP_InputField input_Runner_MoveSpeed;
    [SerializeField] private TMP_InputField input_Runner_Attack;
    [SerializeField] private TMP_InputField input_Runner_KillReward;

    [SerializeField] private Button btn_Save;
    [SerializeField] private Button btn_OnOff;
    private bool isOnOff;
    [SerializeField] private GameObject CustomTool;

    void Start()
    {
        Off();

        btn_Save.onClick.AddListener(() => {
            Update_Setting();
        });
        btn_OnOff.onClick.AddListener(() => {
            if (isOnOff == true) On();
            else Off();
        });
    }

    void On()
    {
        isOnOff = false;
        btn_OnOff.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "숨기기";
        CustomTool.SetActive(true);
        Refresh_UI();
    }
    void Off()
    {
        isOnOff = true;
        btn_OnOff.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "보이기";
        CustomTool.SetActive(false);
    }

    private void OnEnable()
    {
        Refresh_UI();
    }

    void Update_Setting()
    {
        // 게임설정
        economyConfig.StartingMoney = int.Parse(input_StartingMoney.text);
        economyConfig.HealCost = int.Parse(input_HealCost.text);
        economyConfig.HealFraction = slider_HealFraction.value;

        // 난이도
        var Normal_EnemyHealth = difficultyConfig.Normal; Normal_EnemyHealth.EnemyHealthMultiplier = float.Parse(input_Normal_EnemyHealth.text); difficultyConfig.Normal = Normal_EnemyHealth;
        var Normal_SpawnCount = difficultyConfig.Normal; Normal_SpawnCount.SpawnCountMultiplier = float.Parse(input_Normal_SpawnCount.text); difficultyConfig.Normal = Normal_SpawnCount;
        var Normal_MaxAlive = difficultyConfig.Normal; Normal_MaxAlive.MaxAliveMultiplier = float.Parse(input_Normal_MaxAlive.text); difficultyConfig.Normal = Normal_MaxAlive;
        var Normal_KillReward = difficultyConfig.Normal; Normal_KillReward.KillRewardMultiplier = float.Parse(input_Normal_KillReward.text); difficultyConfig.Normal = Normal_KillReward;

        var Hard_EnemyHealth = difficultyConfig.Hard; Hard_EnemyHealth.EnemyHealthMultiplier = float.Parse(input_Hard_EnemyHealth.text); difficultyConfig.Hard = Hard_EnemyHealth;
        var Hard_SpawnCount = difficultyConfig.Hard; Hard_SpawnCount.SpawnCountMultiplier = float.Parse(input_Hard_SpawnCount.text); difficultyConfig.Hard = Hard_SpawnCount;
        var Hard_MaxAlive = difficultyConfig.Hard; Hard_MaxAlive.MaxAliveMultiplier = float.Parse(input_Hard_MaxAlive.text); difficultyConfig.Hard = Hard_MaxAlive;
        var Hard_KillReward = difficultyConfig.Hard; Hard_KillReward.KillRewardMultiplier = float.Parse(input_Hard_KillReward.text); difficultyConfig.Hard = Hard_KillReward;

        // 무기
        weapons[0].Damage = int.Parse(input_Dagger_Damage.text);

        weapons[1].Damage = int.Parse(input_Pistol_Damage.text);
        shopCatalog.Offers[0].Ammo = int.Parse(input_Pistol_Ammo.text);
        shopCatalog.Offers[0].Price = int.Parse(input_Pistol_Price.text);
        weapons[1].ReloadPrice = int.Parse(input_Pistol_ReloadPrice.text);

        weapons[2].Damage = int.Parse(input_AR_Damage.text);
        shopCatalog.Offers[2].Ammo = int.Parse(input_AR_Ammo.text);
        shopCatalog.Offers[2].Price = int.Parse(input_AR_Price.text);
        weapons[2].ReloadPrice = int.Parse(input_AR_ReloadPrice.text);

        weapons[3].Damage = int.Parse(input_SMG_Damage.text);
        shopCatalog.Offers[1].Ammo = int.Parse(input_SMG_Ammo.text);
        shopCatalog.Offers[1].Price = int.Parse(input_SMG_Price.text);
        weapons[3].ReloadPrice = int.Parse(input_SMG_ReloadPrice.text);

        // 좀비
        enemyDatas[0].MaxHealth = float.Parse(input_Walker_MaxHealth.text);
        enemyDatas[0].Defense = float.Parse(input_Walker_Defense.text);
        enemyDatas[0].MoveSpeed = float.Parse(input_Walker_MoveSpeed.text);
        enemyDatas[0].AttackPower = float.Parse(input_Walker_Attack.text);
        enemyDatas[0].KillReward = int.Parse(input_Walker_KillReward.text);

        enemyDatas[1].MaxHealth = float.Parse(input_Runner_MaxHealth.text);
        enemyDatas[1].Defense = float.Parse(input_Runner_Defense.text);
        enemyDatas[1].MoveSpeed = float.Parse(input_Runner_MoveSpeed.text);
        enemyDatas[1].AttackPower = float.Parse(input_Runner_Attack.text);
        enemyDatas[1].KillReward = int.Parse(input_Runner_KillReward.text);
    }

    void Refresh_UI()
    {
        // 게임설정
        input_StartingMoney.text = economyConfig.StartingMoney.ToString();
        input_HealCost.text = economyConfig.HealCost.ToString();
        slider_HealFraction.value = economyConfig.HealFraction;

        // 난이도
        input_Normal_EnemyHealth.text = difficultyConfig.Normal.EnemyHealthMultiplier.ToString();
        input_Normal_SpawnCount.text = difficultyConfig.Normal.SpawnCountMultiplier.ToString();
        input_Normal_MaxAlive.text = difficultyConfig.Normal.MaxAliveMultiplier.ToString();
        input_Normal_KillReward.text = difficultyConfig.Normal.KillRewardMultiplier.ToString();

        input_Hard_EnemyHealth.text = difficultyConfig.Hard.EnemyHealthMultiplier.ToString();
        input_Hard_SpawnCount.text = difficultyConfig.Hard.SpawnCountMultiplier.ToString();
        input_Hard_MaxAlive.text = difficultyConfig.Hard.MaxAliveMultiplier.ToString();
        input_Hard_KillReward.text = difficultyConfig.Hard.KillRewardMultiplier.ToString();

        // 무기
        input_Dagger_Damage.text = weapons[0].Damage.ToString();

        input_Pistol_Damage.text = weapons[1].Damage.ToString();
        input_Pistol_Ammo.text = shopCatalog.Offers[0].Ammo.ToString();
        input_Pistol_Price.text = shopCatalog.Offers[0].Price.ToString();
        input_Pistol_ReloadPrice.text = weapons[1].ReloadPrice.ToString();

        input_AR_Damage.text = weapons[2].Damage.ToString();
        input_AR_Ammo.text = shopCatalog.Offers[2].Ammo.ToString();
        input_AR_Price.text = shopCatalog.Offers[2].Price.ToString();
        input_AR_ReloadPrice.text = weapons[2].ReloadPrice.ToString();

        input_SMG_Damage.text = weapons[3].Damage.ToString();
        input_SMG_Ammo.text = shopCatalog.Offers[1].Ammo.ToString();
        input_SMG_Price.text = shopCatalog.Offers[1].Price.ToString();
        input_SMG_ReloadPrice.text = weapons[3].ReloadPrice.ToString();

        // 좀비
        input_Walker_MaxHealth.text = enemyDatas[0].MaxHealth.ToString();
        input_Walker_Defense.text = enemyDatas[0].Defense.ToString();
        input_Walker_MoveSpeed.text = enemyDatas[0].MoveSpeed.ToString();
        input_Walker_Attack.text = enemyDatas[0].AttackPower.ToString();
        input_Walker_KillReward.text = enemyDatas[0].KillReward.ToString();

        input_Runner_MaxHealth.text = enemyDatas[1].MaxHealth.ToString();
        input_Runner_Defense.text = enemyDatas[1].Defense.ToString();
        input_Runner_MoveSpeed.text = enemyDatas[1].MoveSpeed.ToString();
        input_Runner_Attack.text = enemyDatas[1].AttackPower.ToString();
        input_Runner_KillReward.text = enemyDatas[1].KillReward.ToString();
    }
}

