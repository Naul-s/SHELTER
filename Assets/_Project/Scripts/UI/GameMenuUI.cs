using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Shelter.Rounds;
using TMPro;

public class GameMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI text_GameOver;
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
        text_CurrentRound.text = "Round : " + roundNumber;
    }

    public void MenuSetActive()
    {
        gameOverCanvas.SetActive(!gameOverCanvas.activeSelf);
        text_GameOver.gameObject.SetActive(false);
        text_CurrentRound.text = "Round : " + _roundManager.CurrentRound;
    }
}
