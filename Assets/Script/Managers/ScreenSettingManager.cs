using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ScreenResolutionData
{
    public int Width;
    public int Height;

    public ScreenResolutionData(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return Width + " x " + Height;
    }
}

public enum ScreenModeType
{
    Windowed,
    Fullscreen
}

public class ScreenSettingManager : MonoBehaviour
{
    [Header("Resolution")]
    public List<ScreenResolutionData> Resolutions =
        new List<ScreenResolutionData>();

    [SerializeField] private int currentResolutionIndex = 2;

    [Header("Screen Mode")]
    [SerializeField] private ScreenModeType currentScreenMode =
        ScreenModeType.Windowed;

    public int CurrentResolutionIndex => currentResolutionIndex;
    public ScreenModeType CurrentScreenMode => currentScreenMode;

    public int CurrentWidth
    {
        get
        {
            return GetCurrentResolution().Width;
        }
    }

    public int CurrentHeight
    {
        get
        {
            return GetCurrentResolution().Height;
        }
    }

    private void Awake()
    {
        MakeDefaultResolutionList();
        ClampResolutionIndex();
    }

    private void Start()
    {
        LoadOrCreateScreenSetting();
    }

    void MakeDefaultResolutionList()
    {
        if (Resolutions.Count > 0)
            return;

        Resolutions.Add(new ScreenResolutionData(1280, 720));
        Resolutions.Add(new ScreenResolutionData(1600, 900));
        Resolutions.Add(new ScreenResolutionData(1920, 1080));
        Resolutions.Add(new ScreenResolutionData(2560, 1440));
        Resolutions.Add(new ScreenResolutionData(3840, 2160));
    }

    void ClampResolutionIndex()
    {
        if (Resolutions.Count == 0)
        {
            currentResolutionIndex = 0;
            return;
        }

        currentResolutionIndex =
            Mathf.Clamp(
                currentResolutionIndex,
                0,
                Resolutions.Count - 1
            );
    }

    public ScreenResolutionData GetCurrentResolution()
    {
        MakeDefaultResolutionList();
        ClampResolutionIndex();

        return Resolutions[currentResolutionIndex];
    }

    public ScreenResolutionData GetResolution(int index)
    {
        MakeDefaultResolutionList();

        index =
            Mathf.Clamp(
                index,
                0,
                Resolutions.Count - 1
            );

        return Resolutions[index];
    }

    public void SetResolutionIndex(int index)
    {
        MakeDefaultResolutionList();

        currentResolutionIndex =
            Mathf.Clamp(
                index,
                0,
                Resolutions.Count - 1
            );

        ApplyScreenSetting();
    }

    public void SetWindowed()
    {
        currentScreenMode = ScreenModeType.Windowed;
        ApplyScreenSetting();
    }

    public void SetWindowedResolution(int index)
    {
        MakeDefaultResolutionList();

        currentResolutionIndex =
            Mathf.Clamp(
                index,
                0,
                Resolutions.Count - 1
            );

        currentScreenMode = ScreenModeType.Windowed;

        ApplyScreenSetting();
    }

    public void SetFullscreen()
    {
        currentScreenMode = ScreenModeType.Fullscreen;
        ApplyScreenSetting();
    }

    public void SetScreenMode(ScreenModeType mode)
    {
        currentScreenMode = mode;
        ApplyScreenSetting();
    }

    public void ApplyScreenSetting()
    {
        ApplyScreenSetting(true);
        SaveScreenSetting();
    }

    void ApplyScreenSetting(bool skipSave)
    {
        ScreenResolutionData resolution =
            GetCurrentResolution();

        FullScreenMode mode =
            currentScreenMode == ScreenModeType.Fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            resolution.Width,
            resolution.Height,
            mode
        );

        if (!skipSave)
        {
            SaveScreenSetting();
        }
    }

    void LoadOrCreateScreenSetting()
    {
        bool loaded =
            GameManager.Instance
                .SaveAndLoadManager
                .PlayerSetting_LoadScreen(this);

        if (loaded)
        {
            ApplyScreenSetting(true);
            return;
        }

        currentResolutionIndex = 2;
        currentScreenMode = ScreenModeType.Windowed;

        ApplyScreenSetting(true);
        SaveScreenSetting();
    }

    void SaveScreenSetting()
    {
        GameManager.Instance
            .SaveAndLoadManager
            .PlayerSetting_SaveScreen(this);
    }

    public void LoadScreenSetting(
        int resolutionIndex,
        ScreenModeType screenMode
    )
    {
        MakeDefaultResolutionList();

        currentResolutionIndex =
            Mathf.Clamp(
                resolutionIndex,
                0,
                Resolutions.Count - 1
            );

        currentScreenMode = screenMode;
    }
}
