using UnityEngine;

[CreateAssetMenu(fileName = "TranslationTable", menuName = "Translation/Translation Table")]
public class TranslationData : ScriptableObject
{
    public SaveString Strings;
}

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Game/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Audio Clip")]
    public AudioClip Clip;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float Volume = 1f;

    [Header("Pitch")]
    [Range(0.1f, 3f)]
    public float Pitch = 1f;

    [Header("Random Pitch")]
    public bool UseRandomPitch = false;

    [Range(0.1f, 3f)]
    public float MinPitch = 0.95f;

    [Range(0.1f, 3f)]
    public float MaxPitch = 1.05f;

    public float GetPitch()
    {
        if (!UseRandomPitch)
            return Pitch;

        return Random.Range(MinPitch, MaxPitch);
    }

    public bool IsValid()
    {
        return Clip != null;
    }
}