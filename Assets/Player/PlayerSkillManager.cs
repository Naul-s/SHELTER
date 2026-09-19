using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    //public static PlayerSkillManager Instance { get; private set; }
    public PlayerStats PlayerStats;

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

    private void Start()
    {
        btn_A.onClick.AddListener(() => {
            btn_A.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
            btn_G.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
            btn_H.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
            node_A.SetActive(true);
            node_G.SetActive(false);
            node_H.SetActive(false);
        });
        btn_G.onClick.AddListener(() => {
             btn_A.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
             btn_G.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
             btn_H.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
            node_A.SetActive(false);
            node_G.SetActive(true);
            node_H.SetActive(false);
        });
        btn_H.onClick.AddListener(() => {
            btn_A.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
            btn_G.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
            btn_H.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
            node_A.SetActive(false);
            node_G.SetActive(false);
            node_H.SetActive(true);
        });

        btn_A.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 192 / 255f);
        btn_G.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
        btn_H.image.color = new Color(80 / 255f, 80 / 255f, 80 / 255f, 128 / 255f);
        node_A.SetActive(true);
        node_G.SetActive(false);
        node_H.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerStats.OnSPChanged += UI_Update_curSP;
        UI_Update_curSP(PlayerStats.SP);
        PlayerStats.ONLevelChanged += UI_Update_Level;
        UI_Update_Level(PlayerStats.Level);
    }

    private void OnDisable()
    {
        PlayerStats.OnSPChanged -= UI_Update_curSP;
        PlayerStats.ONLevelChanged -= UI_Update_curSP;
    }

    void UI_Update_curSP(int _sp)
    {
        text_curSP.text = "SP : " + _sp.ToString();
    }

    void UI_Update_Level(int _level)
    {
        text_Level.text = "LV : " + _level.ToString();
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
        total_stats._EXP += _stats._EXP;
        total_stats._SP += _stats._SP;
        PlayerStats.Calculate_TotalStats();

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
