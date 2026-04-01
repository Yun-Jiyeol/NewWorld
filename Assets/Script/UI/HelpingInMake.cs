using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpingInMake : BasicUI
{
    CameraShaderPutter subCamPutter;

    Light MainLight;
    GameObject SubLight;
    int PreviewWorldLayer = 7;

    public List<Image> HelpingImages;
    public Sprite OnSprite;
    public Sprite OffSprite;

    public bool isLight = true;
    public bool isFilter = true;

    public override void OnSetting()
    {
        if (GameManager.Instance == null) return;

        MainLight = GameManager.Instance.LightManager.MainLight.GetComponent<Light>();
        SubLight = GameManager.Instance.LightManager.SubLight;
        subCamPutter = GameManager.Instance.MainCamera.SubCamera.GetComponent<CameraShaderPutter>();

        UpdateLight();
        UpdateFilter();
    }

    public void OnClick(int type)
    {
        switch (type)
        {
            case 0:
                isLight = !isLight;
                UpdateLight();
                break;
            case 1:
                isFilter = !isFilter;
                UpdateFilter();
                break;
        }
    }

    void UpdateLight()
    {
        if (isLight)
        {
            HelpingImages[0].sprite = OnSprite;
            MainLight.cullingMask = ~0;
            SubLight.SetActive(false);
        }
        else
        {
            HelpingImages[0].sprite = OffSprite;
            MainLight.cullingMask &= ~(1 << PreviewWorldLayer);
            SubLight.SetActive(true);
        }
    }

    void UpdateFilter()
    {
        if (isFilter)
        {
            HelpingImages[1].sprite = OnSprite;
            subCamPutter.isUsingEffect = true;
        }
        else
        {
            HelpingImages[1].sprite = OffSprite;
            subCamPutter.isUsingEffect = false;
        }
    }
}
