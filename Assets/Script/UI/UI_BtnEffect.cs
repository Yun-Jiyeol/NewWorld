using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MouseOnSound 
{
    None,
    DigitalSound,
    ResetSound,
    NameSound,
    IconSound,
}

public enum ClickSound
{
    None,
    DigitalSound,
    NameSound,
    IconSound,
}


public class UI_BtnEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Color _SaveColor;
    public Color _ChangingColor = Color.black;
    public Sprite EnterSprite;
    public Sprite ExitSprite;
    public Sprite ClickSprite;

    public TextMeshProUGUI _text;
    public TextMeshProUGUI _text2;
    public Image _image;
    public Image _image2;
    Image thisimage;

    public bool CanChage = true;

    [Header("Sound")]
    public MouseOnSound mouseOnSound = MouseOnSound.None;
    public ClickSound clickSound = ClickSound.None;

    private void Awake()
    {
        thisimage = GetComponent<Image>();
    }

    public void HoldEnter()
    {
        CanChage = false;
        ChangeEffect();
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
        if (mouseOnSound != MouseOnSound.None)
        {
            GameManager.Instance.SoundManager.PlayEffect(SoundChoose.CallAudio(mouseOnSound));
        } 
    }

    void ChangeEffect()
    {
        if (_text != null) _text.color = _ChangingColor;
        if (_text2 != null) _text2.color = _ChangingColor;
        if (_image != null) _image.color = _ChangingColor;
        if (_image2 != null) _image2.color = _ChangingColor;
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
        if (_image2 != null) _image2.color = _SaveColor;
        if (thisimage != null) thisimage.sprite = ExitSprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanChage) return;

        ClickEffect();
        if (clickSound != ClickSound.None)
        {
            GameManager.Instance.SoundManager.PlayEffect(SoundChoose.CallAudio(clickSound));
        }
    }

    void ClickEffect()
    {
        if (ClickSprite == null) return;
        if (_text != null) _text.color = _ChangingColor;
        if (_text2 != null) _text2.color = _ChangingColor;
        if (_image != null) _image.color = _ChangingColor;
        if (_image2 != null) _image2.color = _ChangingColor;
        if (thisimage != null) thisimage.sprite = ClickSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!CanChage) return;
        ChangeEffect();
    }
}
