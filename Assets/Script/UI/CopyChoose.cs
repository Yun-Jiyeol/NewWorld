using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CopyChoose : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform childToMove;
    RectTransform parentRect;

    bool isOnImage = false;

    Color color;

    float Max = 150f;
    float Min = -150f;

    bool isPressed = false;
    LightUI lightui;

    private void Awake()
    {
        parentRect = transform as RectTransform;
    }

    public void Setting(LightUI _lightUI)
    {
        lightui = _lightUI;

        Color.RGBToHSV(lightui._color, out float h, out float s, out float v);

        float svalue = Mathf.Lerp(-150f, 150f, s);
        float vvalue = Mathf.Lerp(-150f, 150f, v);

        childToMove.anchoredPosition = new Vector2(svalue, vvalue);

        DrawPaletteLine();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.InputManager.isTouchingUI) return;

        isOnImage = true;
        GameManager.Instance.InputManager.isTouchingUI = true;
        childToMove.DOSizeDelta(new Vector2(35f, 35f), 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOnImage) return;
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
        childToMove.DOSizeDelta(new Vector2(0f, 0f), 0.5f);
    }

    public void DrawPaletteLine()
    {
        Color baseColor = lightui.inLineChoose; // 선택된 기준 색

        Image targetImage = GetComponent<Image>();
        RectTransform rt = targetImage.rectTransform;

        int width = Mathf.RoundToInt(rt.rect.width);
        int height = Mathf.RoundToInt(rt.rect.height);

        if (width <= 0 || height <= 0)
            return;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float xT = x / (float)(width - 1); // 0~1 (좌→우)

            // 흰색 → 선택 색
            Color horizontalColor = Color.Lerp(Color.white, baseColor, xT);

            for (int y = 0; y < height; y++)
            {
                // 위가 0, 아래가 1이 되도록 뒤집기
                float yT = 1f - (y / (float)(height - 1));

                // 위: 그대로 / 아래: 검정 섞기
                Color finalColor = Color.Lerp(horizontalColor, Color.black, yT);

                tex.SetPixel(x, y, finalColor);
            }
        }

        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        targetImage.sprite = sprite;
        lightui._color = GetColorFromPalette(childToMove.anchoredPosition);
        childToMove.gameObject.GetComponent<Image>().color = lightui._color;
        lightui.ChangeColor();
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


            if (localPoint.x > Max)
            {
                localPoint.x = Max;
            }
            if (localPoint.x < Min)
            {
                localPoint.x = Min;
            }
            if (localPoint.y > Max)
            {
                localPoint.y = Max;
            }
            if (localPoint.y < Min)
            {
                localPoint.y = Min;
            }

            childToMove.anchoredPosition = localPoint;

            lightui._color = GetColorFromPalette(childToMove.anchoredPosition);
            childToMove.gameObject.GetComponent<Image>().color = lightui._color;
            lightui.ChangeColor();
        }
        else if (Input.GetMouseButtonUp(0)) isPressed = false;
    }

    Color GetColorFromPalette(Vector2 localPoint)
    {
        Rect rect = parentRect.rect;
        Texture2D tex = GetComponent<Image>().sprite.texture;

        // X : 좌 → 우
        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        u = Mathf.Clamp01(u);

        // Y : 위 → 아래 (검정이 아래니까 반전)
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        v = Mathf.Clamp01(v);

        int texX = Mathf.RoundToInt(u * (tex.width - 1));
        int texY = Mathf.RoundToInt(v * (tex.height - 1));

        return tex.GetPixel(texX, texY);
    }
}
