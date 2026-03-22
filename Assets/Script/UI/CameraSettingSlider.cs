using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettingSlider : MonoBehaviour
{
    public TMP_InputField inputfield;
    public Slider slider;

    CameraSetting parent;
    CameraEffect thistype;

    public void Init(CameraEffect type, CameraSetting cam)
    {
        thistype = type;
        parent = cam;
    }

    public void OnSliderChange()
    {

    }

    public void EndChange()
    {

    }
}
