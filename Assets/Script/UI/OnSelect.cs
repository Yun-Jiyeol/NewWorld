using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OnSelect : MonoBehaviour
{
    public Color color;

    public void OnClick()
    {
        gameObject.GetComponent<Image>().DOColor(color, 0.5f);
    }

    public void DisClick()
    {
        gameObject.GetComponent<Image>().DOColor(Color.white, 0.5f);
    }
}
