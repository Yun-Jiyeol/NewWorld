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

    bool isPressed = false;

    float Max = 122.5f;
    float Min = -122.5f;

    [Header("Sound")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData moveSound;
    [SerializeField] private SoundData releaseSound;

    [Header("Move Sound Setting")]
    [SerializeField] private float moveSoundStep = 8f;
    [SerializeField] private float moveSoundVolume = 1f;
    [SerializeField] private float minMovePitch = 0.75f;
    [SerializeField] private float maxMovePitch = 1.35f;

    private float lastMoveSoundX = 0f;

    private void Awake()
    {
        parentRect = transform as RectTransform;
    }

    public void Setting()
    {
        float value = Mathf.Lerp(Min, Max, GameManager.Instance.BuildManager.alfar);
        childToMove.anchoredPosition = new Vector2(value, -25f);

        lastMoveSoundX = value;
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

        if (Input.GetMouseButtonDown(0))
        {
            isPressed = true;
            lastMoveSoundX = childToMove.anchoredPosition.x;
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

            childToMove.anchoredPosition = new Vector2(localPoint.x, -25f);

            GameManager.Instance.BuildManager.alfar = Mathf.InverseLerp(Min, Max, localPoint.x);

            PlayMoveSoundByX(localPoint.x);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPressed = false;
            PlaySound(releaseSound);
        }
    }

    private void PlayMoveSoundByX(float currentX)
    {
        float difference = Mathf.Abs(currentX - lastMoveSoundX);

        if (difference < moveSoundStep)
            return;

        lastMoveSoundX = currentX;

        float pitch = GetPitchByX(currentX);

        PlayMoveSound(pitch);
    }

    private float GetPitchByX(float currentX)
    {
        float t = Mathf.InverseLerp(Min, Max, currentX);
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