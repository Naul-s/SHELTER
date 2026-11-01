using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("시각적 효과를 줄 자식 오브젝트")]
    [SerializeField] private Transform visualTransform;

    private void Awake()
    {
        // 자식 오브젝트를 따로 지정하지 않았다면 자기 자신을 참조
        if (visualTransform == null)
            visualTransform = transform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        visualTransform.localScale = new Vector3(0.95f, 0.95f, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        visualTransform.localScale = new Vector3(1, 1, 1);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        visualTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
    }
}