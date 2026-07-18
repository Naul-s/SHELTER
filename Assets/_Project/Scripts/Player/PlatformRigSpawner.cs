using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Shelter.Player
{
    /// <summary>
    /// 런타임에 XR 디스플레이 활성 여부를 감지해 VR 또는 PC 플레이어 리그를 스폰한다 (cross-platform-input 스킬).
    /// 하나의 빌드가 두 플랫폼을 모두 처리하는 구조의 진입점.
    /// 멀티플레이어 도입 시 이 스폰 로직은 네트워크 스포너(Fusion)로 대체/이관된다.
    /// </summary>
    public class PlatformRigSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _vrRigPrefab;
        [SerializeField] private GameObject _pcRigPrefab;

        private void Start()
        {
            GameObject prefab = IsXRRunning() ? _vrRigPrefab : _pcRigPrefab;
            GameObject rig = Instantiate(prefab, transform.position, transform.rotation);
            rig.name = prefab.name;
        }

        private static bool IsXRRunning()
        {
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            foreach (var display in displays)
            {
                if (display.running)
                    return true;
            }
            return false;
        }
    }
}
