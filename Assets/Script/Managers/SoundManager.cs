using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0f, 1f)] public float MainSoundVolume = 1.0f;
    [Range(0f, 1f)] public float EffectVolume = 1f;
    [Range(0f, 1f)] public float BGMVolume = 1f;

    [Header("Effect Source Pool")]
    [SerializeField] private int startEffectSourceCount = 10;
    [SerializeField] private int maxEffectSourceCount = 30;

    private Transform effectParent;
    private Transform bgmParent;
    private Transform loopParent;

    private AudioSource bgmSource;

    private readonly List<AudioSource> effectSources = new List<AudioSource>();
    private readonly Dictionary<AudioSource, Coroutine> effectReturnRoutines = new Dictionary<AudioSource, Coroutine>();
    private readonly HashSet<AudioSource> ignoreVolumeControlSources = new HashSet<AudioSource>();

    private readonly Dictionary<string, AudioSource> loopSoundSources = new Dictionary<string, AudioSource>(); 

    private void Awake()
    {
        CreateParents();
        CreateAudioSources();
    }

    private void Start()
    {
        PlayEffect(SoundChoose.FindSound("PowerOn"));
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

        GameObject loopObj = new GameObject("LoopSounds");
        loopObj.transform.SetParent(transform);
        loopObj.transform.localPosition = Vector3.zero;
        loopParent = loopObj.transform;
    }

    private void CreateAudioSources()
    {
        for (int i = 0; i < startEffectSourceCount; i++)
        {
            CreateEffectSource();
        }

        GameObject bgmObj = new GameObject("BGMSource");
        bgmObj.transform.SetParent(bgmParent);
        bgmObj.transform.localPosition = Vector3.zero;

        bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    private AudioSource CreateEffectSource()
    {
        GameObject effectObj = new GameObject("EffectSource_" + effectSources.Count);
        effectObj.transform.SetParent(effectParent);
        effectObj.transform.localPosition = Vector3.zero;

        AudioSource source = effectObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.clip = null;
        source.pitch = 1f;
        source.volume = 1f;

        effectSources.Add(source);
        return source;
    }

    private AudioSource GetFreeEffectSource()
    {
        for (int i = 0; i < effectSources.Count; i++)
        {
            if (!effectSources[i].isPlaying)
            {
                return effectSources[i];
            }
        }

        if (effectSources.Count < maxEffectSourceCount)
        {
            return CreateEffectSource();
        }

        return GetOldestOrFirstEffectSource();
    }

    private AudioSource GetOldestOrFirstEffectSource()
    {
        if (effectSources.Count <= 0)
            return CreateEffectSource();

        return effectSources[0];
    }

    // ==============================
    // Effect Sound - AudioClip 방식
    // ==============================

    public void PlayEffect(AudioClip clip)
    {
        PlayEffect(clip, 1f, 1f);
    }

    public void PlayEffect(AudioClip clip, float volume)
    {
        PlayEffect(clip, volume, 1f);
    }

    public void PlayEffect(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;

        float finalVolume = GetFinalEffectVolume() * Mathf.Clamp01(volume);
        float finalPitch = Mathf.Max(0.01f, pitch);

        PlayEffectWithPooledSource(clip, finalVolume, finalPitch);
    }

    // ==============================
    // Effect Sound - SoundData 방식
    // ==============================

    public void PlayEffect(SoundData soundData)
    {
        PlayEffect(soundData, 1f, 1f, 1f);
    }

    public void PlayEffect(SoundData soundData, float volumeMultiplier)
    {
        PlayEffect(soundData, volumeMultiplier, 1f, 1f);
    }

    public void PlayEffect(SoundData soundData, float volumeMultiplier, float pitchMultiplier)
    {
        PlayEffect(soundData, volumeMultiplier, pitchMultiplier, 1f);
    }

    public void PlayEffect(
        SoundData soundData,
        float volumeMultiplier,
        float pitchMultiplier,
        float speedMultiplier
    )
    {
        if (soundData == null) return;
        if (!soundData.IsValid()) return;

        float finalVolume =
            GetFinalEffectVolume() *
            Mathf.Clamp01(soundData.Volume) *
            Mathf.Clamp01(volumeMultiplier);

        float finalPitch =
            soundData.GetPitch() *
            soundData.GetSpeed() *
            Mathf.Max(0.01f, pitchMultiplier) *
            Mathf.Max(0.01f, speedMultiplier);

        finalPitch = Mathf.Max(0.01f, finalPitch);

        PlayEffectWithPooledSource(soundData.Clip, finalVolume, finalPitch);
    }

    private void PlayEffectWithPooledSource(AudioClip clip, float volume, float pitch)
    {
        PlayEffectWithPooledSource(clip, volume, pitch, false);
    }

    private void PlayEffectWithPooledSource(AudioClip clip, float volume, float pitch, bool ignoreVolumeControl)
    {
        if (clip == null) return;

        AudioSource source = GetFreeEffectSource();

        if (effectReturnRoutines.TryGetValue(source, out Coroutine oldRoutine))
        {
            if (oldRoutine != null)
                StopCoroutine(oldRoutine);

            effectReturnRoutines.Remove(source);
        }

        ignoreVolumeControlSources.Remove(source);

        if (ignoreVolumeControl)
        {
            ignoreVolumeControlSources.Add(source);
        }

        source.Stop();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Max(0.01f, pitch);
        source.loop = false;
        source.Play();

        float returnTime = clip.length / Mathf.Max(0.01f, pitch);
        Coroutine routine = StartCoroutine(ReturnEffectSourceAfterPlay(source, returnTime));
        effectReturnRoutines[source] = routine;
    }

    private IEnumerator ReturnEffectSourceAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.05f);

        if (source != null)
        {
            source.Stop();
            source.clip = null;
            source.pitch = 1f;
            source.volume = 1f;
            source.loop = false;

            ignoreVolumeControlSources.Remove(source);
        }

        if (source != null && effectReturnRoutines.ContainsKey(source))
        {
            effectReturnRoutines.Remove(source);
        }
    }

    public void PlayEffectIgnoreVolume(SoundData soundData, float volume)
    {
        if (soundData == null) return;
        if (!soundData.IsValid()) return;

        float finalVolume =
            Mathf.Clamp01(soundData.Volume) *
            Mathf.Clamp01(volume);

        float finalPitch =
            soundData.GetPitch() *
            soundData.GetSpeed();

        finalPitch = Mathf.Max(0.01f, finalPitch);

        PlayEffectWithPooledSource(soundData.Clip, finalVolume, finalPitch, true);
    }

    // ==============================
    // Loop Sound - SoundData 반복 재생
    // ==============================

    public void ToggleLoopSound(SoundData soundData)
    {
        if (soundData == null) return;

        ToggleLoopSound(soundData.name, soundData);
    }

    public void ToggleLoopSound(SoundData soundData, bool isPlay)
    {
        if (soundData == null) return;

        ToggleLoopSound(soundData.name, soundData, isPlay);
    }

    public void ToggleLoopSound(string key, SoundData soundData)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (IsLoopSoundPlaying(key))
        {
            StopLoopSound(key);
        }
        else
        {
            PlayLoopSound(key, soundData);
        }
    }

    public void ToggleLoopSound(string key, SoundData soundData, bool isPlay)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (isPlay)
        {
            PlayLoopSound(key, soundData);
        }
        else
        {
            StopLoopSound(key);
        }
    }

    public void ToggleLoopSound(
        string key,
        SoundData soundData,
        bool isPlay,
        float volumeMultiplier,
        float pitchMultiplier,
        float speedMultiplier
    )
    {
        if (string.IsNullOrEmpty(key)) return;

        if (isPlay)
        {
            PlayLoopSound(key, soundData, volumeMultiplier, pitchMultiplier, speedMultiplier);
        }
        else
        {
            StopLoopSound(key);
        }
    }

    public void PlayLoopSound(SoundData soundData)
    {
        if (soundData == null) return;

        PlayLoopSound(soundData.name, soundData, 1f, 1f, 1f);
    }

    public void PlayLoopSound(string key, SoundData soundData)
    {
        PlayLoopSound(key, soundData, 1f, 1f, 1f);
    }

    public void PlayLoopSound(
        string key,
        SoundData soundData,
        float volumeMultiplier,
        float pitchMultiplier,
        float speedMultiplier
    )
    {
        if (string.IsNullOrEmpty(key)) return;
        if (soundData == null) return;
        if (!soundData.IsValid()) return;

        if (loopSoundSources.ContainsKey(key))
        {
            StopLoopSound(key);
        }

        GameObject loopObj = new GameObject("LoopSound_" + key);
        loopObj.transform.SetParent(loopParent);
        loopObj.transform.localPosition = Vector3.zero;

        AudioSource source = loopObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.clip = soundData.Clip;

        float finalVolume =
            GetFinalEffectVolume() *
            Mathf.Clamp01(soundData.Volume) *
            Mathf.Clamp01(volumeMultiplier);

        float finalPitch =
            soundData.GetPitch() *
            soundData.GetSpeed() *
            Mathf.Max(0.01f, pitchMultiplier) *
            Mathf.Max(0.01f, speedMultiplier);

        source.volume = finalVolume;
        source.pitch = Mathf.Max(0.01f, finalPitch);

        loopSoundSources.Add(key, source);

        source.Play();
    }

    public void StopLoopSound(SoundData soundData)
    {
        if (soundData == null) return;

        StopLoopSound(soundData.name);
    }

    public void StopLoopSound(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (!loopSoundSources.TryGetValue(key, out AudioSource source))
            return;

        loopSoundSources.Remove(key);

        if (source != null)
        {
            GameObject obj = source.gameObject;
            source.Stop();
            source.clip = null;
            Destroy(obj);
        }
    }

    public bool IsLoopSoundPlaying(SoundData soundData)
    {
        if (soundData == null) return false;

        return IsLoopSoundPlaying(soundData.name);
    }

    public bool IsLoopSoundPlaying(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        if (!loopSoundSources.TryGetValue(key, out AudioSource source))
            return false;

        if (source == null)
        {
            loopSoundSources.Remove(key);
            return false;
        }

        return source.isPlaying;
    }

    public void StopAllLoopSounds()
    {
        List<string> keys = new List<string>(loopSoundSources.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            StopLoopSound(keys[i]);
        }

        loopSoundSources.Clear();
    }

    // ==============================
    // BGM Sound
    // ==============================

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.volume = GetFinalBGMVolume();
        bgmSource.pitch = 1f;
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
        ApplyVolumes();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private float GetFinalEffectVolume()
    {
        return Mathf.Clamp01(MainSoundVolume * EffectVolume);
    }

    private float GetFinalBGMVolume()
    {
        return Mathf.Clamp01(MainSoundVolume * BGMVolume);
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = GetFinalBGMVolume();
        }

        for (int i = 0; i < effectSources.Count; i++)
        {
            AudioSource source = effectSources[i];

            if (source == null)
                continue;

            if (ignoreVolumeControlSources.Contains(source))
                continue;

            if (source.isPlaying)
            {
                source.volume = Mathf.Min(source.volume, GetFinalEffectVolume());
            }
        }

        foreach (KeyValuePair<string, AudioSource> pair in loopSoundSources)
        {
            AudioSource source = pair.Value;

            if (source == null)
                continue;

            if (source.isPlaying)
            {
                source.volume = Mathf.Min(source.volume, GetFinalEffectVolume());
            }
        }
    }
}