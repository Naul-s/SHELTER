using TMPro;
using UnityEngine;

public class UI_Resource : MonoBehaviour
{
    [SerializeField] private PlayerInventoryManager playerInventoryManager;

    [SerializeField] private TextMeshProUGUI text_woodCount;
    [SerializeField] private TextMeshProUGUI text_woodWeight;

    [SerializeField] private TextMeshProUGUI text_ironCount;
    [SerializeField] private TextMeshProUGUI text_ironWeight;

    [SerializeField] private TextMeshProUGUI text_steelCount;
    [SerializeField] private TextMeshProUGUI text_steelWeight;

    [SerializeField] private TextMeshProUGUI text_goldCount;
    [SerializeField] private TextMeshProUGUI text_goldWeight;

    [SerializeField] private TextMeshProUGUI text_moneyCount;
    [SerializeField] private TextMeshProUGUI text_moneyWeight;

    private void OnEnable()
    {
        if (playerInventoryManager.playerStats != null)
        {
            playerInventoryManager.playerStats.OnStatChanged += UI_Refresh;
        }
        UI_Refresh();
    }
    private void OnDisable()
    {
        if (playerInventoryManager.playerStats != null)
        {
            playerInventoryManager.playerStats.OnStatChanged -= UI_Refresh;
        }
    }

    void UI_Refresh()
    {
        text_woodCount.text = "목재 : " + FormatNumber(playerInventoryManager.playerResource.wood);
        text_woodWeight.text = "무게 : " + FormatNumber((playerInventoryManager.playerResource.wood * playerInventoryManager.playerResource.weightOneWood));

        text_ironCount.text = "   철 : " + FormatNumber(playerInventoryManager.playerResource.iron);
        text_ironWeight.text = "무게 : " + FormatNumber((playerInventoryManager.playerResource.iron * playerInventoryManager.playerResource.weightOneIron));

        text_steelCount.text = "강철 : " + FormatNumber(playerInventoryManager.playerResource.steel);
        text_steelWeight.text = "무게 : " + FormatNumber((playerInventoryManager.playerResource.steel * playerInventoryManager.playerResource.weightOneSteel));

        text_goldCount.text = "   금 : " + FormatNumber(playerInventoryManager.playerResource.gold);
        text_goldWeight.text = "무게 : " + FormatNumber((playerInventoryManager.playerResource.gold * playerInventoryManager.playerResource.weightOneGold));

        text_moneyCount.text = "   돈 : " + FormatNumber(playerInventoryManager.playerResource.money);
        text_moneyWeight.text = "무게 : " + FormatNumber((playerInventoryManager.playerResource.money * playerInventoryManager.playerResource.weightOneMoney));
    }

    string FormatNumber(float value)
    {
        // 소수점 1자리 밑으로는 무조건 버림(내림) 처리
        float truncated = Mathf.Floor(value * 10f) / 10f;

        // 1로 나눈 나머지가 거의 0이면 정수로 출력
        if (Mathf.Abs(truncated % 1f) < 0.0001f)
        {
            return truncated.ToString("0");
        }

        // 소수가 남아있으면 소수점 1자리로 출력
        return truncated.ToString("0.0");
    }
}
