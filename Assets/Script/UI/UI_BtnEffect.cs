using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BtnEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color _SaveColor;
    public Sprite EnterSprite;
    public Sprite ExitSprite;

    public TextMeshProUGUI _text;
    public Image _image;
    Image thisimage;

    private void Awake()
    {
        thisimage = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_text != null) _text.color = Color.black;
        if (_image != null) _image.color = Color.black;
        if (thisimage != null) thisimage.sprite = EnterSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_text != null) _text.color = _SaveColor;
        if (_image != null) _image.color = _SaveColor;
        if (thisimage != null) thisimage.sprite = ExitSprite;
    }
}
