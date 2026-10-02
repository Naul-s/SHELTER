using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryManager : MonoBehaviour
{
    public PlayerStats playerStats;
    public Resource playerResource;

    [SerializeField] private TextMeshProUGUI text_maxWeight;

    [SerializeField] private Button btn_Resource;
    [SerializeField] private Button btn_Weapon;
    [SerializeField] private Button btn_Item;
    [SerializeField] private GameObject page_Resource;
    [SerializeField] private GameObject page_Weapon;
    [SerializeField] private GameObject page_Item;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged += UI_Refresh;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= UI_Refresh;
        }
    }

    void UI_Refresh()
    {
        text_maxWeight.text = "무게/가방 용량 " + playerStats.CurrentStats.Weight.ToString() + " / " + playerStats.TotalStats.Weight.ToString();
        if (playerStats.CurrentStats.Weight > playerStats.TotalStats.Weight)
        {
            float overWeight = playerStats.CurrentStats.Weight - playerStats.TotalStats.Weight;
            float overRate = overWeight / playerStats.TotalStats.Weight;
            float penalty = Mathf.Max(0f, 1 - (overRate * 0.5f));// 초과율 10%당 이동 속도 5% 감소
            text_maxWeight.color = new Color(1, penalty, penalty);
        }
        else
            text_maxWeight.color = Color.white;
    }

    private readonly Color activeColor = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
    private readonly Color inactiveColor = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);

    private void Start()
    {
        btn_Resource.onClick.AddListener(() => SelectTab(page_Resource, btn_Resource));
        btn_Weapon.onClick.AddListener(() => SelectTab(page_Weapon, btn_Weapon));
        btn_Item.onClick.AddListener(() => SelectTab(page_Item, btn_Item));

        // 기본 탭 선택
        SelectTab(page_Resource, btn_Resource);
    }

    private void SelectTab(GameObject targetPage, Button targetButton)
    {
        // 모든 페이지 비활성화 및 색상 초기화
        page_Resource.SetActive(page_Resource == targetPage);
        page_Weapon.SetActive(page_Weapon == targetPage);
        page_Item.SetActive(page_Item == targetPage);

        btn_Resource.image.color = (btn_Resource == targetButton) ? activeColor : inactiveColor;
        btn_Weapon.image.color = (btn_Weapon == targetButton) ? activeColor : inactiveColor;
        btn_Item.image.color = (btn_Item == targetButton) ? activeColor : inactiveColor;
    }


    public void AddResource(Resource resource)
    {
        playerResource.wood += resource.wood;
        playerResource.iron += resource.iron;
        playerResource.steel += resource.steel;
        playerResource.gold += resource.gold;
        playerResource.money += resource.money;

        playerStats.Calculate_Stats();
    }
}
