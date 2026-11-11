using Shelter.Rounds;
using UnityEngine;
using UnityEngine.UI;

public class SkipIntermission : MonoBehaviour
{
    [SerializeField] private Button btn_Skip;
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private RoundManager _roundManager;
    [SerializeField] private float changeSeconds = 3f;

    void Start()
    {
        btn_Skip.onClick.AddListener(() => {
            _roundManager.ChangeIntermission(changeSeconds);
            blockingPanel.SetActive(true);
        });
    }

    void Update()
    {
        bool show = ShouldShow();
        if (btn_Skip.gameObject.activeSelf != show) btn_Skip.gameObject.SetActive(show);
    }

    private bool ShouldShow()
    {
        if (_roundManager.State != RoundState.Intermission) return false;
        if (_roundManager.IntermissionTimeRemaining <= changeSeconds - 0.5f)
        {
            blockingPanel.SetActive(false);
            return false;
        }
        return true;
    }
}
