using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarSoundEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scrollbar")]
    [SerializeField] private Scrollbar targetScrollbar;

    [Header("SoundData")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData moveSound;
    [SerializeField] private SoundData releaseSound;

    [Header("Move Sound Setting")]
    [SerializeField] private float moveSoundStep = 0.05f;
    [SerializeField] private float minMovePitch = 0.75f;
    [SerializeField] private float maxMovePitch = 1.35f;
    [SerializeField] private float moveSoundVolume = 1f;

    [Header("Option")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playReleaseSound = true;
    [SerializeField] private bool onlyPlayMoveSoundWhileDragging = true;

    private bool isDragging = false;
    private float lastMoveSoundValue = 0f;

    private void Awake()
    {
        if (targetScrollbar == null)
        {
            targetScrollbar = GetComponent<Scrollbar>();
        }
    }

    private void OnEnable()
    {
        if (targetScrollbar != null)
        {
            targetScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            lastMoveSoundValue = targetScrollbar.value;
        }
    }

    private void OnDisable()
    {
        if (targetScrollbar != null)
        {
            targetScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
        }

        isDragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartScrollbarSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndScrollbarSound();
    }

    private void OnScrollbarValueChanged(float value)
    {
        if (onlyPlayMoveSoundWhileDragging && !isDragging)
            return;

        PlayMoveSoundByCurrentValueWithStep();
    }
    private void PlayMoveSoundByCurrentValueWithStep()
    {
        if (targetScrollbar == null) return;

        float currentValue = targetScrollbar.value;
        float difference = Mathf.Abs(currentValue - lastMoveSoundValue);

        if (difference < moveSoundStep)
            return;

        lastMoveSoundValue = currentValue;

        float pitch = GetPitchByCurrentScrollbarValue();

        PlayMoveSound(pitch);
    }

    public void PlayMoveSoundByCurrentValue()
    {
        if (targetScrollbar == null) return;

        float currentValue = targetScrollbar.value;

        lastMoveSoundValue = currentValue;

        float pitch = GetPitchByCurrentScrollbarValue();

        PlayMoveSound(pitch);
    }

    private float GetPitchByCurrentScrollbarValue()
    {
        if (targetScrollbar == null)
            return 1f;

        float currentValue = Mathf.Clamp01(targetScrollbar.value);

        return Mathf.Lerp(minMovePitch, maxMovePitch, currentValue);
    }

    public void StartScrollbarSound()
    {
        if (targetScrollbar == null) return;

        isDragging = true;
        lastMoveSoundValue = targetScrollbar.value;

        if (playClickSound)
        {
            PlaySound(clickSound);
        }
    }

    public void EndScrollbarSound()
    {
        if (targetScrollbar == null) return;

        isDragging = false;

        if (playReleaseSound)
        {
            PlaySound(releaseSound);
        }
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