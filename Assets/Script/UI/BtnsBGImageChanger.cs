using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnsBGImageChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Sprite BaseImage;
    public Sprite OnMouseImage;
    public Sprite ClickImage;

    Image thisImage;

    private void OnEnable()
    {
        thisImage = gameObject.GetComponent<Image>();
        thisImage.sprite = BaseImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        thisImage.sprite = OnMouseImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        thisImage.sprite = BaseImage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        thisImage.sprite = ClickImage;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        thisImage.sprite = BaseImage;
    }
}
