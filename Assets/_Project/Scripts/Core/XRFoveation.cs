using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Shelter.Core
{
    /// <summary>
    /// Quest 고정 포비에이티드 렌더링을 켠다 — 시야 가장자리를 낮은 해상도로 그려 GPU 픽셀 부하를 줄인다(시선 추적 기기는 시선 기반).
    /// OpenXR 설정의 FoveatedRenderingFeature + SRP Foveation API 가 켜져 있어야 동작한다. 씬 배치 없이 앱 시작 시 자동 적용.
    /// XR 디스플레이가 늦게 올라올 수 있어, 실행 중인 디스플레이를 찾을 때까지 매 렌더 직전에 한 번씩 확인한다.
    /// </summary>
    public static class XRFoveation
    {
        /// <summary>0 = 끔, 1 = 최대. 가장자리 흐림이 거슬리면 낮춘다(실기기에서 조정).</summary>
        private const float Level = 1f;

        private static readonly List<XRDisplaySubsystem> Displays = new List<XRDisplaySubsystem>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            Application.onBeforeRender -= TryApply;
            Application.onBeforeRender += TryApply;
        }

        private static void TryApply()
        {
            SubsystemManager.GetSubsystems(Displays);
            for (int i = 0; i < Displays.Count; i++)
            {
                XRDisplaySubsystem display = Displays[i];
                if (!display.running) continue;

                display.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
                display.foveatedRenderingLevel = Level;
                Application.onBeforeRender -= TryApply;
                return;
            }
        }
    }
}
