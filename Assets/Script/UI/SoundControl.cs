using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundControl : NeedToDoOnit
{
    public SoundEnum thisSoundEnum;

    public TextMeshProUGUI VolumeValue;
    public List<Image> ShowValues;

    public Gradient gradient;

    public Sprite OnSprite;
    public Sprite OffSprite;

    SoundManager soundManager;

    public override void Onit()
    {
        soundManager = GameManager.Instance.SoundManager;

        Setting();
    }

    void Setting()
    {
        float NowValue = 0;

        switch (thisSoundEnum)
        {
            case SoundEnum.Main:
                NowValue = soundManager.MainSoundVolume;
                break;
        }

        int count = Mathf.RoundToInt(NowValue * 10f);
        count = Mathf.Clamp(count, 0, ShowValues.Count);

        VolumeValue.text = count.ToString() + "0%";

        for (int i = 0; i < ShowValues.Count; i++)
        {
            if (i < count)
            {
                ShowValues[i].sprite = OnSprite;
                ShowValues[i].color = gradient.Evaluate(i / 10f);
            }
            else
            {
                ShowValues[i].sprite = OffSprite;
                ShowValues[i].color = Color.white;
            }
        }
    }

    public void ClickSoundChangeBtn(float i)
    {
        switch (thisSoundEnum)
        {
            case SoundEnum.Main:
                soundManager.MainSoundVolume += i;
                break;
        }

        soundManager.CheckSounds();
        Setting();
    }
}
