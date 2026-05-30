using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BtnColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image ChangedImage;
    public Color ChangingColor;
    Color OriginalColor = Color.white;

    private void Awake()
    {
        if (ChangedImage == null)
        {
            enabled = false;
            return;
        }
        OriginalColor = ChangedImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ChangedImage.DOKill();
        ChangedImage.DOColor(ChangingColor, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangedImage.DOKill();
        ChangedImage.DOColor(OriginalColor, 0.5f);
    }
}
