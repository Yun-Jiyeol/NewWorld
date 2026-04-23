using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CopyLine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform childToMove;
    RectTransform parentRect;

    Image targetImage;
    bool isOnImage = false;

    public Color[] colors = { };

    bool isPressed = false;

    float Max = 145f;
    float Min = -145f;

    LightUI lightUI = null;
    MainLightUI MainLightUI = null;

    private void Awake()
    {
        parentRect = transform as RectTransform;
    }

    public void Setting(LightUI _lightui)
    {
        DrawPaletteLine();

        Color.RGBToHSV(_lightui._color, out float h, out float s, out float v);
        float value = Mathf.Lerp(-145f, 145f, h);

        childToMove.anchoredPosition = new Vector2(value, -25f);
        _lightui.ChangeLineChoose(GetColorFromPalette(new Vector2(value, 0)));
        childToMove.gameObject.GetComponent<Image>().color = GetColorFromPalette(new Vector2(value, 0));
        lightUI = _lightui;
    }

    public void Setting(MainLightUI _mainLightUI)
    {
        DrawPaletteLine();

        Color.RGBToHSV(_mainLightUI._color, out float h, out float s, out float v);
        float value = Mathf.Lerp(-145f, 145f, h);

        childToMove.anchoredPosition = new Vector2(value, -25f);
        _mainLightUI.ChangeInlineColor(GetColorFromPalette(new Vector2(value, 0)));
        childToMove.gameObject.GetComponent<Image>().color = GetColorFromPalette(new Vector2(value, 0));
        MainLightUI = _mainLightUI;
    }

    void DrawPaletteLine()
    {
        if (colors == null || colors.Length < 2)
            return;

        targetImage = gameObject.GetComponent<Image>();
        RectTransform rt = targetImage.rectTransform;

        int width = Mathf.RoundToInt(rt.rect.width);
        int height = Mathf.RoundToInt(rt.rect.height);

        if (width <= 0 || height <= 0)
            return;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        int colorCount = colors.Length;

        for (int x = 0; x < width; x++)
        {
            // 0 ~ 1 위치
            float t = x / (float)(width - 1);

            // 전체 색 범위에서 위치 계산
            float scaled = t * (colorCount - 1);

            int indexA = Mathf.FloorToInt(scaled);
            int indexB = Mathf.Clamp(indexA + 1, 0, colorCount - 1);

            float lerpT = scaled - indexA;

            Color c = Color.Lerp(colors[indexA], colors[indexB], lerpT);

            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        targetImage.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.InputManager.isTouchingUI) return;

        isOnImage = true;
        GameManager.Instance.InputManager.isTouchingUI = true;
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

            childToMove.anchoredPosition = new Vector2(localPoint.x, -25f);

            if(lightUI != null)
            {
                lightUI.ChangeLineChoose(GetColorFromPalette(new Vector2(localPoint.x, 0)));
                childToMove.gameObject.GetComponent<Image>().color = lightUI.inLineChoose;
                lightUI.CopyChoose.DrawPaletteLine();
            }else if(MainLightUI != null)
            {
                MainLightUI.ChangeInlineColor(GetColorFromPalette(new Vector2(localPoint.x, 0)));
                childToMove.gameObject.GetComponent<Image>().color = MainLightUI.inLineChoose;
                MainLightUI.CopyChoose.DrawPaletteLine();
            }
        }
        else if (Input.GetMouseButtonUp(0)) isPressed = false;
    }

    Color GetColorFromPalette(Vector2 localPoint)
    {
        Rect rect = parentRect.rect;
        Texture2D text = gameObject.GetComponent<Image>().sprite.texture;

        float t = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        t = Mathf.Clamp01(t);

        int texX = Mathf.RoundToInt(t * (text.width - 1));
        int texY = text.height / 2;

        return text.GetPixel(texX, texY);
    }
}
