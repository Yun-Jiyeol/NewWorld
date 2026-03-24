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

    bool isLight = true;
    bool isFilter = true;

    public override void OnSetting()
    {
        if (GameManager.Instance == null) return;

        MainLight = GameManager.Instance.LightManager.MainLight.GetComponent<Light>();
        SubLight = GameManager.Instance.LightManager.SubLight;
        subCamPutter = GameManager.Instance.MainCamera.SubCamera.GetComponent<CameraShaderPutter>();
    }

    public void OnClick(int type)
    {
        switch (type)
        {
            case 0:
                isLight = !isLight;
                if (isLight)
                {
                    HelpingImages[0].sprite = OnSprite;
                }
                else
                {
                    HelpingImages[0].sprite = OffSprite;
                }
                UpdateLight();
                break;
            case 1:
                isFilter = !isFilter;
                if (isFilter)
                {
                    HelpingImages[1].sprite = OnSprite;
                }
                else
                {
                    HelpingImages[1].sprite = OffSprite;
                }
                UpdateFilter();
                break;
        }
    }

    void UpdateFilter()
    {
        if (isFilter)
        {
            subCamPutter.isUsingEffect = true;
        }
        else
        {
            subCamPutter.isUsingEffect = false;
        }
    }

    void UpdateLight()
    {
        if (isLight)
        {
            MainLight.cullingMask = ~0;
            SubLight.SetActive(false);
        }
        else
        {
            MainLight.cullingMask &= ~(1 << PreviewWorldLayer);
            SubLight.SetActive(true);
        }
    }
}
