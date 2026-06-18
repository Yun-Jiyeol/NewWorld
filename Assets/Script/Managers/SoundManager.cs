using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0f, 1f)] public float MainSoundVolume = 1.0f;
    [Range(0f, 1f)] public float EffectVolume = 1f;
    [Range(0f, 1f)] public float BGMVolume = 1f;

    private Transform effectParent;
    private Transform bgmParent;

    private AudioSource effectSource;
    private AudioSource bgmSource;

    private void Awake()
    {
        CreateParents();
        CreateAudioSources();
    }

    private void CreateParents()
    {
        GameObject effectObj = new GameObject("EffectSounds");
        effectObj.transform.SetParent(transform);
        effectObj.transform.localPosition = Vector3.zero;
        effectParent = effectObj.transform;

        GameObject bgmObj = new GameObject("BGMSounds");
        bgmObj.transform.SetParent(transform);
        bgmObj.transform.localPosition = Vector3.zero;
        bgmParent = bgmObj.transform;
    }

    private void CreateAudioSources()
    {
        GameObject effectObj = new GameObject("EffectSource");
        effectObj.transform.SetParent(effectParent);
        effectObj.transform.localPosition = Vector3.zero;

        effectSource = effectObj.AddComponent<AudioSource>();
        effectSource.playOnAwake = false;
        effectSource.loop = false;

        GameObject bgmObj = new GameObject("BGMSource");
        bgmObj.transform.SetParent(bgmParent);
        bgmObj.transform.localPosition = Vector3.zero;

        bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    // ==============================
    // Effect Sound
    // ==============================

    public void PlayEffect(AudioClip clip)
    {
        if (clip == null) return;
        if (effectSource == null) return;

        effectSource.pitch = 1f;
        effectSource.PlayOneShot(clip, GetFinalEffectVolume());
    }

    public void PlayEffect(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (effectSource == null) return;

        effectSource.pitch = 1f;
        effectSource.PlayOneShot(clip, GetFinalEffectVolume() * volume);
    }

    public void PlayEffect(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;
        if (effectSource == null) return;

        effectSource.pitch = pitch;
        effectSource.PlayOneShot(clip, GetFinalEffectVolume() * volume);
    }

    // ==============================
    // BGM Sound
    // ==============================

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.volume = GetFinalBGMVolume();
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PauseBGM()
    {
        if (bgmSource == null) return;

        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource == null) return;

        bgmSource.UnPause();
    }

    // ==============================
    // Volume
    // ==============================

    public void SetMainVolume(float value)
    {
        MainSoundVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetEffectVolume(float value)
    {
        EffectVolume = Mathf.Clamp01(value);
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private float GetFinalEffectVolume()
    {
        return MainSoundVolume * EffectVolume;
    }

    private float GetFinalBGMVolume()
    {
        return MainSoundVolume * BGMVolume;
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = GetFinalBGMVolume();
        }
    }
}
