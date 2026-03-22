using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CameraEffect
{
    Reverse,
    BlackWhite,
    ContrastThreshold,
    Posterize,
    Pixelate,
    Vignette,
    EdgeDetect,
    FilmNoise,
    CRTScanline
}

public class CameraState
{
    public bool isOn;
    public bool MakeLine;

    public float NowPower;
    public float MinPower;
    public float MaxPower;
}

public class CameraSetting : BasicUI
{
    Material CameraMaterial;
    Dictionary<CameraEffect, CameraState> CameraNowSetting;

    public List<Image> CameraNowShow;
    public Sprite OnSprite;
    public Sprite OffSprite;

    public override void OnSetting()
    {
        if(GameManager.Instance == null) return;

        CameraMaterial = GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().CamMaterial;
        if (CameraNowSetting == null) MakeNewSetting();
    }

    void MakeNewSetting()
    {
        CameraNowSetting = new Dictionary<CameraEffect, CameraState>();
        CameraNowSetting.Add(CameraEffect.Reverse, new CameraState() { isOn = false, MakeLine = false });
        CameraNowSetting.Add(CameraEffect.BlackWhite, new CameraState() { isOn = false, MakeLine = false });
        CameraNowSetting.Add(CameraEffect.ContrastThreshold, new CameraState() { isOn = false, MakeLine = true, NowPower = 0.5f, MinPower = 0f, MaxPower = 1f });
        CameraNowSetting.Add(CameraEffect.Posterize, new CameraState() { isOn = false, MakeLine = true, NowPower = 4f, MinPower = 2f, MaxPower = 32f });
        CameraNowSetting.Add(CameraEffect.Pixelate, new CameraState() { isOn = false, MakeLine = true, NowPower = 254f, MinPower = 32f, MaxPower = 1024f });
        CameraNowSetting.Add(CameraEffect.Vignette, new CameraState() { isOn = false, MakeLine = true, NowPower = 1f, MinPower = 0f, MaxPower = 2f });
        CameraNowSetting.Add(CameraEffect.EdgeDetect, new CameraState() { isOn = false, MakeLine = true, NowPower = 1f, MinPower = 0f, MaxPower = 5f });
        CameraNowSetting.Add(CameraEffect.FilmNoise, new CameraState() { isOn = false, MakeLine = true, NowPower = 0.1f, MinPower = 0f, MaxPower = 1f });
        CameraNowSetting.Add(CameraEffect.CRTScanline, new CameraState() { isOn = false, MakeLine = true, NowPower = 0.2f, MinPower = 0f, MaxPower = 1f });
    }

    public void ClickThisBtn(int type)
    {
        CameraEffect clickedtype = (CameraEffect)type;
        CameraNowSetting[clickedtype].isOn = !CameraNowSetting[clickedtype].isOn;

        if (CameraNowSetting[clickedtype].isOn) //킬때
        {
            CameraNowShow[type].sprite = OnSprite;
            if (CameraNowSetting[clickedtype].MakeLine) //조절기 제작
            {

            }
        }
        else //끌때
        {
            CameraNowShow[type].sprite = OffSprite;
        }
    }
}
