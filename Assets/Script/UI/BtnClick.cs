using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public UnityEvent UnityEvent;
    public UnityEvent UnityDownEvent;

    public bool OnOffObject = false;
    public GameObject OnEnter;

    public bool ClickOnOff = false;
    public GameObject ClickObject;

    public bool ChangeColor = false;
    public Color _color;
    Color Originalcolor;
    Image thisImage;

    private void Awake()
    {
        thisImage = gameObject.GetComponent<Image>();
        Originalcolor = thisImage.color;
    }

    private void Start()
    {
        if (OnOffObject)
        {
            OnEnter.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UnityEvent.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UnityDownEvent.Invoke();

        if (ClickOnOff)
        {
            StartCoroutine(OnObject());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ChangeColor)
        {
            thisImage.color = _color;
        }

        if (OnOffObject)
        {
            OnEnter.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ChangeColor)
        {
            thisImage.color = Originalcolor;
        }

        if (OnOffObject)
        {
            OnEnter.SetActive(false);
        }
    }

    IEnumerator OnObject()
    {
        OnEnter.SetActive(false);

        while (true)
        {
            if (!Input.GetMouseButton(0)) break;

            ClickObject.SetActive(true);
            yield return null;
        }

        ClickObject.SetActive(false);
    }
}
