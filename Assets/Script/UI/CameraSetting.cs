using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    Dictionary<CameraEffect, GameObject> SlidersSave = new Dictionary<CameraEffect, GameObject>();

    public List<Image> CameraNowShow;
    public Sprite OnSprite;
    public Sprite OffSprite;

    public Transform SpawnPos;
    public GameObject Slider;

    public TMP_InputField FieldOfVeiw_Input;
    public ScrollbarHelper FieldOfVeiw_Scroll;
    float FieldOfView_Value;

    public override void OnSetting()
    {
        if(GameManager.Instance == null) return;

        mainCamera = GameManager.Instance.MainCamera;
        CameraMaterial = mainCamera.GetComponent<CameraShaderPutter>().CamMaterial;

        FieldOfView_Value = mainCamera.GetComponent<Camera>().fieldOfView;
        FieldOfVeiw_Input.text = FieldOfView_Value.ToString();
        FieldOfVeiw_Scroll.Onit(this);
        FieldOfVeiw_Scroll.ChangedValue(FieldOfView_Value);

        SettingCameraNowSetting();
    }

    public void AddSubBtn(float num)
    {
        ChangeFieldOfView(FieldOfView_Value + num);
    }

    public void InputField(string value)
    {
        if (float.TryParse(value, out float number))
        {
            // 숫자면 FOV 변경
            ChangeFieldOfView(number);
        }
        else
        {
            ChangeFieldOfView(FieldOfView_Value);
        }
    }

    public void ChangeFieldOfView(float _FOVV)
    {
        if(_FOVV < 1)
        {
            _FOVV = 1;
        }else if(_FOVV > 180)
        {
            _FOVV = 180;
        }

        mainCamera.GetComponent<Camera>().fieldOfView = _FOVV;
        FieldOfView_Value = mainCamera.GetComponent<Camera>().fieldOfView;  

        FieldOfVeiw_Input.text = FieldOfView_Value.ToString("F0");
        FieldOfVeiw_Scroll.ChangedValue(FieldOfView_Value);
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
        if (CameraNowSetting == null)
        {
            MakeNewSetting();
        }

        CameraNowSetting[CameraEffect.Reverse].isOn = CameraMaterial.GetFloat(((SystemName)0).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.BlackWhite].isOn = CameraMaterial.GetFloat(((SystemName)2).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.ContrastThreshold].isOn = CameraMaterial.GetFloat(((SystemName)4).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.Posterize].isOn = CameraMaterial.GetFloat(((SystemName)6).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.Pixelate].isOn = CameraMaterial.GetFloat(((SystemName)8).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.Vignette].isOn = CameraMaterial.GetFloat(((SystemName)10).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.EdgeDetect].isOn = CameraMaterial.GetFloat(((SystemName)12).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.FilmNoise].isOn = CameraMaterial.GetFloat(((SystemName)14).ToString()) >= 0.5f;
        CameraNowSetting[CameraEffect.CRTScanline].isOn = CameraMaterial.GetFloat(((SystemName)16).ToString()) >= 0.5f;

        CameraNowSetting[CameraEffect.ContrastThreshold].NowPower = CameraMaterial.GetFloat(((SystemName)5).ToString());
        CameraNowSetting[CameraEffect.Posterize].NowPower = CameraMaterial.GetFloat(((SystemName)7).ToString());
        CameraNowSetting[CameraEffect.Pixelate].NowPower = CameraMaterial.GetFloat(((SystemName)9).ToString());
        CameraNowSetting[CameraEffect.Vignette].NowPower = CameraMaterial.GetFloat(((SystemName)11).ToString());
        CameraNowSetting[CameraEffect.EdgeDetect].NowPower = CameraMaterial.GetFloat(((SystemName)13).ToString());
        CameraNowSetting[CameraEffect.FilmNoise].NowPower = CameraMaterial.GetFloat(((SystemName)15).ToString());
        CameraNowSetting[CameraEffect.CRTScanline].NowPower = CameraMaterial.GetFloat(((SystemName)17).ToString());

        CheckAllSlider();

        for (int i = 0; i < CameraNowSetting.Count; i++)
        {
            CheckSetting(i);
        }
    }

    public void ClickThisBtn(int type)
    {
        CameraEffect clickedtype = (CameraEffect)type;
        CameraNowSetting[clickedtype].isOn = !CameraNowSetting[clickedtype].isOn;

        CheckSetting(type);
    }

    private void CheckSetting(int type)
    {
        CameraEffect clickedtype = (CameraEffect)type;

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
                slider.GetComponent<CameraSettingSlider>().Init(clickedtype, this, parent.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text);
                SlidersSave[clickedtype] = slider;
            }
        }
        else //끌때
        {
            CameraMaterial.SetFloat(Name, 0f);

            CameraNowShow[type].sprite = OffSprite;

            if (CameraNowSetting[clickedtype].MakeLine) //조절기 삭제
            {
                Destroy(SlidersSave[clickedtype]);
                SlidersSave[clickedtype] = null;    
            }
        }
    }

    void CheckAllSlider()
    {
        foreach(var CS in CameraNowSetting)
        {
            if (CS.Value.MakeLine)
            {
                CheckSliderbar(CS.Key);
            }
        }
    } 

    void CheckSliderbar(CameraEffect clickedtype)
    {
        if (!SlidersSave.ContainsKey(clickedtype))
        {
            SlidersSave.Add(clickedtype, null);
            return;
        }

        if (SlidersSave[clickedtype] != null)
        {
            Destroy(SlidersSave[clickedtype]);
            SlidersSave[clickedtype] = null;
        }
    }
}
