using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public class CameraSettingSlider : MonoBehaviour
{
    public TMP_InputField inputfield;
    public Slider slider;

    CameraSetting parent;
    CameraEffect thistype;
    Material CameraMaterial;

    float Max = 0;
    float Min = 0;
    float NowValue = 0;

    public void Init(CameraEffect type, CameraSetting cam)
    {
        thistype = type;
        parent = cam;
        CameraMaterial = GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().CamMaterial;

        Max = parent.CameraNowSetting[thistype].MaxPower;
        Min = parent.CameraNowSetting[thistype].MinPower;
        NowValue = parent.CameraNowSetting[thistype].NowPower;

        slider.minValue = Min;
        slider.maxValue = Max;

        SettingSlider();
        SettingInputField();
        SettingCameraEffect();
    }

    public void OnSliderChange()
    {
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
}
