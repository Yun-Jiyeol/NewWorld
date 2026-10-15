using UnityEngine;

public class ScreenSizes : MonoBehaviour
{
    ScreenSettingManager ScreenSettingManager;

    public UI_BtnEffect Fullscreen;
    public UI_BtnEffect Windowed;

    public UI_BtnEffect Resolution1280x720;
    public UI_BtnEffect Resolution1600x900;
    public UI_BtnEffect Resolution1920x1080;
    public UI_BtnEffect Resolution2560x1440;
    public UI_BtnEffect Resolution3840x2160;

    public void Onit()
    {
        ScreenSettingManager = GameManager.Instance.ScreenSettingManager; 

        Setting();
    }

    public void ClickFullscreen()
    {
        if (ScreenSettingManager.CurrentScreenMode ==
            ScreenModeType.Fullscreen)
        {
            return;
        }

        ScreenSettingManager.SetFullscreen();

        PlayClickSound();
        Setting();
    }

    public void ClickWindowed()
    {
        if (ScreenSettingManager.CurrentScreenMode ==
            ScreenModeType.Windowed)
        {
            return;
        }

        ScreenSettingManager.SetWindowed();

        PlayClickSound();
        Setting();
    }

    public void Click1280x720()
    {
        ClickResolution(0);
    }

    public void Click1600x900()
    {
        ClickResolution(1);
    }

    public void Click1920x1080()
    {
        ClickResolution(2);
    }

    public void Click2560x1440()
    {
        ClickResolution(3);
    }

    public void Click3840x2160()
    {
        ClickResolution(4);
    }

    void ClickResolution(int index)
    {
        if (ScreenSettingManager.CurrentScreenMode ==
                ScreenModeType.Windowed &&
            ScreenSettingManager.CurrentResolutionIndex == index)
        {
            return;
        }

        ScreenSettingManager.SetWindowedResolution(index);

        PlayClickSound();
        Setting();
    }

    void Setting()
    {
        ResetEffect();

        if (ScreenSettingManager.CurrentScreenMode ==
            ScreenModeType.Fullscreen)
        {
            Fullscreen.HoldEnter();
            return;
        }

        Windowed.HoldEnter();

        switch (ScreenSettingManager.CurrentResolutionIndex)
        {
            case 0:
                Resolution1280x720.HoldEnter();
                break;

            case 1:
                Resolution1600x900.HoldEnter();
                break;

            case 2:
                Resolution1920x1080.HoldEnter();
                break;

            case 3:
                Resolution2560x1440.HoldEnter();
                break;

            case 4:
                Resolution3840x2160.HoldEnter();
                break;
        }
    }

    void ResetEffect()
    {
        Fullscreen.UnHoldChange();
        Windowed.UnHoldChange();

        Resolution1280x720.UnHoldChange();
        Resolution1600x900.UnHoldChange();
        Resolution1920x1080.UnHoldChange();
        Resolution2560x1440.UnHoldChange();
        Resolution3840x2160.UnHoldChange();
    }

    void PlayClickSound()
    {
        GameManager.Instance.SoundManager.PlayEffect(
            SoundChoose.CallAudio(
                ClickSound.SimplyUITap
            )
        );
    }
}
