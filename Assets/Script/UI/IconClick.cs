using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class IconClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform layout;
    public LayoutElement layoutelement;
    public GameObject Parent;
    public GameObject icon;
    public SelectionUIenum _btnuienum;

    float normalHeight = 60f;
    float hoverHeight = 120f;
    float currentHeight;

    void Awake()
    {
        currentHeight = normalHeight;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.SelectionUI.ChangeNowUIEnum(_btnuienum);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOLocalMoveX(-5f, 0.2f).SetEase(Ease.OutBack);
        icon.transform.DOLocalMoveX(5f, 0.2f).SetEase(Ease.OutBack);
        Parent.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);

        Animate(hoverHeight);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutBack);
        icon.transform.DOLocalMoveX(9f, 0.2f).SetEase(Ease.OutBack);
        Parent.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);

        Animate(normalHeight);
    }

    void Animate(float target)
    {
        DOTween.To(
            () => currentHeight,
            x =>
            {
                currentHeight = x;
                layoutelement.preferredHeight = x;
                LayoutRebuilder.MarkLayoutForRebuild(layout);
            },
            target,
            0.2f
        ).SetEase(Ease.OutBack);
    }
}
