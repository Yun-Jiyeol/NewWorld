using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum vector
{
    X, Y, Z
}

public class ScrollbarHelper : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    ChooseHowToMake Parent;
    Scrollbar thisscroll;

    public Image FullImage;
    public Color color;
    public vector vector;

    public void Onit(ChooseHowToMake parent)
    {
        Parent = parent;
        thisscroll = gameObject.GetComponent<Scrollbar>();
    }

    public void OnChange()
    {
        float value = thisscroll.value;

        FullImage.fillAmount = value;

        switch (vector)
        {
            case vector.X:
                Parent.Xtext.text = ((int)(value * 128)).ToString();
                break;

            case vector.Y:
                Parent.Ytext.text = ((int)(value * 128)).ToString();
                break;

            case vector.Z:
                Parent.Ztext.text = ((int)(value * 128)).ToString();
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        thisscroll.handleRect.GetComponent<Image>().DOColor(color,0.5f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        thisscroll.handleRect.GetComponent<Image>().DOColor(Color.white, 0.5f);

        switch (vector)
        {
            case vector.X:
                Parent.ChageXValue();
                break;

            case vector.Y:
                Parent.ChageYValue();
                break;

            case vector.Z:
                Parent.ChageZValue();
                break;
        }
    }

    public void ChangedValue(int num)
    {
        float percentage = num / 128f;

        thisscroll.value = percentage;
        FullImage.fillAmount= percentage;
    }
}
