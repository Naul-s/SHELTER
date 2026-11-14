using Shelter.Difficulty;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMainUI : MonoBehaviour
{
    [SerializeField] private Button btn_Start;
    [SerializeField] private Button btn_Exit;

    [Header("난이도 선택 - 게임 시작을 누르면 시작/종료 버튼 대신 나타난다")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button btn_Normal;
    [SerializeField] private Button btn_Hard;
    [SerializeField] private DifficultyConfig difficultyConfig;
    [SerializeField] private string gameSceneName = "TestScene";

    void Start()
    {
        if (difficultyPanel != null) difficultyPanel.SetActive(false);

        btn_Start.onClick.AddListener(ShowDifficulty);
        if (btn_Normal != null) btn_Normal.onClick.AddListener(() => StartGame(DifficultyLevel.Normal));
        if (btn_Hard != null) btn_Hard.onClick.AddListener(() => StartGame(DifficultyLevel.Hard));

        btn_Exit.onClick.AddListener(() => {
            // 에디터에서는 플레이 모드를 끈다
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

            // 빌드(VR 기기 또는 PC)에서는 앱을 종료한다
#else
        Application.Quit();
#endif
        });
    }

    // 게임 시작 → NORMAL/HARD 버튼을 보여 준다. 난이도 버튼이 연결되지 않은 씬이면 Normal 로 바로 시작한다.
    private void ShowDifficulty()
    {
        if (difficultyPanel == null || btn_Normal == null || btn_Hard == null)
        {
            StartGame(DifficultyLevel.Normal);
            return;
        }

        btn_Start.gameObject.SetActive(false);
        btn_Exit.gameObject.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // 고른 난이도는 세션 설정으로 보관된다(다시 하기에도 유지). 타이틀로 돌아오면 다시 고른다.
    private void StartGame(DifficultyLevel level)
    {
        DifficultySelection.Select(level, difficultyConfig);
        SceneManager.LoadScene(gameSceneName);
    }
}
