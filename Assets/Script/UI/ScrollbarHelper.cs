using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum vector
{
    X, Y, Z, None
}

public class ScrollbarHelper : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    ChooseHowToMake ChoParent;
    CameraSetting CamParent;
    Scrollbar thisscroll;

    public Image FullImage;
    public Color color;
    public vector vector;

    public void Onit(CameraSetting parent)
    {
        CamParent = parent;
        thisscroll = GetComponent<Scrollbar>();
    }

    public void Onit(ChooseHowToMake parent)
    {
        ChoParent = parent;
        thisscroll = gameObject.GetComponent<Scrollbar>();
    }

    public void OnChange()
    {
        float value = thisscroll.value;

        FullImage.fillAmount = value;

        switch (vector)
        {
            case vector.X:
                ChoParent.Xtext.text = ((int)(value * 128)).ToString();
                break;

            case vector.Y:
                ChoParent.Ytext.text = ((int)(value * 128)).ToString();
                break;

            case vector.Z:
                ChoParent.Ztext.text = ((int)(value * 128)).ToString();
                break;

            case vector.None:
                CamParent.ChangeFieldOfView(value * 180);
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
                ChoParent.ChageXValue();
                break;

            case vector.Y:
                ChoParent.ChageYValue();
                break;

            case vector.Z:
                ChoParent.ChageZValue();
                break;

            case vector.None:
                break;
        }
    }

    public void ChangedValue(int num)
    {
        float percentage = num / 128f;

        thisscroll.value = percentage;
        FullImage.fillAmount= percentage;
    }

    public void ChangedValue(float num)
    {
        float percentage = num / 180f;

        thisscroll.value = percentage;
        FullImage.fillAmount = percentage;
    }
}
