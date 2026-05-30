using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BtnEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Color _SaveColor;
    public Sprite EnterSprite;
    public Sprite ExitSprite;
    public Sprite ClickSprite;

    public TextMeshProUGUI _text;
    public TextMeshProUGUI _text2;
    public Image _image;
    Image thisimage;

    public bool CanChage = true;

    private void Awake()
    {
        thisimage = GetComponent<Image>();
    }

    public void HoldChange()
    {
        CanChage = false;
        ClickEffect();
    }

    public void UnHoldChange()
    {
        CanChage = true;
        UnChangeEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanChage) return;
        ChangeEffect();
    }

    void ChangeEffect()
    {
        if (_text != null) _text.color = Color.black;
        if (_text2 != null) _text2.color = Color.black;
        if (_image != null) _image.color = Color.black;
        if (thisimage != null) thisimage.sprite = EnterSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanChage) return;
        UnChangeEffect();
    }

    void UnChangeEffect()
    {
        if (_text != null) _text.color = _SaveColor;
        if (_text2 != null) _text2.color = _SaveColor;
        if (_image != null) _image.color = _SaveColor;
        if (thisimage != null) thisimage.sprite = ExitSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanChage) return;
        ClickEffect();
    }

    void ClickEffect()
    {
        if (ClickSprite == null) return;
        if (_text != null) _text.color = Color.black;
        if (_text2 != null) _text2.color = Color.black;
        if (_image != null) _image.color = Color.black;
        if (thisimage != null) thisimage.sprite = ClickSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!CanChage) return;
        ChangeEffect();
    }
}
