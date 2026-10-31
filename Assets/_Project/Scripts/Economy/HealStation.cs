using Shelter.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shelter.Economy
{
    /// <summary>
    /// 구급상자(회복 스테이션): 상점 옆 벽의 버튼. 누르면 회복 비용을 내고 체력을 회복한다.
    /// 체력이 가득 차 있거나 돈이 모자라면 거절한다. 비용·회복량은 EconomyConfig(기획 미확정 — 임시값).
    /// 체력·잔액은 공유 상태 — 상태 변경은 <see cref="TryHeal"/> 한 곳뿐이고, 네트워킹 도입 시 이 메서드가 서버 진입점이 된다
    /// (클라이언트는 "회복" 요청만 보내고, 서버가 거리·체력·잔액을 검증한 뒤 비용을 EconomyConfig 로 다시 계산한다).
    /// </summary>
    public class HealStation : MonoBehaviour
    {
        // 표시는 매 프레임 볼 필요가 없다.
        private const float CheckInterval = 0.2f;

        [SerializeField] private EconomyConfig _config;
        [Tooltip("월드 스페이스 버튼 프리팹(Button 과 문구 TMP 포함). 이 오브젝트의 위치·방향(앞면 = -forward 쪽)에 놓인다.")]
        [SerializeField] private RectTransform _buttonPrefab;
        [Tooltip("이 거리(m, 수평) 안에 있는 플레이어만 사용할 수 있다.")]
        [SerializeField, Min(0.5f)] private float _useRange = 3f;
        [SerializeField] private Color _affordableColor = new Color(0.55f, 0.95f, 0.55f, 1f);
        [SerializeField] private Color _unaffordableColor = new Color(0.8f, 0.15f, 0.12f, 1f);
        [SerializeField] private Color _fullColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private AudioClip _deniedClip;

        private RectTransform _button;
        private Button _buttonControl;
        private TMP_Text _label;
        private AudioSource _audio;
        private float _checkTimer;
        private int _shownState = -1;

        private void Awake()
        {
            if (_config == null || _buttonPrefab == null)
            {
                Debug.LogError($"{name}: EconomyConfig 또는 버튼 프리팹이 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _button = Instantiate(_buttonPrefab, transform, false);
            _button.name = "HealButton";
            _buttonControl = _button.GetComponentInChildren<Button>(true);
            _label = _button.GetComponentInChildren<TMP_Text>(true);
            _audio = _button.gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f;
            _buttonControl.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_buttonControl != null) _buttonControl.onClick.RemoveListener(HandleClick);
        }

        private void Update()
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer > 0f) return;
            _checkTimer = CheckInterval;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            PlayerWallet wallet = PlayerWallet.Local;
            PlayerHealth health = wallet != null ? wallet.GetComponent<PlayerHealth>() : null;

            // 0 = 사용 가능, 1 = 돈 부족, 2 = 체력 가득(또는 사망)
            int state = health == null || !health.IsDamaged ? 2 : wallet.Balance >= _config.HealCost ? 0 : 1;
            if (state == _shownState) return;
            _shownState = state;

            if (state == 2)
            {
                _label.SetText("체력 충분");
                _label.color = _fullColor;
                return;
            }

            _label.SetText("회복  {0}", _config.HealCost);
            _label.color = state == 0 ? _affordableColor : _unaffordableColor;
        }

        private void HandleClick()
        {
            if (!TryHeal(PlayerWallet.Local) && _deniedClip != null) _audio.PlayOneShot(_deniedClip);
        }

        /// <summary>buyer 의 돈으로 buyer 자신을 회복한다. 멀리 있거나, 체력이 가득하거나, 돈이 모자라면 false.</summary>
        public bool TryHeal(PlayerWallet buyer)
        {
            if (buyer == null) return false;

            PlayerHealth health = buyer.GetComponent<PlayerHealth>();
            if (health == null || !health.IsDamaged) return false;

            Vector3 offset = health.TargetPoint.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > _useRange * _useRange) return false;

            if (!buyer.TrySpend(_config.HealCost))
            {
                _shownState = -1;
                return false;
            }

            health.Heal(health.MaxHealth * _config.HealFraction);
            _shownState = -1;
            return true;
        }
    }
}
