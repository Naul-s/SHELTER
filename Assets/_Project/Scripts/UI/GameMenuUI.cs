using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Shelter.Rounds;
using TMPro;

public class GameMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI text_GameOver;
    [Tooltip("일시정지로 열었을 때 보이는 제목(게임오버 제목 대신).")]
    [SerializeField] private TextMeshProUGUI text_Paused;
    [SerializeField] private TextMeshProUGUI text_CurrentRound;
    [SerializeField] private Button btn_ReStart;
    [SerializeField] private Button btn_Main;

    [SerializeField] private RoundManager _roundManager;
    [Header("게임 클리어 — 게임오버와 같은 패널을 제목·문구만 바꿔 띄운다")]
    [SerializeField] private string _clearTitle = "GAME CLEAR";
    [SerializeField] private Color _clearTitleColor = new Color(0.95f, 0.82f, 0.35f, 1f);
    [Tooltip("{0} = 클리어한 라운드 수")]
    [SerializeField] private string _clearMessage = "{0} 라운드를 모두 버텼습니다!";

    /// <summary>게임이 끝났다(패배 또는 클리어). 끝난 뒤에는 일시정지 메뉴로 패널을 닫지 못하게 한다(VRControllerInput).</summary>
    public bool isDefeated = false;

    void Start()
    {
        gameOverCanvas.SetActive(false);
        btn_ReStart.onClick.AddListener(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        btn_Main.onClick.AddListener(() => {
            SceneManager.LoadScene("Main");
        });
    }

    private void OnEnable()
    {
        _roundManager.Defeated += Defeated;
        _roundManager.AllRoundsCleared += Cleared;
    }

    private void OnDisable()
    {
        _roundManager.Defeated -= Defeated;
        _roundManager.AllRoundsCleared -= Cleared;
    }

    // 10라운드 클리어: 게임오버 패널(플레이어 앞, WorldMenuPlacer)을 클리어 제목으로 띄운다. 버튼은 같다(다시 하기 · 타이틀).
    void Cleared()
    {
        isDefeated = true;
        gameOverCanvas.SetActive(true);
        text_GameOver.gameObject.SetActive(true);
        text_GameOver.SetText(_clearTitle);
        text_GameOver.color = _clearTitleColor;
        if (text_Paused != null) text_Paused.gameObject.SetActive(false);
        text_CurrentRound.SetText(_clearMessage, _roundManager.MaxRounds);
    }

    void Defeated(int roundNumber)
    {
        isDefeated = true;
        gameOverCanvas.SetActive(true);
        text_GameOver.gameObject.SetActive(true);
        if (text_Paused != null) text_Paused.gameObject.SetActive(false);
        text_CurrentRound.SetText("{0} 라운드에서 쓰러졌습니다", roundNumber);
    }

    public void MenuSetActive()
    {
        gameOverCanvas.SetActive(!gameOverCanvas.activeSelf);
        text_GameOver.gameObject.SetActive(false);
        if (text_Paused != null) text_Paused.gameObject.SetActive(true);
        text_CurrentRound.SetText("현재 {0} 라운드", _roundManager.CurrentRound);
    }
}
