using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingPageBtnMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform thisRect;

    private void Awake()
    {
        thisRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        thisRect.DOKill();

        Vector2 size = thisRect.sizeDelta;
        size.y = 80;
        thisRect.sizeDelta = size;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        thisRect.DOKill();

        thisRect.DOSizeDelta(new Vector2(75f, 90f), 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        thisRect.DOKill();

        thisRect.DOSizeDelta(new Vector2(75f, 80f), 0.2f);
    }
}
