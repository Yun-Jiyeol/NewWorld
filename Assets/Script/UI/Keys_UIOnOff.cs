using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Keys_UIOnOff : MonoBehaviour, IPointerDownHandler
{
    public Keys_Type ParentGameObject;
    RectTransform ParentRect;
    public float HowMuchMake;

    public RectTransform Contents;
    public RectTransform ExTendGameObject;
    bool isExtend = false;

    UI_BtnEffect uibtneffect;

    public ClickSound clickSound = ClickSound.None;

    private void Awake()
    {
        ParentRect = ParentGameObject.gameObject.GetComponent<RectTransform>();
        uibtneffect = GetComponent<UI_BtnEffect>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DOTween.IsTweening(ParentRect)) return;
        if (DOTween.IsTweening(Contents)) return;
        if (DOTween.IsTweening(ExTendGameObject)) return;

        if (clickSound != ClickSound.None)
        {
            GameManager.Instance.SoundManager.PlayEffect(SoundChoose.CallAudio(clickSound));
        }

        if (isExtend)
        {
            isExtend = false;

            ParentRect.DOSizeDelta(ParentRect.sizeDelta - new Vector2(0, HowMuchMake), 0.2f);
            Contents.DOSizeDelta(Contents.sizeDelta - new Vector2(0, HowMuchMake), 0.2f);
            ExTendGameObject.DOScaleY(0, 0.2f);

            uibtneffect.UnHoldChange();
        }
        else
        {
            isExtend = true;

            ParentRect.DOSizeDelta(ParentRect.sizeDelta + new Vector2(0, HowMuchMake), 0.2f);
            Contents.DOSizeDelta(Contents.sizeDelta + new Vector2(0, HowMuchMake), 0.2f);
            ExTendGameObject.DOScaleY(1, 0.2f);

            uibtneffect.HoldChange();
        }
    }
}
