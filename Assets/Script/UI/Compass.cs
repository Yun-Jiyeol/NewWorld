using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public GameObject UpDownCompass;
    public GameObject RightLeftCompass;

    public TextMeshProUGUI UpDownCompassNumber;
    public TextMeshProUGUI RightLeftCompassNumber;

    public RectTransform[] UpDownCompassSlider;
    float[] saveUpDownposition;
    public RectTransform[] RightLeftCompassSlider;
    float[] saveRightLeftposition;

    float currentAnglex = 0;
    float beforeAnglex = 90;

    float currentAngley = 0;
    float beforeAngley = 0;

    GameObject MC;

    private void Awake()
    {
        MC = Camera.main.gameObject;

        saveUpDownposition = new float[UpDownCompassSlider.Length];
        for (int i = 0; i < UpDownCompassSlider.Length; i++)
        {
            saveUpDownposition[i] = UpDownCompassSlider[i].anchoredPosition.y;
        }

        saveRightLeftposition = new float[RightLeftCompassSlider.Length];
        for (int i = 0; i < RightLeftCompassSlider.Length; i++)
        {
            saveRightLeftposition[i] = RightLeftCompassSlider[i].anchoredPosition.x;
        }
    }

    private void OnEnable()
    {
        beforeAnglex = 90;
        beforeAngley = 0;

        for (int i = 0; i < UpDownCompassSlider.Length; i++)
        {
            UpDownCompassSlider[i].anchoredPosition = new Vector2(0, saveUpDownposition[i]);
        }
        for (int i = 0; i < RightLeftCompassSlider.Length; i++)
        {
            RightLeftCompassSlider[i].anchoredPosition = new Vector2(saveRightLeftposition[i],0 );
        }
    }

    private void Update()
    {
        currentAnglex = MC.transform.rotation.eulerAngles.x;
        if (currentAnglex > 180) currentAnglex -= 360f;
        currentAnglex += 90f;

        currentAngley = MC.transform.rotation.eulerAngles.y;

        ChangePositionx(UpDownCompassSlider);
        ChangePositiony(RightLeftCompassSlider);
    }

    void ChangePositionx(RectTransform[] _sliders)
    {
        if (_sliders.Length == 0) return;
        if(currentAnglex == beforeAnglex) return;
        float anglex = currentAnglex - beforeAnglex;
        anglex *= 15f;

        foreach (RectTransform slider in _sliders)
        {
            slider.anchoredPosition += new Vector2(0,anglex);
        }

        int textx = Mathf.RoundToInt(180 - currentAnglex);
        UpDownCompassNumber.text = textx.ToString();

        beforeAnglex = currentAnglex;

        CheckPositionsx(_sliders);
    }

    void CheckPositionsx(RectTransform[] _sliders) 
    {
        if(_sliders.Length == 0) return;

        foreach(RectTransform slider in _sliders)
        {
            if(slider.anchoredPosition.y <= -600f)
            {
                while (slider.anchoredPosition.y <= -600f)
                {
                    slider.anchoredPosition += new Vector2(0, 1050);
                }
            }
            else if(slider.anchoredPosition.y > 600f)
            {
                while (slider.anchoredPosition.y > -600f)
                {
                    slider.anchoredPosition -= new Vector2(0, 1050);
                }
            }
        }
    }

    void ChangePositiony(RectTransform[] _sliders)
    {
        if (_sliders.Length == 0) return;
        if (currentAngley == beforeAngley) return;
        float angley = currentAngley - beforeAngley;
        angley *= 15f;

        foreach (RectTransform slider in _sliders)
        {
            slider.anchoredPosition += new Vector2(angley, 0);
        }

        int texty = Mathf.RoundToInt(currentAngley);
        RightLeftCompassNumber.text = texty.ToString();

        beforeAngley = currentAngley;

        CheckPositionsy(_sliders);
    }

    void CheckPositionsy(RectTransform[] _sliders)
    {
        if (_sliders.Length == 0) return;

        foreach (RectTransform slider in _sliders)
        {
            if (slider.anchoredPosition.x <= -750f)
            {
                while (slider.anchoredPosition.x <= -750f)
                {
                    slider.anchoredPosition += new Vector2(1350, 0);
                }
            }
            else if (slider.anchoredPosition.x > 750f)
            {
                while (slider.anchoredPosition.x > 750f)
                {
                    slider.anchoredPosition -= new Vector2(1350, 0);
                }
            }
        }
    }
}
