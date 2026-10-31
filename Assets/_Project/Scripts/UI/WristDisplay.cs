using Shelter.Core;
using Shelter.Economy;
using Shelter.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shelter.UI
{
    /// <summary>
    /// 왼손목 시계형 다이제틱 표시: 체력 바(+맞은 만큼 늦게 줄어드는 잔상 바)·체력 수치·보유 금액.
    /// 손목 바깥쪽(손등 쪽)에 붙어 시계 보듯 손목을 눈앞으로 돌리면 선명해지고, 평소엔 흐려져 시야를 가리지 않는다.
    /// 화면 고정 UI 를 쓰지 않는다(ui-audio). 값은 읽기만 한다 — PlayerHealth/PlayerWallet 이벤트를 구독한다.
    /// UI 는 실행 시 코드로 만든다(손목 앵커만 연결하면 되도록). 위치·각도는 실기기에서 다듬을 값이다.
    /// </summary>
    public class WristDisplay : MonoBehaviour
    {
        private static readonly Vector2 CanvasSize = new Vector2(260f, 120f);

        [Tooltip("붙일 손(왼손 컨트롤러).")]
        [SerializeField] private Transform _anchor;
        [SerializeField] private TMP_FontAsset _font;
        [Tooltip("손 기준 위치(m). 기본은 손목 바깥쪽(-x)·손 뒤쪽(-z) — 장갑·소매 표면 바로 위.")]
        [SerializeField] private Vector3 _localPosition = new Vector3(-0.06f, 0f, -0.1f);
        [Tooltip("손 기준 회전. 기본은 앞면이 손등 쪽(-x)을 보고, 글자가 팔 방향(+z)으로 놓인다.")]
        [SerializeField] private Vector3 _localEuler = new Vector3(0f, 90f, 180f);
        [Tooltip("캔버스 1px 당 m. 260x120px → 약 10x5cm.")]
        [SerializeField, Min(0.0001f)] private float _metersPerPixel = 0.0004f;

        [Header("보이기")]
        [Tooltip("손목을 보지 않을 때의 불투명도.")]
        [SerializeField, Range(0f, 1f)] private float _idleAlpha = 0.15f;
        [Tooltip("표시면과 시선이 이루는 정도(코사인)가 이 범위를 지나며 선명해진다.")]
        [SerializeField] private Vector2 _lookFadeRange = new Vector2(0.35f, 0.75f);

        [Header("색")]
        [SerializeField] private Color _panelColor = new Color(0.05f, 0.04f, 0.04f, 0.75f);
        [SerializeField] private Color _healthyColor = new Color(0.45f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color _woundedColor = new Color(0.95f, 0.7f, 0.15f, 1f);
        [SerializeField] private Color _criticalColor = new Color(0.9f, 0.1f, 0.06f, 1f);
        [SerializeField] private Color _trailColor = new Color(1f, 0.92f, 0.85f, 0.85f);
        [SerializeField] private Color _textColor = new Color(0.95f, 0.92f, 0.85f, 1f);
        [SerializeField] private Color _moneyColor = new Color(0.95f, 0.85f, 0.4f, 1f);

        [Header("움직임")]
        [Tooltip("잔상 바가 줄어들기 시작하기까지 기다리는 시간(초).")]
        [SerializeField, Min(0f)] private float _trailDelay = 0.45f;
        [SerializeField, Min(0.01f)] private float _trailSpeed = 0.6f;

        private PlayerHealth _health;
        private PlayerWallet _wallet;
        private Transform _head;
        private Transform _root;
        private CanvasGroup _group;
        private RectTransform _fill;
        private RectTransform _trail;
        private Image _fillImage;
        private TMP_Text _healthText;
        private TMP_Text _moneyText;

        private float _shown01 = 1f;
        private float _trail01 = 1f;
        private float _trailWait;

        private void Awake()
        {
            _health = GetComponentInParent<PlayerHealth>();
            _wallet = GetComponentInParent<PlayerWallet>();
            Camera cam = GetComponentInChildren<Camera>(true);
            if (cam == null && _health != null) cam = _health.GetComponentInChildren<Camera>(true);

            if (_anchor == null || _health == null || cam == null)
            {
                Debug.LogError($"{name}: 손목 앵커·PlayerHealth·카메라 중 연결되지 않은 것이 있습니다.", this);
                enabled = false;
                return;
            }

            _head = cam.transform;
            Build();
        }

        private void OnEnable()
        {
            if (_root == null) return;
            _root.gameObject.SetActive(true);
            _health.Damaged += HandleDamaged;
            _health.HealthChanged += Refresh;
            if (_wallet != null) _wallet.BalanceChanged += ShowMoney;
        }

        private void Start()
        {
            _shown01 = _trail01 = _health.Health01;
            Refresh();
            if (_wallet != null) ShowMoney(_wallet.Balance);
        }

        private void OnDisable()
        {
            if (_root == null) return;
            _root.gameObject.SetActive(false);
            _health.Damaged -= HandleDamaged;
            _health.HealthChanged -= Refresh;
            if (_wallet != null) _wallet.BalanceChanged -= ShowMoney;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            _trailWait = _trailDelay;
        }

        private void Refresh()
        {
            _healthText.SetText("{0}", Mathf.CeilToInt(_health.CurrentHealth));
            if (_health.Health01 > _trail01) _trail01 = _health.Health01; // 회복은 잔상 없이 바로
        }

        private void ShowMoney(int balance)
        {
            _moneyText.SetText("보유 {0}", balance);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float target = _health.Health01;

            _shown01 = Mathf.MoveTowards(_shown01, target, dt * 2.5f);
            if (_trailWait > 0f) _trailWait -= dt;
            else _trail01 = Mathf.MoveTowards(_trail01, _shown01, dt * _trailSpeed);

            SetWidth(_fill, _shown01);
            SetWidth(_trail, _trail01);

            Color color = _shown01 > 0.6f
                ? Color.Lerp(_woundedColor, _healthyColor, (_shown01 - 0.6f) / 0.4f)
                : Color.Lerp(_criticalColor, _woundedColor, Mathf.Clamp01((_shown01 - 0.25f) / 0.35f));
            if (_shown01 <= 0.25f && target > 0f) color *= 0.75f + 0.25f * Mathf.Sin(Time.time * 9f);
            color.a = 1f;
            _fillImage.color = color;

            // 시계 보듯 손목을 돌려 표시면이 눈을 향할수록 선명하게
            Vector3 toFace = (_root.position - _head.position).normalized;
            float facing = Vector3.Dot(_root.forward, toFace);
            float look = Mathf.InverseLerp(_lookFadeRange.x, _lookFadeRange.y, facing);
            _group.alpha = Mathf.Lerp(_idleAlpha, 1f, look);
        }

        private static void SetWidth(RectTransform bar, float value01)
        {
            Vector2 max = bar.anchorMax;
            max.x = Mathf.Clamp01(value01);
            bar.anchorMax = max;
        }

        private void Build()
        {
            var canvasGo = new GameObject("WristDisplay", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            _root = canvasGo.transform;
            _root.SetParent(_anchor, false);
            _root.localPosition = _localPosition;
            _root.localRotation = Quaternion.Euler(_localEuler);
            _root.localScale = Vector3.one * _metersPerPixel;

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            ((RectTransform)_root).sizeDelta = CanvasSize;
            _group = canvasGo.GetComponent<CanvasGroup>();
            _group.interactable = false;
            _group.blocksRaycasts = false;

            Image panel = AddImage(_root, "Panel", _panelColor);
            Stretch(panel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // 체력 바: 배경 → 잔상 → 채움
            Image barBack = AddImage(_root, "HealthBar", new Color(0f, 0f, 0f, 0.6f));
            Stretch(barBack.rectTransform, new Vector2(0f, 0.56f), new Vector2(1f, 0.92f), new Vector2(14f, 0f), new Vector2(-14f, 0f));
            Image trail = AddImage(barBack.rectTransform, "Trail", _trailColor);
            _trail = trail.rectTransform;
            Stretch(_trail, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _fillImage = AddImage(barBack.rectTransform, "Fill", _healthyColor);
            _fill = _fillImage.rectTransform;
            Stretch(_fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _healthText = AddText(barBack.rectTransform, "HealthText", 30f, TextAlignmentOptions.Center, _textColor);
            Stretch(_healthText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _healthText.outlineWidth = 0.12f;
            _healthText.outlineColor = new Color32(0, 0, 0, 200);

            _moneyText = AddText(_root, "MoneyText", 34f, TextAlignmentOptions.Center, _moneyColor);
            Stretch(_moneyText.rectTransform, new Vector2(0f, 0.06f), new Vector2(1f, 0.5f), new Vector2(14f, 0f), new Vector2(-14f, 0f));
            if (_wallet == null) _moneyText.gameObject.SetActive(false);
        }

        private static Image AddImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private TMP_Text AddText(Transform parent, string name, float size, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (_font != null) text.font = _font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
