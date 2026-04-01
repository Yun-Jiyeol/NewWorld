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
    MainCamera mainCamera;
    Material CameraMaterial;
    public Dictionary<CameraEffect, CameraState> CameraNowSetting;

    public List<Image> CameraNowShow;
    public Sprite OnSprite;
    public Sprite OffSprite;

    public Transform SpawnPos;
    public GameObject Slider;

    public override void OnSetting()
    {
        if(GameManager.Instance == null) return;

        mainCamera = GameManager.Instance.MainCamera;
        CameraMaterial = mainCamera.GetComponent<CameraShaderPutter>().CamMaterial;
        SettingCameraNowSetting();
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

    void SettingCameraNowSetting()
    {
        if (CameraNowSetting == null) MakeNewSetting();

        CameraNowSetting[CameraEffect.Reverse].isOn = CameraMaterial.GetFloat(((SystemName)0).ToString()) == 1f;
        CameraNowSetting[CameraEffect.BlackWhite].isOn = CameraMaterial.GetFloat(((SystemName)2).ToString()) == 1f;
        CameraNowSetting[CameraEffect.ContrastThreshold].isOn = CameraMaterial.GetFloat(((SystemName)4).ToString()) == 1f;
        CameraNowSetting[CameraEffect.Posterize].isOn = CameraMaterial.GetFloat(((SystemName)6).ToString()) == 1f;
        CameraNowSetting[CameraEffect.Pixelate].isOn = CameraMaterial.GetFloat(((SystemName)8).ToString()) == 1f;
        CameraNowSetting[CameraEffect.Vignette].isOn = CameraMaterial.GetFloat(((SystemName)10).ToString()) == 1f;
        CameraNowSetting[CameraEffect.EdgeDetect].isOn = CameraMaterial.GetFloat(((SystemName)12).ToString()) == 1f;
        CameraNowSetting[CameraEffect.FilmNoise].isOn = CameraMaterial.GetFloat(((SystemName)14).ToString()) == 1f;
        CameraNowSetting[CameraEffect.CRTScanline].isOn = CameraMaterial.GetFloat(((SystemName)16).ToString()) == 1f;

        CameraNowSetting[CameraEffect.ContrastThreshold].NowPower = CameraMaterial.GetFloat(((SystemName)5).ToString());
        CameraNowSetting[CameraEffect.Posterize].NowPower = CameraMaterial.GetFloat(((SystemName)7).ToString());
        CameraNowSetting[CameraEffect.Pixelate].NowPower = CameraMaterial.GetFloat(((SystemName)9).ToString());
        CameraNowSetting[CameraEffect.Vignette].NowPower = CameraMaterial.GetFloat(((SystemName)11).ToString());
        CameraNowSetting[CameraEffect.EdgeDetect].NowPower = CameraMaterial.GetFloat(((SystemName)13).ToString());
        CameraNowSetting[CameraEffect.FilmNoise].NowPower = CameraMaterial.GetFloat(((SystemName)15).ToString());
        CameraNowSetting[CameraEffect.CRTScanline].NowPower = CameraMaterial.GetFloat(((SystemName)17).ToString());
    }

    public void ClickThisBtn(int type)
    {
        CameraEffect clickedtype = (CameraEffect)type;
        CameraNowSetting[clickedtype].isOn = !CameraNowSetting[clickedtype].isOn;

        string Name = ((SystemName)(type * 2)).ToString();
        string Value = ((SystemName)(type * 2 + 1)).ToString();

        if (CameraNowSetting[clickedtype].isOn) //킬때
        {
            CameraMaterial.SetFloat(Name, 1f);

            CameraNowShow[type].sprite = OnSprite;
            Transform parent = CameraNowShow[type].transform.parent.parent.transform;
            int index = parent.transform.GetSiblingIndex();

            if (CameraNowSetting[clickedtype].MakeLine) //조절기 제작
            {
                GameObject slider = Instantiate(Slider, SpawnPos);
                slider.transform.SetSiblingIndex(index + 1);
                SpawnPos.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 300f);
                slider.GetComponent<CameraSettingSlider>().Init(clickedtype, this);
            }
        }
        else //끌때
        {
            CameraMaterial.SetFloat(Name, 0f);

            CameraNowShow[type].sprite = OffSprite;

            if (CameraNowSetting[clickedtype].MakeLine) //조절기 삭제
            {
                Transform parent = CameraNowShow[type].transform.parent.parent;

                int index = parent.transform.GetSiblingIndex();

                GameObject nextObj = null;
                if (index + 1 < parent.parent.childCount)
                {
                    nextObj = parent.parent.GetChild(index + 1).gameObject;
                }

                Destroy(nextObj);
            }
        }
    }
}
