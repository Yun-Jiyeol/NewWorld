using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IconClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject icon;
    public SelectionUIenum _btnuienum;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.SelectionUI.ChangeNowUIEnum(_btnuienum);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        icon.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.Linear);
        icon.transform.DOScale(0.75f, 0.2f).SetEase(Ease.OutBack);
    }
}
