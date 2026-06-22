using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoopSoundType
{
    None,

    CassetteSelect,
    DrawerMove,
    TapeRunning,
    WorldLoading,
    SystemProcessing
}

public static class ForLoopSound
{
    public static AudioClip CallStartSound(LoopSoundType _name)
    {
        switch (_name)
        {
            case LoopSoundType.None:
                return null;

            case LoopSoundType.CassetteSelect:
                return null;
                // return CassetteSelectLoopSoundFactory.CreateCassetteSelect_Start();

            case LoopSoundType.DrawerMove:
                return null;
                // return DrawerLoopSoundFactory.CreateDrawerMove_Start();

            case LoopSoundType.TapeRunning:
                return null;
                // return TapeLoopSoundFactory.CreateTapeRunning_Start();

            case LoopSoundType.WorldLoading:
                return null;
                // return WorldLoadingLoopSoundFactory.CreateWorldLoading_Start();

            case LoopSoundType.SystemProcessing:
                return null;
                // return SystemProcessingLoopSoundFactory.CreateSystemProcessing_Start();
        }

        return null;
    }

    public static AudioClip CallLoopSound(LoopSoundType _name)
    {
        switch (_name)
        {
            case LoopSoundType.None:
                return null;

            case LoopSoundType.CassetteSelect:
                return null;
                // return CassetteSelectLoopSoundFactory.CreateCassetteSelect_Loop();

            case LoopSoundType.DrawerMove:
                return null;
                // return DrawerLoopSoundFactory.CreateDrawerMove_Loop();

            case LoopSoundType.TapeRunning:
                return null;
                // return TapeLoopSoundFactory.CreateTapeRunning_Loop();

            case LoopSoundType.WorldLoading:
                return null;
                // return WorldLoadingLoopSoundFactory.CreateWorldLoading_Loop();

            case LoopSoundType.SystemProcessing:
                return null;
                // return SystemProcessingLoopSoundFactory.CreateSystemProcessing_Loop();
        }

        return null;
    }

    public static AudioClip CallEndSound(LoopSoundType _name)
    {
        switch (_name)
        {
            case LoopSoundType.None:
                return null;

            case LoopSoundType.CassetteSelect:
                return null;
                // return CassetteSelectLoopSoundFactory.CreateCassetteSelect_End();

            case LoopSoundType.DrawerMove:
                return null;
                // return DrawerLoopSoundFactory.CreateDrawerMove_End();

            case LoopSoundType.TapeRunning:
                return null;
                // return TapeLoopSoundFactory.CreateTapeRunning_End();

            case LoopSoundType.WorldLoading:
                return null;
                // return WorldLoadingLoopSoundFactory.CreateWorldLoading_End();

            case LoopSoundType.SystemProcessing:
                return null;
                // return SystemProcessingLoopSoundFactory.CreateSystemProcessing_End();
        }

        return null;
    }
}

public class SoundManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0f, 1f)] public float MainSoundVolume = 1.0f;
    [Range(0f, 1f)] public float EffectVolume = 1f;
    [Range(0f, 1f)] public float BGMVolume = 1f;

    private Transform effectParent;
    private Transform bgmParent;
    private Transform loopParent;

    private AudioSource effectSource;
    private AudioSource bgmSource;

    private readonly Dictionary<string, PlayingLoopSound> playingLoopSounds =
        new Dictionary<string, PlayingLoopSound>();

    private class PlayingLoopSound
    {
        public string Key;
        public LoopSoundType SoundType;

        public AudioClip StartClip;
        public AudioClip LoopClip;
        public AudioClip EndClip;

        public GameObject SourceObject;
        public AudioSource Source;
        public Coroutine Coroutine;

        public float Volume;
        public float Pitch;
    }

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

        GameObject loopObj = new GameObject("LoopSounds");
        loopObj.transform.SetParent(transform);
        loopObj.transform.localPosition = Vector3.zero;
        loopParent = loopObj.transform;
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
        effectSource.PlayOneShot(clip, GetFinalEffectVolume() * Mathf.Clamp01(volume));
    }

    public void PlayEffect(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;
        if (effectSource == null) return;

        effectSource.pitch = Mathf.Max(0.01f, pitch);
        effectSource.PlayOneShot(clip, GetFinalEffectVolume() * Mathf.Clamp01(volume));
    }

    // ==============================
    // Effect Sound - SoundData 방식
    // ==============================

    public void PlayEffect(SoundData soundData)
    {
        PlayEffect(soundData, 1f, 1f);
    }

    public void PlayEffect(SoundData soundData, float volumeMultiplier)
    {
        PlayEffect(soundData, volumeMultiplier, 1f);
    }

    public void PlayEffect(SoundData soundData, float volumeMultiplier, float pitchMultiplier)
    {
        if (soundData == null) return;
        if (!soundData.IsValid()) return;
        if (effectSource == null) return;

        float finalVolume =
            GetFinalEffectVolume() *
            Mathf.Clamp01(soundData.Volume) *
            Mathf.Clamp01(volumeMultiplier);

        float finalPitch =
            Mathf.Max(0.01f, soundData.GetPitch() * pitchMultiplier);

        effectSource.pitch = finalPitch;
        effectSource.PlayOneShot(soundData.Clip, finalVolume);
        effectSource.pitch = 1f;
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
    // Loop Sound
    // string key + enum type
    // Start -> Loop -> End
    // EffectVolume 기준
    // ==============================

    public void PlayNamedLoop(string key, LoopSoundType soundType)
    {
        PlayNamedLoop(key, soundType, 1f, 1f);
    }

    public void PlayNamedLoop(string key, LoopSoundType soundType, float volume)
    {
        PlayNamedLoop(key, soundType, volume, 1f);
    }

    public void PlayNamedLoop(string key, LoopSoundType soundType, float volume, float pitch)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("LoopSound key가 비어있습니다.");
            return;
        }

        if (soundType == LoopSoundType.None)
            return;

        AudioClip startClip = ForLoopSound.CallStartSound(soundType);
        AudioClip loopClip = ForLoopSound.CallLoopSound(soundType);
        AudioClip endClip = ForLoopSound.CallEndSound(soundType);

        if (loopClip == null)
        {
            Debug.LogWarning($"LoopClip이 없습니다: {soundType}");
            return;
        }

        // 같은 이름이면 기존 사운드 제거 후 덮어쓰기
        if (playingLoopSounds.ContainsKey(key))
        {
            StopNamedLoopImmediate(key);
        }

        GameObject sourceObj = new GameObject("LoopSound_" + key + "_" + soundType);
        sourceObj.transform.SetParent(loopParent);
        sourceObj.transform.localPosition = Vector3.zero;

        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;

        PlayingLoopSound playing = new PlayingLoopSound();
        playing.Key = key;
        playing.SoundType = soundType;
        playing.StartClip = startClip;
        playing.LoopClip = loopClip;
        playing.EndClip = endClip;
        playing.SourceObject = sourceObj;
        playing.Source = source;
        playing.Volume = Mathf.Clamp01(volume);
        playing.Pitch = Mathf.Max(0.01f, pitch);

        playingLoopSounds.Add(key, playing);

        playing.Coroutine = StartCoroutine(PlayNamedLoopRoutine(playing));
    }

    private IEnumerator PlayNamedLoopRoutine(PlayingLoopSound playing)
    {
        if (playing == null) yield break;
        if (playing.Source == null) yield break;
        if (playing.LoopClip == null) yield break;

        AudioSource source = playing.Source;

        source.volume = GetFinalLoopVolume(playing.Volume);
        source.pitch = playing.Pitch;
        source.loop = false;

        if (playing.StartClip != null)
        {
            source.clip = playing.StartClip;
            source.Play();

            float waitTime = playing.StartClip.length / playing.Pitch;
            yield return new WaitForSeconds(waitTime);
        }

        if (source == null) yield break;

        source.Stop();
        source.clip = playing.LoopClip;
        source.volume = GetFinalLoopVolume(playing.Volume);
        source.pitch = playing.Pitch;
        source.loop = true;
        source.Play();
    }

    public void StopNamedLoop(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (!playingLoopSounds.TryGetValue(key, out PlayingLoopSound playing))
            return;

        playingLoopSounds.Remove(key);

        if (playing.Coroutine != null)
        {
            StopCoroutine(playing.Coroutine);
            playing.Coroutine = null;
        }

        StartCoroutine(StopNamedLoopRoutine(playing));
    }

    private IEnumerator StopNamedLoopRoutine(PlayingLoopSound playing)
    {
        if (playing == null) yield break;
        if (playing.Source == null) yield break;

        AudioSource source = playing.Source;

        source.Stop();
        source.loop = false;

        if (playing.EndClip != null)
        {
            source.clip = playing.EndClip;
            source.volume = GetFinalLoopVolume(playing.Volume);
            source.pitch = playing.Pitch;
            source.Play();

            float waitTime = playing.EndClip.length / playing.Pitch;
            yield return new WaitForSeconds(waitTime);
        }

        if (playing.SourceObject != null)
        {
            Destroy(playing.SourceObject);
        }
    }

    public void StopNamedLoopImmediate(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (!playingLoopSounds.TryGetValue(key, out PlayingLoopSound playing))
            return;

        playingLoopSounds.Remove(key);

        if (playing.Coroutine != null)
        {
            StopCoroutine(playing.Coroutine);
            playing.Coroutine = null;
        }

        if (playing.Source != null)
        {
            playing.Source.Stop();
        }

        if (playing.SourceObject != null)
        {
            Destroy(playing.SourceObject);
        }
    }

    public bool IsNamedLoopPlaying(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return playingLoopSounds.ContainsKey(key);
    }

    public void StopAllNamedLoops()
    {
        List<string> keys = new List<string>(playingLoopSounds.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            StopNamedLoop(keys[i]);
        }
    }

    public void StopAllNamedLoopsImmediate()
    {
        List<string> keys = new List<string>(playingLoopSounds.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            StopNamedLoopImmediate(keys[i]);
        }
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

    private float GetFinalLoopVolume(float volume)
    {
        return Mathf.Clamp01(MainSoundVolume * EffectVolume * volume);
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = GetFinalBGMVolume();
        }

        ApplyLoopVolumes();
    }

    private void ApplyLoopVolumes()
    {
        foreach (KeyValuePair<string, PlayingLoopSound> pair in playingLoopSounds)
        {
            PlayingLoopSound playing = pair.Value;

            if (playing == null) continue;
            if (playing.Source == null) continue;

            playing.Source.volume = GetFinalLoopVolume(playing.Volume);
        }
    }

    private void OnDestroy()
    {
        StopAllNamedLoopsImmediate();
    }
}