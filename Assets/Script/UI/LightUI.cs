using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DG.Tweening.DOTweenAnimation;
using static UnityEngine.GraphicsBuffer;

public class LightUI : BasicUI
{
    public CopyChoose CopyChoose;
    public CopyLine CopyLine;
    public ClickImagetoChange RightClick;
    public ClickImagetoChange LeftClick;
    public ClickImageToChangeRange RightRangeClick;
    public ClickImageToChangeRange LeftRangeClick;

    public TextMeshProUGUI showSettingLight;
    public TextMeshProUGUI showSettingRange;

    public ChangeLightTypeButton Spot;
    public ChangeLightTypeButton Point;

    int SettingLight;
    float SettingRange;
    int MaxLight = 99;
    int MinLight = 0;

    Light target;
    LightType targettype;

    public Color _color;
    public Color inLineChoose;

    public override void OnSetting() 
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null) return;
        target = GameManager.Instance.InputManager.TargetGameObject.GetComponent<WorldBaseCode>().ChangeLightPower();
        SettingLight = (int)target.intensity;
        _color = target.color;
        targettype = target.type;
        SettingRange = target.range;

        CopyLine.Setting(this);
        CopyChoose.Setting(this);
        RightClick.Setting(this);
        LeftClick.Setting(this);
        RightRangeClick.Setting(this);
        LeftRangeClick.Setting(this);
        Spot.Setting(this);
        Point.Setting(this);

        SettingType();
        ShowSettingLightRange();
        ShowSettingLight();
    }

    public void ChangeSettingLight(int i)
    {
        SettingLight += i;

        if (SettingLight < MinLight)
        {
            SettingLight = MinLight;
        }
        if (SettingLight > MaxLight)
        {
            SettingLight = MaxLight;
        }

        target.intensity = SettingLight;
        ShowSettingLight();
    }

    void ShowSettingLight()
    {
        showSettingLight.text = SettingLight.ToString();
    }

    public void ChangeSettingLightRange(int i)
    {
        SettingRange += i;

        if (SettingRange < MinLight)
        {
            SettingRange = MinLight;
        }
        if (SettingRange > MaxLight)
        {
            SettingRange = MaxLight;
        }

        target.range = SettingRange * 0.5f;
        ShowSettingLightRange();
    }

    void ShowSettingLightRange()
    {
        showSettingRange.text = SettingRange.ToString();
    }

    public void ChangeColor()
    {
        target.color = _color;
    }

    public void ChangeType(LightType _type)
    {
        if(targettype == _type) return;
        targettype = _type;
        target.type = _type;
        SettingType();
    }

    void SettingType()
    {
        switch (targettype)
        {
            case (LightType.Spot):
                Spot.gameObject.GetComponent<Image>().color = new Color(1, 0.5f, 0.5f);
                Point.gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
                break;

            case (LightType.Point):
                Spot.gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
                Point.gameObject.GetComponent<Image>().color = new Color(0.5f, 1, 0.5f);
                break;
        }
    }
}
