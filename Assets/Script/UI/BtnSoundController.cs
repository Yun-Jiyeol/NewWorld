using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnSoundController : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    public bool isForDown = false;

    [Header("Sound")]
    public SoundData OnData;

    public SoundData BtnDownData;
    public SoundData BtnUpData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(OnData == null) return;

        GameManager.Instance.SoundManager.PlayEffect(OnData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isForDown) return;
        if (BtnDownData == null) return;

        GameManager.Instance.SoundManager.PlayEffect(BtnDownData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isForDown) return;
        if (BtnDownData == null) return;

        GameManager.Instance.SoundManager.PlayEffect(BtnDownData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (BtnUpData == null) return;

        GameManager.Instance.SoundManager.PlayEffect(BtnUpData);
    }
}
