using Shelter.Economy;
using Shelter.Player;
using Shelter.Rounds;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shelter.ShelterDefense
{
    /// <summary>
    /// 바리케이드 수리: 준비 시간 중 판자가 떨어진 입구 안쪽 가까이 가면 입구 옆에 수리 버튼이 나타나고,
    /// 누르면 (떨어진 판자 수 × 장당 비용)을 지갑에서 빼고 판자를 모두 되붙인다.
    /// 판자 수·잔액은 공유 상태 — 상태 변경은 <see cref="TryRepair"/> 한 곳뿐이고, 네트워킹 도입 시 이 메서드가 서버 진입점이 된다
    /// (클라이언트는 "이 바리케이드 수리" 요청만 보내고, 비용은 서버가 BarricadeConfig 로 다시 계산한다).
    /// </summary>
    [RequireComponent(typeof(Barricade))]
    public class BarricadeRepairStation : MonoBehaviour
    {
        // 표시 여부는 매 프레임 볼 필요가 없다(가까이 왔는지 0.2초마다 확인).
        private const float CheckInterval = 0.2f;

        [SerializeField] private RoundManager _roundManager;
        [Tooltip("월드 스페이스 수리 버튼 프리팹(Button 과 비용 표시 TMP 포함). 바리케이드마다 하나씩 만들어 켜고 끈다.")]
        [SerializeField] private RectTransform _promptPrefab;
        [Tooltip("버튼 위치: x = 입구 가장자리에서 옆으로 띄우는 거리, y = 실내 바닥에서의 높이, z = 판자 면에서 실내 쪽으로 띄우는 거리(m).")]
        [SerializeField] private Vector3 _promptPlacement = new Vector3(0.35f, 1.2f, 0.12f);
        [SerializeField] private Color _affordableColor = new Color(0.95f, 0.85f, 0.4f, 1f);
        [SerializeField] private Color _unaffordableColor = new Color(0.8f, 0.15f, 0.12f, 1f);
        [SerializeField] private AudioClip _repairClip;

        private Barricade _barricade;
        private RectTransform _prompt;
        private Button _button;
        private TMP_Text _costText;
        private AudioSource _audio;
        private float _checkTimer;
        private int _shownCost = -1;
        private bool _shownAffordable;

        /// <summary>지금 수리하면 드는 비용(떨어진 판자 수 × 장당 비용).</summary>
        public int RepairCost
        {
            get
            {
                int missing = _barricade.MaxBoards - _barricade.Boards;
                int perBoard = _barricade.Config != null ? _barricade.Config.RepairCostPerBoard : 0;
                return missing * perBoard;
            }
        }

        private void Awake()
        {
            _barricade = GetComponent<Barricade>();

            if (_roundManager == null || _promptPrefab == null)
            {
                Debug.LogError($"{name}: RoundManager 또는 수리 버튼 프리팹이 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _prompt = Instantiate(_promptPrefab);
            _prompt.name = $"{name}_RepairPrompt";
            _button = _prompt.GetComponentInChildren<Button>(true);
            _costText = _prompt.GetComponentInChildren<TMP_Text>(true);
            _audio = _prompt.gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f;
            _button.onClick.AddListener(HandleClick);
            _prompt.gameObject.SetActive(false);
        }

        private void Start()
        {
            PlacePrompt();
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
            if (_prompt != null) Destroy(_prompt.gameObject);
        }

        /// <summary>
        /// 입구 바로 옆 벽면, 가슴 높이에 실내를 향하게 놓는다(판자를 가리지 않게 입구 가장자리 바깥으로).
        /// 바리케이드 루트는 판자와 수십 m 떨어져 있을 수 있어 판자 렌더러 기준으로 계산한다.
        /// </summary>
        private void PlacePrompt()
        {
            Vector3 inward = _barricade.InsidePosition - _barricade.OutsidePosition;
            inward.y = 0f;
            inward = inward.sqrMagnitude > 0.0001f ? inward.normalized : Vector3.forward;
            Vector3 side = Vector3.Cross(Vector3.up, inward).normalized;

            // 판자들의 중심과 입구 폭(측면 방향 반폭)
            Vector3 center = Vector3.zero;
            float halfWidth = 0f;
            int found = 0;
            var boards = _barricade.BoardTransforms;
            for (int i = 0; i < boards.Count; i++)
            {
                Renderer r = boards[i] != null ? boards[i].GetComponentInChildren<Renderer>() : null;
                if (r == null) continue;
                center += r.bounds.center;
                found++;
            }
            center = found > 0 ? center / found : _barricade.InsidePosition;
            for (int i = 0; i < boards.Count; i++)
            {
                Renderer r = boards[i] != null ? boards[i].GetComponentInChildren<Renderer>() : null;
                if (r == null) continue;
                Bounds b = r.bounds;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1));
                    halfWidth = Mathf.Max(halfWidth, Mathf.Abs(Vector3.Dot(corner - center, side)));
                }
            }

            Vector3 position = center + side * (halfWidth + _promptPlacement.x) + inward * _promptPlacement.z;
            position.y = _barricade.InsidePosition.y + _promptPlacement.y;
            _prompt.position = position;
            // 캔버스 앞면이 실내(플레이어 쪽)를 보도록: forward = 바깥 방향
            _prompt.rotation = Quaternion.LookRotation(-inward, Vector3.up);
        }

        private void Update()
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer > 0f) return;
            _checkTimer = CheckInterval;

            bool show = ShouldShow();
            if (_prompt.gameObject.activeSelf != show) _prompt.gameObject.SetActive(show);
            if (show) RefreshCost();
        }

        private bool ShouldShow()
        {
            if (_roundManager.State != RoundState.Intermission) return false;
            if (_barricade.Boards >= _barricade.MaxBoards) return false;

            float range = _barricade.Config != null ? _barricade.Config.RepairPromptDistance : 2.5f;
            Vector3 anchor = _barricade.InsidePosition;
            var players = PlayerHealth.ActiveInstances;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerHealth player = players[i];
                if (player == null || player.IsDead) continue;

                Vector3 d = player.TargetPoint.position - anchor;
                d.y = 0f;
                if (d.sqrMagnitude <= range * range) return true;
            }
            return false;
        }

        private void RefreshCost()
        {
            int cost = RepairCost;
            PlayerWallet wallet = PlayerWallet.Local;
            bool affordable = wallet != null && wallet.Balance >= cost;
            if (cost == _shownCost && affordable == _shownAffordable) return;

            _shownCost = cost;
            _shownAffordable = affordable;
            _costText.SetText("수리  {0}", cost);
            _costText.color = affordable ? _affordableColor : _unaffordableColor;
        }

        private void HandleClick()
        {
            TryRepair(PlayerWallet.Local);
        }

        /// <summary>buyer 의 돈으로 이 바리케이드를 전부 수리한다. 준비 시간이 아니거나 돈이 모자라면 false.</summary>
        public bool TryRepair(PlayerWallet buyer)
        {
            if (buyer == null || _roundManager.State != RoundState.Intermission) return false;

            int cost = RepairCost;
            if (cost <= 0 || !buyer.TrySpend(cost))
            {
                _shownCost = -1; // 비용 색 다시 칠하기
                return false;
            }

            _barricade.RepairAll();
            if (_repairClip != null) _audio.PlayOneShot(_repairClip);
            _prompt.gameObject.SetActive(false);
            return true;
        }
    }
}
