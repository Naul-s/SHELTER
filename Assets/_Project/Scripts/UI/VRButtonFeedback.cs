using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Shelter.UI
{
    /// <summary>
    /// VR 버튼 반응(개인 표현 — 로컬): 레이·포크로 가리키면 살짝 커지며 뒤쪽 발광이 켜지고 글자가 밝아진다.
    /// 누르면 눌리는 크기 변화 + 딸깍 소리 + 가리킨 손 컨트롤러 진동. 비활성(interactable=false) 버튼은 반응하지 않는다.
    /// 애니메이션은 목표에 도달하면 멈춘다(매 프레임 비용 없음, 할당 없음).
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class VRButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _visual;
        [Tooltip("가리킬 때 켜지는 뒤쪽 발광(선택).")]
        [SerializeField] private Graphic _glow;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _labelNormal = new Color(0.87f, 0.83f, 0.77f, 1f);
        [SerializeField] private Color _labelHover = new Color(1f, 0.96f, 0.9f, 1f);
        [SerializeField, Range(1f, 1.2f)] private float _hoverScale = 1.04f;
        [SerializeField, Range(0.8f, 1f)] private float _pressScale = 0.96f;
        [SerializeField, Range(0f, 1f)] private float _glowAlpha = 0.85f;
        [SerializeField, Min(1f)] private float _speed = 14f;

        [Header("소리·진동")]
        [SerializeField] private AudioClip _hoverClip;
        [SerializeField] private AudioClip _clickClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _hoverHaptic = 0.12f;
        [SerializeField, Range(0f, 1f)] private float _clickHaptic = 0.45f;

        private Selectable _selectable;
        private AudioSource _audio;
        private bool _hovered;
        private bool _pressed;
        private float _scale = 1f;
        private float _glowValue;
        private bool _animating;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            if (_visual == null) _visual = (RectTransform)transform;

            if (_hoverClip != null || _clickClip != null)
            {
                _audio = gameObject.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = 1f;
                _audio.minDistance = 0.5f;
                _audio.maxDistance = 6f;
            }
            ApplyImmediate();
        }

        private void OnDisable()
        {
            _hovered = _pressed = false;
            ApplyImmediate();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_selectable.IsInteractable()) return;
            _hovered = true;
            _animating = true;
            Play(_hoverClip, 0.6f);
            Pulse(eventData, _hoverHaptic, 0.03f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = _pressed = false;
            _animating = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_selectable.IsInteractable()) return;
            _pressed = true;
            _animating = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
            _animating = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_selectable.IsInteractable()) return;
            Play(_clickClip, 1f);
            Pulse(eventData, _clickHaptic, 0.08f);
        }

        private void Update()
        {
            if (!_animating) return;

            float targetScale = _pressed ? _pressScale : _hovered ? _hoverScale : 1f;
            float targetGlow = _hovered ? _glowAlpha : 0f;
            float k = 1f - Mathf.Exp(-_speed * Time.unscaledDeltaTime);
            _scale = Mathf.Lerp(_scale, targetScale, k);
            _glowValue = Mathf.Lerp(_glowValue, targetGlow, k);

            if (Mathf.Abs(_scale - targetScale) < 0.0005f && Mathf.Abs(_glowValue - targetGlow) < 0.005f)
            {
                _scale = targetScale;
                _glowValue = targetGlow;
                _animating = false;
            }
            Apply();
        }

        private void ApplyImmediate()
        {
            _scale = 1f;
            _glowValue = 0f;
            _animating = false;
            Apply();
        }

        private void Apply()
        {
            if (_visual != null) _visual.localScale = new Vector3(_scale, _scale, 1f);
            if (_glow != null)
            {
                Color c = _glow.color;
                c.a = _glowValue;
                _glow.color = c;
            }
            if (_label != null) _label.color = Color.Lerp(_labelNormal, _labelHover, _glowAlpha > 0f ? _glowValue / _glowAlpha : 0f);
        }

        private void Play(AudioClip clip, float volumeScale)
        {
            if (_audio != null && clip != null) _audio.PlayOneShot(clip, _volume * volumeScale);
        }

        /// <summary>이 버튼을 가리킨 손(XR UI 인터랙터)의 컨트롤러에만 진동을 보낸다.</summary>
        private static void Pulse(PointerEventData eventData, float amplitude, float duration)
        {
            if (amplitude <= 0f || !(eventData is TrackedDeviceEventData tracked) || !(tracked.interactor is Component interactor)) return;
            var haptics = interactor.GetComponentInParent<HapticImpulsePlayer>();
            if (haptics != null) haptics.SendHapticImpulse(amplitude, duration);
        }
    }
}
