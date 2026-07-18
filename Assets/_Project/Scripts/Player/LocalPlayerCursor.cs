using UnityEngine;

namespace Shelter.Player
{
    /// <summary>
    /// 로컬 PC 플레이어 전용 커서 잠금 관리. Cursor는 클라이언트 전역 상태이므로
    /// 리그 컴포넌트의 활성/비활성(사망·리스폰 등)과 분리해 이 컴포넌트가 단독으로 소유한다.
    /// 메뉴/사망 UI 등 커서가 필요한 시스템은 SetLocked(false)를 호출할 것.
    /// 멀티플레이어 도입 시 로컬 소유 리그에만 부착/활성화되어야 한다 (원격 프록시 금지).
    /// </summary>
    public class LocalPlayerCursor : MonoBehaviour
    {
        private bool _locked;

        private void Start()
        {
            SetLocked(true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // 알트탭 복귀 시 마지막 의도 상태 복원 (메뉴가 풀어둔 커서를 다시 잠그지 않도록)
            if (hasFocus)
                Apply();
        }

        public void SetLocked(bool locked)
        {
            _locked = locked;
            Apply();
        }

        private void Apply()
        {
            Cursor.lockState = _locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_locked;
        }
    }
}
