using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Hierarchy;
using Shelter.Rounds;
using Shelter.Economy;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private Button btn_tutorial;
    [SerializeField] private TextMeshProUGUI text_tutorial;
    [SerializeField] private TextMeshProUGUI text_WellText;
    [SerializeField] private Button btn_next;
    [SerializeField] private SkipIntermission skipIntermission;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private WeaponShop weaponShop;

    [SerializeField] private GameObject player_ArmMesh;

    [SerializeField] private VRController vrController;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject[] lookObject;
    [SerializeField] private GameObject rightControllerVisual;
    [SerializeField] private GameObject leftControllerVisual;

    public static Action OnNextStep;
    public static int CurrentStep { get; private set; }
    private bool waitForNext;
    public bool check_StickY;
    public bool check_Stick_Y;
    public bool check_StickX;
    public bool check_Stick_X;

    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private Transform handBackBone;

    void Start()
    {
        CurrentStep = -1;

        btn_tutorial.onClick.AddListener(() => {
            if (CurrentStep == -1)
            {
                CurrentStep = 0;
                SetStep(0);
                text_tutorial.text = "튜토리얼 종료";
                rightControllerVisual.SetActive(true);
                leftControllerVisual.SetActive(true);
                blockingPanel.SetActive(true);
                btn_tutorial.GetComponent<RectTransform>().localPosition = new Vector3(-1656, 903, 0);
                skipIntermission.blockingPanel.SetActive(true);
            }
            else if (CurrentStep >= 0)
            {
                Tutorial_Exit();
            }
        });

        for (int i = 0; i < lookObject.Length; i++)
        {
            lookObject[i].gameObject.SetActive(false);
        }
        rightControllerVisual.SetActive(false);
        leftControllerVisual.SetActive(false);
        btn_next.gameObject.SetActive(false);

        skipIntermission.btn_Skip.onClick.AddListener(() => { 
            skipIntermission.blockingPanel.SetActive(false); 
            gameObject.SetActive(false); 
        });

        btn_next.onClick.AddListener(() => {
            if (CurrentStep < 0) return;

            if (waitForNext == false)
            {
                Next();
            }
        });
    }

    void Tutorial_Exit()
    {
        for (int i = 0; i < lookObject.Length; i++)
        {
            lookObject[i].gameObject.SetActive(false);
        }
        rightControllerVisual.SetActive(false);
        leftControllerVisual.SetActive(false);
        blockingPanel.SetActive(false);
        CurrentStep = -2;
        skipIntermission.blockingPanel.SetActive(false);
        text_WellText.gameObject.SetActive(false);
        btn_next.gameObject.SetActive(false);
        player_ArmMesh.SetActive(true);
        weaponShop.ChangeMaterial_OriginalPistol();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (CurrentStep < 0) return;

        if (roundManager.CurrentRound > 0)
            Tutorial_Exit();

        switch (CurrentStep)
        {
            case 0:// 조이스틱을 움직여 이동하세요 
                player_ArmMesh.SetActive(false);
                text_WellText.text = "튜토리얼 진행 중에는 벽면과 컨트롤러 주변에 표시되는 안내 따라 조작해 보세요";
                if (waitForNext == false)
                {
                    if (vrController.LeftStickInput.y > 0.7f) check_StickY = true;
                    if (vrController.LeftStickInput.y < -0.7f) check_Stick_Y = true;
                    if (vrController.LeftStickInput.x > 0.7f) check_StickX = true;
                    if (vrController.LeftStickInput.x < -0.7f) check_Stick_X = true;
                }
                if (check_StickX && check_StickY && check_Stick_X && check_Stick_Y)
                {
                    check_StickY = false; check_StickX = false; check_Stick_Y = false; check_Stick_X = false;
                    NextStep();
                }
                break;
            case 1:// 조이스틱을 양옆으로 튕겨 화면을 회전하세요 
                text_WellText.text = "";
                if (waitForNext == false)
                {
                    if (vrController.RightStickInput.x > 0.7f) check_StickX = true;
                    if (vrController.RightStickInput.x < -0.7f) check_Stick_X = true;
                }
                if (check_StickX && check_Stick_X)
                {
                    check_StickX = false; check_Stick_X = false;
                    NextStep();
                }
                break;
            case 2:// 손등에 체력과 돈이 표시됩니다 
                if (waitForNext == false && IsLookingAtHandBack())
                {
                    NextStep();
                }
                break;
            case 3:// 컨트롤러 레이로 권총을 가리키고 트리거를 당겨 구매하세요
                blockingPanel.SetActive(false);
                break;
            case 4:// 무기를 가리키고 트리거를 당겨 무기를 잡아보세요 
                break;
            case 5:// 무기를 쥔 상태에서는 UI를 클릭할 수 없어요
                text_WellText.text = "<color=#E9B055>무기를 쥔 상태</color>에서는 UI를 선택할 수 없어요\n<color=#E9B055>다른 손을 사용</color>하여 [확인]버튼을 눌러보세요";
                btn_next.gameObject.SetActive(true);
                break;
            case 6:// 트리거를 당겨 총을 쏘세요
                text_WellText.text = "컨트롤러 주변에 표시되는 안내 따라 조작해 보세요";
                btn_next.gameObject.SetActive(false);
                break;
            case 7:// 조이스틱 왼쪽 아래의 버튼을 눌러 재장전을 하세요 
                text_WellText.text = "";
                break;
            case 8:// 무기를 쥐고 허리 옆으로 가져가 진동이 느껴지면 트리거를 놓아 무기를 장착해보세요
                break;
            case 9:// 웨이브가 시작되면 바깥에서 좀비가 몰려와 판자를 부수고 들어옵니다
                text_WellText.text = "<color=#C82828>웨이브가 시작</color>되면 바깥에서 <color=#C82828>좀비</color>가 몰려와 <color=#50DC50>판자</color>를 <color=#C82828>부수고</color> 들어옵니다";
                btn_next.gameObject.SetActive(true);
                break;
            case 10:// 웨이브 종료 후 준비시간에는 판자에 가까이 다가가면 돈을 써 수리할 수 있습니다
                text_WellText.text = "<color=#50DC50>웨이브 종료</color> 후 <color=#50DC50>준비시간</color>에는 <color=#50DC50>판자</color>에 <color=#E9B055>가까이 다가가</color>면 <color=#F2C255>돈</color>을 써 <color=#50DC50>수리</color>할 수 있습니다";
                break;
            case 11:// 무기상점 위의 준비완료 버튼을 누르면 준비시간을 생략합니다
                text_WellText.text = "무기상점 위의 준비완료 버튼을 누르면 <color=#50DC50>준비시간</color>을 <color=#C82828>생략</color>합니다";
                break;
            case 12:// 튜토리얼이 끝났습니다!
                text_WellText.text = "튜토리얼이 끝났습니다!";
                btn_next.gameObject.SetActive(false);
                break;
        }
    }
    private bool IsLookingAtHandBack()
    {
        if (handBackBone == null || playerCamera == null) return false;

        Vector3 directionToHand =
            (handBackBone.position - playerCamera.position).normalized;

        // 손등이 향하는 방향
        Vector3 handBackDirection = -handBackBone.up;

        return Vector3.Dot(handBackDirection, -directionToHand) > 0.7f;
    }

    public void NextStep()
    {
        waitForNext = true;
        lookObject[CurrentStep].transform.GetChild(0).gameObject.SetActive(true);
        Invoke(nameof(NextStepDelayed), 1f);
        Invoke(nameof(Next), 1f);
    }
    void NextStepDelayed()
    {
        lookObject[CurrentStep].transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Next()
    {
        SetStep(CurrentStep + 1);
    }

    private void SetStep(int step)
    {
        if (step >= lookObject.Length) return;

        CurrentStep = step;

        for (int i = 0; i < lookObject.Length; i++)
        {
            lookObject[i].SetActive(i == CurrentStep);
        }

        waitForNext = false;
    }

    private void LateUpdate()
    {
        if (CurrentStep < 0) return;

        if (playerCamera == null) return;

        for (int i = 0; i < lookObject.Length; i++)
        {
            if (!lookObject[i].activeSelf) continue;

            lookObject[i].transform.LookAt(
                lookObject[i].transform.position + playerCamera.forward,
                playerCamera.up
            );
        }
    }

    private void OnEnable()
    {
        OnNextStep += NextStep;
    }

    private void OnDisable()
    {
        OnNextStep -= NextStep;
    }
}
