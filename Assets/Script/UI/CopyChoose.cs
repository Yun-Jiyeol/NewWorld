using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CopyChoose : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform childToMove;
    RectTransform parentRect;

    bool isOnImage = false;

    Color color;

    float Max = 122.5f;
    float Min = -122.5f;

    bool isPressed = false;
    LightUI lightui = null;
    MainLightUI MainLightUI = null;

    [Header("Sound")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData moveSound;
    [SerializeField] private SoundData releaseSound;

    [Header("Move Sound Setting")]
    [SerializeField] private float moveSoundStep = 8f;
    [SerializeField] private float moveSoundVolume = 1f;
    [SerializeField] private float minMovePitch = 0.75f;
    [SerializeField] private float maxMovePitch = 1.35f;

    private Vector2 lastMoveSoundPosition = Vector2.zero;

    private void Awake()
    {
        parentRect = transform as RectTransform;
    }

    public void Setting(LightUI _lightUI)
    {
        lightui = _lightUI;
        MainLightUI = null;

        Color.RGBToHSV(lightui._color, out float h, out float s, out float v);

        float svalue = Mathf.Lerp(Min, Max, s);
        float vvalue = Mathf.Lerp(Min, Max, v);

        childToMove.anchoredPosition = new Vector2(svalue, vvalue);
        lastMoveSoundPosition = childToMove.anchoredPosition;

        DrawPaletteLine();
    }

    public void Setting(MainLightUI _mainLightUI)
    {
        MainLightUI = _mainLightUI;
        lightui = null;

        Color.RGBToHSV(MainLightUI._color, out float h, out float s, out float v);

        float svalue = Mathf.Lerp(Min, Max, s);
        float vvalue = Mathf.Lerp(Min, Max, v);

        childToMove.anchoredPosition = new Vector2(svalue, vvalue);
        lastMoveSoundPosition = childToMove.anchoredPosition;

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
        Color baseColor = new Color();

        if (lightui != null)
        {
            baseColor = lightui.inLineChoose;
        }
        else if (MainLightUI != null)
        {
            baseColor = MainLightUI.inLineChoose;
        }

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
            float xT = x / (float)(width - 1);

            Color horizontalColor = Color.Lerp(Color.white, baseColor, xT);

            for (int y = 0; y < height; y++)
            {
                float yT = 1f - (y / (float)(height - 1));

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

        ApplyCurrentColor();
    }

    private void Update()
    {
        if (!isOnImage) return;

        if (Input.GetMouseButtonDown(0))
        {
            isPressed = true;
            lastMoveSoundPosition = childToMove.anchoredPosition;
            PlaySound(clickSound);
        }

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

            ApplyCurrentColor();

            PlayMoveSoundByPosition(localPoint);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPressed = false;
            PlaySound(releaseSound);
        }
    }

    private void ApplyCurrentColor()
    {
        if (lightui != null)
        {
            lightui._color = GetColorFromPalette(childToMove.anchoredPosition);

            Image handleImage = childToMove
                .transform
                .GetComponentsInChildren<Image>()
                .First(img => img.gameObject != childToMove.gameObject);

            handleImage.color = lightui._color;

            lightui.ChangeColor();
        }
        else if (MainLightUI != null)
        {
            MainLightUI._color = GetColorFromPalette(childToMove.anchoredPosition);

            Image handleImage = childToMove
                .transform
                .GetComponentsInChildren<Image>()
                .First(img => img.gameObject != childToMove.gameObject);

            handleImage.color = MainLightUI._color;

            MainLightUI.ChangeColor();
        }
    }

    Color GetColorFromPalette(Vector2 localPoint)
    {
        Rect rect = parentRect.rect;
        Texture2D tex = GetComponent<Image>().sprite.texture;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        u = Mathf.Clamp01(u);

        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        v = Mathf.Clamp01(v);

        int texX = Mathf.RoundToInt(u * (tex.width - 1));
        int texY = Mathf.RoundToInt(v * (tex.height - 1));

        return tex.GetPixel(texX, texY);
    }

    private void PlayMoveSoundByPosition(Vector2 currentPosition)
    {
        float difference = Vector2.Distance(currentPosition, lastMoveSoundPosition);

        if (difference < moveSoundStep)
            return;

        lastMoveSoundPosition = currentPosition;

        float pitch = GetPitchByPosition(currentPosition);

        PlayMoveSound(pitch);
    }

    private float GetPitchByPosition(Vector2 currentPosition)
    {
        float xT = Mathf.InverseLerp(Min, Max, currentPosition.x);
        float yT = Mathf.InverseLerp(Min, Max, currentPosition.y);

        float t = (xT + yT) * 0.5f;

        return Mathf.Lerp(minMovePitch, maxMovePitch, t);
    }

    private void PlaySound(SoundData soundData)
    {
        if (soundData == null) return;

        GameManager.Instance.SoundManager.PlayEffect(soundData);
    }

    private void PlayMoveSound(float pitch)
    {
        if (moveSound == null) return;

        GameManager.Instance.SoundManager.PlayEffect(
            moveSound,
            moveSoundVolume,
            pitch
        );
    }
}