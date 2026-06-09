using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public class CameraSettingSlider : MonoBehaviour
{
    public TMP_InputField inputfield;
    public Slider slider;

    public TextMeshProUGUI WhatSlide;

    CameraSetting parent;
    CameraEffect thistype;
    Material CameraMaterial;

    float Max = 0;
    float Min = 0;
    float NowValue = 0;

    bool isinit = false;

    public void Init(CameraEffect type, CameraSetting cam, string _name)
    {
        thistype = type;
        parent = cam;
        WhatSlide.text = _name;
        CameraMaterial = GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().CamMaterial;

        Max = parent.CameraNowSetting[thistype].MaxPower;
        Min = parent.CameraNowSetting[thistype].MinPower;
        NowValue = parent.CameraNowSetting[thistype].NowPower;
        
        slider.value = NowValue;

        slider.minValue = Min;

        slider.maxValue = Max;

        SettingSlider();
        SettingInputField();
        SettingCameraEffect();

        isinit = true;
    }

    public void OnSliderChange()
    {
        if (!isinit) return;

        float value = slider.value;
        NowValue = value;

        SettingInputField();
        SettingCameraEffect();
    }

    void SettingSlider()
    {
        slider.value = NowValue;
    }

    public void EndChange()
    {
        string _text = inputfield.text;

        if (float.TryParse(_text, out float inputValue))
        {
            if (inputValue > Max)
            {
                inputValue = Max;
            }
            else if (inputValue < Min)
            {
                inputValue = Min;
            }

            NowValue = inputValue;

            inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = NowValue.ToString("0.##");
            inputfield.text = "";
        }
        else
        {
            inputfield.text = "";
        }

        SettingSlider();
        SettingCameraEffect();
    }

    void SettingInputField()
    {
        inputfield.placeholder.GetComponent<TextMeshProUGUI>().text = NowValue.ToString("0.##");
    }

    void SettingCameraEffect()
    {
        int num = (int)thistype;

        string Value = ((SystemName)(num * 2 + 1)).ToString();
        CameraMaterial.SetFloat(Value, NowValue);
    }

    public void ClickSidePoint(int i)
    {
        float input = (Max - Min) / 50f;

        NowValue += input * i;

        NowValue = Mathf.Min(Max, NowValue);
        NowValue = Mathf.Max(Min, NowValue);

        SettingSlider();
        SettingInputField();
        SettingCameraEffect();
    }
}
