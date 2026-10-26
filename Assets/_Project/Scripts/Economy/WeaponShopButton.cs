using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shelter.Economy
{
    /// <summary>
    /// 자판기 화면의 무기 버튼 하나. 누르면 상점에 품목 ID로 구매를 요청한다(가격을 보내지 않는다).
    /// 이름·가격은 카탈로그에서 채우고, 잔액이 모자라면 가격을 붉게·버튼을 흐리게 표시한다.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class WeaponShopButton : MonoBehaviour
    {
        [SerializeField] private WeaponShop _shop;
        [SerializeField] private string _offerId;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Color _affordableColor = new Color(0.95f, 0.85f, 0.4f, 1f);
        [SerializeField] private Color _unaffordableColor = new Color(0.75f, 0.15f, 0.12f, 1f);

        private Button _button;
        private PlayerWallet _wallet;
        private int _price;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            if (_shop == null || !_shop.Catalog.TryGetOffer(_offerId, out WeaponOffer offer))
            {
                Debug.LogError($"{name}: 상점 또는 품목 '{_offerId}'를 찾을 수 없습니다.", this);
                enabled = false;
                return;
            }

            _price = offer.Price;
            if (_nameText != null) _nameText.text = offer.DisplayName;
            if (_priceText != null) _priceText.SetText("{0}", _price);

            _button.onClick.AddListener(HandleClick);
            PlayerWallet.Registered += HandleWalletRegistered;
            Bind(PlayerWallet.Local);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
            PlayerWallet.Registered -= HandleWalletRegistered;
            Bind(null);
        }

        private void HandleWalletRegistered(PlayerWallet wallet)
        {
            if (_wallet == null) Bind(wallet);
        }

        private void Bind(PlayerWallet wallet)
        {
            if (_wallet != null) _wallet.BalanceChanged -= HandleBalanceChanged;
            _wallet = wallet;
            if (_wallet == null) return;

            _wallet.BalanceChanged += HandleBalanceChanged;
            HandleBalanceChanged(_wallet.Balance);
        }

        private void HandleClick()
        {
            _shop.TryPurchase(_offerId, _wallet != null ? _wallet : PlayerWallet.Local);
        }

        private void HandleBalanceChanged(int balance)
        {
            bool affordable = balance >= _price;
            if (_priceText != null) _priceText.color = affordable ? _affordableColor : _unaffordableColor;

            // 누를 수는 있게 두되(거절음·피드백) 흐리게 표시한다.
            ColorBlock colors = _button.colors;
            colors.colorMultiplier = affordable ? 1f : 0.55f;
            _button.colors = colors;
        }
    }
}
