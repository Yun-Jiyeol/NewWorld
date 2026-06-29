using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEnum
{
    Main,
    Effect,
    Music
}

public class SoundPage : NeedToDoOnit
{
    public SoundControl MainSoundControl;
    public SoundControl EffectSoundControl;
    public SoundControl BGMSoundControl;

    SoundManager soundManager;

    public override void Onit()
    {
        soundManager = GameManager.Instance.SoundManager;

        MainSoundControl.Onit();
        EffectSoundControl.Onit();
        BGMSoundControl.Onit();
    }
}
