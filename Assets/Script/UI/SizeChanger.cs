using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SizeChanger : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;

    public float minWidth = 150f;
    public float maxWidth = 500f;

    private bool isDragging = false;

    Image thisImage;
    Color OriginalColor = new Color(1, 1, 1, 0.6f);
    Color ChangeColor = new Color(0.3f, 0.3f, 1, 0.6f);

    private void OnEnable()
    {
        thisImage = gameObject.GetComponent<Image>();

        thisImage.color = OriginalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        thisImage.color = ChangeColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        thisImage.color = OriginalColor;
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

        size.x += deltaX * 30f; // 감도 조절

        size.x = Mathf.Clamp(size.x, minWidth, maxWidth);

        GameManager.Instance.UIManager.SelectionUI.OnChangeUISize(size);
    }
}
