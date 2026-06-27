
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Game/Sound/Sound Data")]
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

    [Header("Speed")]
    [Range(0.1f, 5f)]
    public float Speed = 1f;

    [Header("Random Pitch")]
    public bool UseRandomPitch = false;

    [Range(0.1f, 3f)]
    public float MinPitch = 0.95f;

    [Range(0.1f, 3f)]
    public float MaxPitch = 1.05f;

    public float GetPitch()
    {
        float pitch;

        if (UseRandomPitch)
        {
            pitch = Random.Range(MinPitch, MaxPitch);
        }
        else
        {
            pitch = Pitch;
        }

        return Mathf.Max(0.01f, pitch);
    }

    public float GetSpeed()
    {
        return Mathf.Max(0.01f, Speed);
    }

    public bool IsValid()
    {
        return Clip != null;
    }
}