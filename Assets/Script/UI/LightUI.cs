using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LightUI : BasicUI
{
    public CopyChoose CopyChoose;
    public CopyLine CopyLine;

    public TextMeshProUGUI showSettingLight;
    
    float SettingLight;
    Light target;

    public Color _color;
    public Color inLineChoose;

    public override void OnSetting() 
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null) return;
        target = GameManager.Instance.InputManager.TargetGameObject.GetComponent<WorldBaseCode>().ChangeLightPower();
        SettingLight = target.intensity;
        _color = target.color;

        CopyLine.Setting(this);
        CopyChoose.Setting(this);
    }

    public void ChangeSettingLight(int i)
    {
        SettingLight += i;
        ShowSettingLight();
    }

    void ShowSettingLight()
    {
        showSettingLight.text = SettingLight.ToString();
    }

    public void ChangeColor()
    {
        target.color = _color;
    }
}
