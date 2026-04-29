using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public float MainSoundVolume = 1.0f;

    public void CheckSounds()
    {
        MainSoundVolume = Mathf.Max(0f, MainSoundVolume);
        MainSoundVolume = Mathf.Min(1f, MainSoundVolume);
    }
}
