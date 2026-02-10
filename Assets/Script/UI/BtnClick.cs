using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent UnityEvent;
    public bool ChangeColor = false;
    public Color _color;
    Color Originalcolor;
    Image thisImage;

    private void Awake()
    {
        thisImage = gameObject.GetComponent<Image>();
        Originalcolor = thisImage.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UnityEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ChangeColor) return;
        thisImage.color = _color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!ChangeColor) return;
        thisImage.color = Originalcolor;
    }
}
