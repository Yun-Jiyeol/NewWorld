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

    SoundManager soundManager;

    public override void Onit()
    {
        soundManager = GameManager.Instance.SoundManager;

        MainSoundControl.Onit();
    }
}
