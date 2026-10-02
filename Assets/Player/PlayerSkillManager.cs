using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    //public static PlayerSkillManager Instance { get; private set; }
    public PlayerStats playerStats;

    public P_Stats total_stats;
    public SkillEffect[] total_effects;

    [SerializeField] private TextMeshProUGUI text_curSP;
    [SerializeField] private TextMeshProUGUI text_Level;

    [SerializeField] private Button btn_A; 
    [SerializeField] private Button btn_G;
    [SerializeField] private Button btn_H;
    [SerializeField] private GameObject node_A;
    [SerializeField] private GameObject node_G;
    [SerializeField] private GameObject node_H;

    private void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    // 씬이 바뀌어도 유지하고 싶다면 아래 주석 해제 (단일 씬이라면 없어도 무방)
        //    // DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }

    private readonly Color activeColor = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
    private readonly Color inactiveColor = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);

    private void Start()
    {
        btn_A.onClick.AddListener(() => SelectTab(node_A, btn_A));
        btn_G.onClick.AddListener(() => SelectTab(node_G, btn_G));
        btn_H.onClick.AddListener(() => SelectTab(node_H, btn_H));

        // 기본 탭 선택
        SelectTab(node_A, btn_A);
    }

    private void SelectTab(GameObject targetPage, Button targetButton)
    {
        // 모든 페이지 비활성화 및 색상 초기화
        node_A.SetActive(node_A == targetPage);
        node_G.SetActive(node_G == targetPage);
        node_H.SetActive(node_H == targetPage);

        btn_A.image.color = (btn_A == targetButton) ? activeColor : inactiveColor;
        btn_G.image.color = (btn_G == targetButton) ? activeColor : inactiveColor;
        btn_H.image.color = (btn_H == targetButton) ? activeColor : inactiveColor;
    }

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

    private void UI_Refresh()
    {
        text_curSP.text = "SP : " + playerStats.CurrentStats.SP.ToString();
        text_Level.text = "LV : " + playerStats.Level.ToString();
    }

    public void Set_Total_Skill_Effects(P_Stats _stats, SkillEffect[] _effects)
    {
        total_stats.HP += _stats.HP;
        total_stats.DEF += _stats.DEF;
        total_stats.STR += _stats.STR;
        total_stats.DEX += _stats.DEX;
        total_stats.INT += _stats.INT;
        total_stats.Speed += _stats.Speed;
        total_stats.Weight += _stats.Weight;
        total_stats.EXP += _stats.EXP;
        total_stats.SP += _stats.SP;
        playerStats.Calculate_Stats();

        for (int i = 0; i < total_effects.Length; i++)
        {
            for (int j = 0; j < _effects.Length; j++)
            {
                if (total_effects[i].effect == _effects[j].effect)
                {
                    total_effects[i].value += _effects[j].value;
                }
            }
        }

    }
}
