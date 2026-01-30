using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaletteAlfaLine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform childToMove;
    RectTransform parentRect;

    bool isOnImage = false;

    Color color;
    public Material targetMaterial;

    Vector2 savedate = new Vector2(150f, 35f);

    bool isPressed = false;

    float Max = 150f;
    float Min = -150f;

    private void Awake()
    {
        parentRect = transform as RectTransform;
        GameManager.Instance.BuildManager.alfar = (savedate.x + 150f) / 300f;
    }

    public void Setting()
    {
        childToMove.anchoredPosition = savedate;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.InputManager.isTouchingUI) return;

        isOnImage = true;
        GameManager.Instance.InputManager.isTouchingUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isOnImage) return;

        StartCoroutine(MouseOutCoroutine());
    }
    IEnumerator MouseOutCoroutine()
    {
        while (isPressed)
        {
            yield return new WaitForSeconds(0.05f);
        }

        isOnImage = false;
        GameManager.Instance.InputManager.isTouchingUI = false;
    }
    private void Update()
    {
        if (!isOnImage) return;

        if (Input.GetMouseButton(0))
        {
            isPressed = true;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
             parentRect,
             Input.mousePosition,
             null,
             out localPoint
            );

            if(localPoint.x > Max)
            {
                localPoint.x = Max;
            }if (localPoint.x < Min)
            {
                localPoint.x = Min;
            }

            savedate = new Vector2(localPoint.x, 35f);
            childToMove.anchoredPosition = savedate;

            GameManager.Instance.BuildManager.alfar = (savedate.x + 150f) / 300f;
        }
        else if (Input.GetMouseButtonUp(0)) isPressed = false;
    }
}
