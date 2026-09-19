using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundControl : NeedToDoOnit
{
    public SoundEnum thisSoundEnum;

    public TextMeshProUGUI VolumeValue;
    public List<Image> ShowValues;

    public Sprite OnSprite;
    public Sprite OffSprite;

    SoundManager soundManager;

    // 현재 활성화되어 있는 SoundControl들
    static List<SoundControl> soundControls = new List<SoundControl>();


    private void OnEnable()
    {
        if (!soundControls.Contains(this))
        {
            soundControls.Add(this);
        }
    }

    private void OnDisable()
    {
        soundControls.Remove(this);
    }


    public override void Onit()
    {
        soundManager = GameManager.Instance.SoundManager;

        Setting();
    }


    void Setting()
    {
        if (soundManager == null)
        {
            soundManager = GameManager.Instance.SoundManager;
        }

        float displayValue = 0f;
        float imageValue = 0f;

        switch (thisSoundEnum)
        {
            case SoundEnum.Main:
                // 글자
                displayValue = soundManager.MainSoundVolume;

                // 이미지
                imageValue = soundManager.MainSoundVolume;
                break;


            case SoundEnum.Effect:
                // 실제로 들리는 볼륨을 글자로 표시
                displayValue =
                    soundManager.MainSoundVolume *
                    soundManager.EffectVolume;

                // 이미지 단계는 Effect 자체 설정값
                imageValue =
                    soundManager.EffectVolume;
                break;


            case SoundEnum.Music:
                // 실제로 들리는 볼륨을 글자로 표시
                displayValue =
                    soundManager.MainSoundVolume *
                    soundManager.BGMVolume;

                // 이미지 단계는 BGM 자체 설정값
                imageValue =
                    soundManager.BGMVolume;
                break;
        }


        displayValue = Mathf.Clamp01(displayValue);
        imageValue = Mathf.Clamp01(imageValue);


        // -------------------------
        // 퍼센트 글자
        // -------------------------

        int percent =
            Mathf.RoundToInt(displayValue * 100f);

        VolumeValue.text =
            percent.ToString() + "%";


        // -------------------------
        // 단계 이미지
        // -------------------------

        int count =
            Mathf.RoundToInt(
                imageValue * ShowValues.Count
            );

        count =
            Mathf.Clamp(
                count,
                0,
                ShowValues.Count
            );


        for (int i = 0; i < ShowValues.Count; i++)
        {
            if (i < count)
            {
                ShowValues[i].sprite = OnSprite;
            }
            else
            {
                ShowValues[i].sprite = OffSprite;
            }
        }
    }


    public void ClickSoundChangeBtn(float i)
    {
        switch (thisSoundEnum)
        {
            case SoundEnum.Main:

                soundManager.SetMainVolume(
                    soundManager.MainSoundVolume + i
                );

                soundManager.PlayEffectIgnoreVolume(
                    SoundChoose.CallAudio(
                        ClickSound.SoundSettingBtn
                    ),
                    soundManager.MainSoundVolume
                );

                break;


            case SoundEnum.Effect:

                soundManager.SetEffectVolume(
                    soundManager.EffectVolume + i
                );

                soundManager.PlayEffectIgnoreVolume(
                    SoundChoose.CallAudio(
                        ClickSound.SoundSettingBtn
                    ),
                    soundManager.MainSoundVolume *
                    soundManager.EffectVolume
                );

                break;


            case SoundEnum.Music:

                soundManager.SetBGMVolume(
                    soundManager.BGMVolume + i
                );

                soundManager.PlayEffectIgnoreVolume(
                    SoundChoose.CallAudio(
                        ClickSound.SoundSettingBtn
                    ),
                    soundManager.MainSoundVolume *
                    soundManager.BGMVolume
                );

                break;
        }


        // Main / Effect / BGM UI 전부 다시 계산
        RefreshAll();


        GameManager.Instance
            .SaveAndLoadManager
            .PlayerSetting_SaveSound();
    }


    static void RefreshAll()
    {
        for (int i = 0; i < soundControls.Count; i++)
        {
            if (soundControls[i] != null)
            {
                soundControls[i].Setting();
            }
        }
    }
}