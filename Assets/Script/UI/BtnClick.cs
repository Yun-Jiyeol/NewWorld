using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BtnClick : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent UnityEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        UnityEvent.Invoke();
    }
}
