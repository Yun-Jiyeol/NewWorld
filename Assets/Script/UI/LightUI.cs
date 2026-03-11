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

    public ClickingImageToCoroutine RightAngleClick;
    public ClickingImageToCoroutine LeftAngleClick;

    public TextMeshProUGUI showSettingLight;
    public TMP_InputField showSettingLightInput;

    public TextMeshProUGUI showSettingRange;
    public TMP_InputField showSettingRangeInput;

    public GameObject SettingAngleGo;
    public TextMeshProUGUI showSettingAngle;
    public TMP_InputField showSettingAngleInput;

    public ChangeLightTypeButton Spot;
    public ChangeLightTypeButton Point;

    float SettingLight;
    float SettingRange;
    float SettingAngle;
    int MaxLight = 999;
    int MinLight = 0;

    public Light target;
    LightType targettype;

    public Color _color;
    public Color inLineChoose;

    GameObject ShowLight;

    public override void OnSetting() 
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null) return;
        target = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>().ChangeLightPower();
        SettingLight = (int)target.intensity;
        _color = target.color;
        targettype = target.type;
        SettingRange = target.range;
        SettingAngle = target.spotAngle;

        CopyLine.Setting(this);
        CopyChoose.Setting(this);
        RightClick.Setting(this);
        LeftClick.Setting(this);
        RightRangeClick.Setting(this);
        LeftRangeClick.Setting(this);
        RightAngleClick.Setting(this);
        LeftAngleClick.Setting(this);
        Spot.Setting(this);
        Point.Setting(this);

        SettingType();
        ShowSettingLightRange();
        ShowSettingLight();
        ShowSettingLightAngle();
    }

    private void Update()
    {
        if(target == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        GameManager.Instance.InputManager.TargetMove(target.gameObject);
        GameManager.Instance.InputManager.TargetTurn(target.gameObject);
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

        Vector3Int SZ = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>().GiveThreeSize();
        float maxSZ = Mathf.Max(SZ.x, SZ.y, SZ.z);

        ShowLight.transform.localScale = Vector3.one  * (maxSZ / 10f);
        SettingColor();
    }

    public void ChangeSettingLight(float i)
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

    public void ChangeSettingLightNum()
    {
        if (!float.TryParse(showSettingLightInput.text, out float x))
        {
            showSettingLightInput.text = "";
            return;
        }

        SettingLight = x;

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

        showSettingLightInput.text = "";
        showSettingLight.text = SettingLight.ToString("F1");
    }

    public void ChangeSettingLightRange(float i)
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

        target.range = SettingRange * 10f;
        ShowSettingLightRange();
    }

    public void ChangeSettingLightRangeNum()
    {
        if (!float.TryParse(showSettingRangeInput.text, out float x))
        {
            showSettingRangeInput.text = "";
            return;
        }

        SettingRange = x;

        if (SettingRange < MinLight)
        {
            SettingRange = MinLight;
        }
        if (SettingRange > MaxLight)
        {
            SettingRange = MaxLight;
        }

        target.range = SettingRange * 10f;
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

        showSettingRangeInput.text = "";
        showSettingRange.text = SettingRange.ToString("F1");
    }

    public void ChangeSettingLightAngle(float i)
    {
        SettingAngle += i;

        if (SettingAngle < 0)
        {
            SettingAngle = 0;
        }
        if (SettingAngle > 179)
        {
            SettingAngle = 179;
        }

        target.spotAngle = SettingAngle;
        ShowSettingLightAngle();
    }

    public void ChangeSettingLightAngleNum()
    {
        if (!float.TryParse(showSettingAngleInput.text, out float x))
        {
            showSettingAngleInput.text = "";
            return;
        }

        SettingAngle = x;

        if (SettingAngle < 0)
        {
            SettingAngle = 0;
        }
        if (SettingAngle > 179)
        {
            SettingAngle = 179;
        }

        target.spotAngle = SettingAngle;
        ShowSettingLightAngle();
    }

    void ShowSettingLightAngle()
    {
        if (SettingAngle == 0)
        {
            target.enabled = false;
        }
        else
        {
            target.enabled = true;
        }

        showSettingAngleInput.text = "";
        showSettingAngle.text = SettingAngle.ToString("F1");
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
                SettingAngleGo.SetActive(true);
                MakeSpere();
                SettingColor();
                break;

            case (LightType.Point):
                Spot.gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
                Point.gameObject.GetComponent<Image>().color = new Color(0.5f, 1, 0.5f);
                SettingAngleGo.SetActive(false);
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
        GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>().SetLightPosition();
    }
}
