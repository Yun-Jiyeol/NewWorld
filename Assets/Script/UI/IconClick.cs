using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public SoundData OnMouseSound;

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
        transform.DOKill();
        icon.transform.DOKill();
        Parent.transform.DOKill();

        // 부모 크기 1.1배
        Parent.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.2f)
            .SetEase(Ease.OutBack);

        Animate(hoverHeight);

        if(OnMouseSound != null)
        {
            GameManager.Instance.SoundManager.PlayEffect(OnMouseSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        icon.transform.DOKill();
        Parent.transform.DOKill();

        // 원래 크기
        Parent.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack);

        Animate(normalHeight);
    }

    void Animate(float target)
    {
        DOTween.Kill(this);

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
        )
        .SetEase(Ease.OutBack)
        .SetId(this);
    }
}