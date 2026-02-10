using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public VerticalLayoutGroup layout;
    public GameObject Parent;
    public GameObject icon;
    public SelectionUIenum _btnuienum;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.SelectionUI.ChangeNowUIEnum(_btnuienum);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOLocalMoveX(-5f, 0.2f).SetEase(Ease.OutBack);
        icon.transform.DOLocalMoveX(5f, 0.2f).SetEase(Ease.OutBack);
        Parent.transform.DOScale(1.2f,0.2f).SetEase(Ease.OutBack).OnUpdate(() =>
        {
            layout.SetLayoutVertical();
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutBack);
        icon.transform.DOLocalMoveX(9f, 0.2f).SetEase(Ease.OutBack);
        Parent.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).OnUpdate(() =>
        {
            layout.SetLayoutVertical();
        });
    }
}
