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
    }

    private void OnDisable()
    {
        _roundManager.Defeated -= Defeated;
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
