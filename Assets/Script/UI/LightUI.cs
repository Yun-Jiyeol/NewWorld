using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public Light target;
    LightType targettype;

    public Color _color;
    public Color inLineChoose;

    GameObject ShowLight;

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

    private void Update()
    {
        if(target == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        GameManager.Instance.InputManager.TargetMove(target.gameObject);
        GameManager.Instance.InputManager.TargetTurn(target.gameObject);

        if(targettype == LightType.Spot)
        {
            ChangeAngle();
        }
    }

    void ChangeAngle()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            target.spotAngle += scroll * 1f;
        }
    }

    void MakeSpere()
    {
        if(ShowLight != null) Destroy(ShowLight);

        if (targettype == LightType.Spot)
        {
            ShowLight = Instantiate(GameManager.Instance.LightManager.RayLight, target.transform);
        }
        if (targettype == LightType.Point)
        {
            ShowLight = Instantiate(GameManager.Instance.LightManager.SphereLight, target.transform);
        }

        ShowLight.layer = 6;
        ShowLight.transform.localScale = Vector3.one * 0.2f;
        SettingColor();
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
        if (SettingLight == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

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
        if (SettingRange == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

        showSettingRange.text = SettingRange.ToString();
    }

    public void ChangeColor()
    {
        target.color = _color;
        SettingColor();
    }

    void SettingColor()
    {
        if (ShowLight == null) return;

        Color showcolor = _color;
        showcolor.a = 1f;
        GameManager.Instance.MaterialManager.LightMaterial.color = showcolor;
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
                MakeSpere();
                SettingColor();
                break;

            case (LightType.Point):
                Spot.gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
                Point.gameObject.GetComponent<Image>().color = new Color(0.5f, 1, 0.5f);
                MakeSpere();
                SettingColor();
                break;
        }
    }

    private void OnDisable()
    {
        if (ShowLight != null) Destroy(ShowLight);
    }

    public void ResetPosition()
    {
        GameManager.Instance.InputManager.TargetGameObject.GetComponent<WorldBaseCode>().SetLightPosition();
    }
}
