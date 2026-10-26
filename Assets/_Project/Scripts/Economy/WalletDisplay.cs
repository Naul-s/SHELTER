using TMPro;
using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>로컬 플레이어의 보유 금액 표시(자판기 화면 등 월드 스페이스). 잔액 변경 이벤트에서만 갱신한다.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class WalletDisplay : MonoBehaviour
    {
        [Tooltip("{0} 자리에 잔액이 들어간다.")]
        [SerializeField] private string _format = "보유 금액  {0}";

        private TMP_Text _text;
        private PlayerWallet _wallet;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            PlayerWallet.Registered += HandleWalletRegistered;
            Bind(PlayerWallet.Local);
        }

        private void OnDestroy()
        {
            PlayerWallet.Registered -= HandleWalletRegistered;
            Bind(null);
        }

        private void HandleWalletRegistered(PlayerWallet wallet)
        {
            if (_wallet == null) Bind(wallet);
        }

        private void Bind(PlayerWallet wallet)
        {
            if (_wallet != null) _wallet.BalanceChanged -= Show;
            _wallet = wallet;
            if (_wallet == null) { _text.text = string.Empty; return; }

            _wallet.BalanceChanged += Show;
            Show(_wallet.Balance);
        }

        private void Show(int balance)
        {
            _text.SetText(_format, balance);
        }
    }
}
