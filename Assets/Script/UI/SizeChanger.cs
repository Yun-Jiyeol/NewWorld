using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;

public class SizeChanger : MonoBehaviour, IPointerDownHandler
{
    public RectTransform target;

    public float minWidth = 100f;
    public float maxWidth = 500f;

    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    void Update()
    {
        if (!isDragging) return;

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            return;
        }

        float deltaX = -1f * Input.GetAxis("Mouse X");

        Vector2 size = GameManager.Instance.UIManager.SelectionUI.SelectPage.sizeDelta;

        size.x += deltaX * 15f; // 감도 조절

        size.x = Mathf.Clamp(size.x, minWidth, maxWidth);

        GameManager.Instance.UIManager.SelectionUI.OnChangeUISize(size);
    }
}
