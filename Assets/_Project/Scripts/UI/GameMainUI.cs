using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMainUI : MonoBehaviour
{
    [SerializeField] private Button btn_Start;
    [SerializeField] private Button btn_Exit;

    void Start()
    {
        btn_Start.onClick.AddListener(() => {
            SceneManager.LoadScene("TestScene");
        });
        btn_Exit.onClick.AddListener(() => {
            // 에디터에서 플레이 중일 때는 에디터 재생을 멈춤
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

            // 빌드된 실제 게임(VR 기기 또는 PC 빌드) 실행 중일 때 종료
#else
        Application.Quit();
#endif
        });
    }
}
