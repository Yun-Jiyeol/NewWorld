using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameFlow
{
    None,
    WatchAllWorld,
    WatchOneWorld,
    Start,
    PutWorldInSpace,
    MakeOneWorld,
    SelectOneWorld
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameFlow flow = GameFlow.None;

    public InputManager InputManager { get; private set; }
    public BuildManager BuildManager { get; private set; }
    public WorldManager WorldManager { get; private set; }
    public MaterialManager MaterialManager { get; private set; }
    public SaveAndLoadManager SaveAndLoadManager { get; private set; }
    public LightManager LightManager { get; private set; }
    public OnStartOrStopManager OnStartOrStopManager { get; private set; }
    public SoundManager SoundManager { get; private set; }
    public TimerManager TimerManager { get; private set; }
    public KeyManager KeyManager { get; private set; }
    public TranslationManager TranslationManager { get; private set; }
    public ScreenSettingManager ScreenSettingManager { get; private set; }

    public UIManager UIManager;

    public MainCamera MainCamera;

    public CursorMover CursorMover;

    public StartAdditionalUI StartAdditionalUI;

    public string NowWorldSaveFolder = null;

    Coroutine playerPosTraceCoroutine;
    bool isReturningToStart = false;

    public void DebugPlayerPosTrace(string stage)
    {
        Transform mainTransform =
            MainCamera != null ? MainCamera.transform : null;

        GameObject subObject =
            MainCamera != null ? MainCamera.SubCamera : null;

        Camera mainCameraComponent =
            MainCamera != null ? MainCamera.GetComponent<Camera>() : null;

        Camera subCameraComponent =
            subObject != null ? subObject.GetComponent<Camera>() : null;

        string mainInfo = "null";
        string subInfo = "null";

        if (mainTransform != null)
        {
            mainInfo =
                "pos=" + mainTransform.position.ToString("F3") +
                " rot=" + mainTransform.eulerAngles.ToString("F2") +
                " active=" + mainTransform.gameObject.activeInHierarchy +
                " cameraEnabled=" +
                (mainCameraComponent != null && mainCameraComponent.enabled);
        }

        if (subObject != null)
        {
            Transform subTransform = subObject.transform;

            subInfo =
                "pos=" + subTransform.position.ToString("F3") +
                " localPos=" + subTransform.localPosition.ToString("F3") +
                " rot=" + subTransform.eulerAngles.ToString("F2") +
                " localRot=" + subTransform.localEulerAngles.ToString("F2") +
                " activeSelf=" + subObject.activeSelf +
                " activeHierarchy=" + subObject.activeInHierarchy +
                " cameraEnabled=" +
                (subCameraComponent != null && subCameraComponent.enabled);
        }

        Camera taggedMainCamera = Camera.main;
        string taggedMainName =
            taggedMainCamera != null ? taggedMainCamera.gameObject.name : "null";

        Debug.Log(
            "[PlayerPosTrace] stage=" + stage +
            " frame=" + Time.frameCount +
            " time=" + Time.realtimeSinceStartup.ToString("F3") +
            " flow=" + flow +
            " Main{" + mainInfo + "}" +
            " Sub{" + subInfo + "}" +
            " Camera.main=" + taggedMainName
        );
    }

    public void StartPlayerPosDebugWatch(float duration = 6f)
    {
        if (playerPosTraceCoroutine != null)
            return;

        playerPosTraceCoroutine =
            StartCoroutine(PlayerPosDebugWatch(duration));
    }

    IEnumerator PlayerPosDebugWatch(float duration)
    {
        if (MainCamera == null)
        {
            playerPosTraceCoroutine = null;
            yield break;
        }

        float startTime = Time.realtimeSinceStartup;

        Vector3 lastMainPos = MainCamera.transform.position;
        Quaternion lastMainRot = MainCamera.transform.rotation;

        bool lastSubActive =
            MainCamera.SubCamera != null &&
            MainCamera.SubCamera.activeInHierarchy;

        DebugPlayerPosTrace("Watch.Begin");

        while (Time.realtimeSinceStartup - startTime < duration)
        {
            yield return null;

            if (MainCamera == null)
                break;

            Vector3 nowMainPos = MainCamera.transform.position;
            Quaternion nowMainRot = MainCamera.transform.rotation;

            bool nowSubActive =
                MainCamera.SubCamera != null &&
                MainCamera.SubCamera.activeInHierarchy;

            float positionDelta =
                Vector3.Distance(lastMainPos, nowMainPos);

            float rotationDelta =
                Quaternion.Angle(lastMainRot, nowMainRot);

            if (
                positionDelta > 0.0001f ||
                rotationDelta > 0.01f ||
                nowSubActive != lastSubActive
            )
            {
                Debug.Log(
                    "[PlayerPosTrace] Watch.Change" +
                    " frame=" + Time.frameCount +
                    " posDelta=" + positionDelta.ToString("F4") +
                    " rotDelta=" + rotationDelta.ToString("F3") +
                    " subActiveChanged=" +
                    (nowSubActive != lastSubActive)
                );

                DebugPlayerPosTrace("Watch.Change.Detail");

                lastMainPos = nowMainPos;
                lastMainRot = nowMainRot;
                lastSubActive = nowSubActive;
            }
        }

        DebugPlayerPosTrace("Watch.End");
        playerPosTraceCoroutine = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputManager = gameObject.GetComponent<InputManager>();
        BuildManager = gameObject.GetComponent<BuildManager>();
        WorldManager = gameObject.GetComponent<WorldManager>();
        MaterialManager = gameObject.GetComponent<MaterialManager>();
        SaveAndLoadManager = gameObject.GetComponent<SaveAndLoadManager>();
        LightManager = gameObject.GetComponent<LightManager>();
        OnStartOrStopManager = gameObject.GetComponent<OnStartOrStopManager>();
        SoundManager = gameObject.GetComponent<SoundManager>();
        TimerManager = gameObject.GetComponent<TimerManager>();
        KeyManager = gameObject.GetComponent<KeyManager>();
        ScreenSettingManager = gameObject.GetComponent<ScreenSettingManager>();
        TranslationManager = gameObject.GetComponent<TranslationManager>();
        MainCamera = Camera.main.GetComponent<MainCamera>();
    }

    private void Start()
    {
        Cursor.visible = false;
        NowWorldSaveFolder = null;

        ChangeFlow(GameFlow.Start);
    }

    public void ChangeFlow(GameFlow _flow)
    {
        if (_flow == flow) return;

        DebugPlayerPosTrace(
            "ChangeFlow.Before." + flow + "To" + _flow
        );

        if (MainCamera != null &&
            flow == GameFlow.WatchAllWorld &&
            (_flow == GameFlow.SelectOneWorld ||
             _flow == GameFlow.MakeOneWorld))
        {
            MainCamera.CaptureEditReturnPose();
        }

        if (MainCamera != null &&
            _flow == GameFlow.WatchAllWorld &&
            flow != GameFlow.WatchAllWorld)
        {
            MainCamera.ClearEditReturnPose();
        }

        flow = _flow;
        if (flow == GameFlow.Start)
        {
            OnStartOrStopManager.DelectActions();
            UIManager.ChangeScene(SceneSelection.StartScene);

            MainCamera.SubCamera.gameObject.SetActive(true);
            MainCamera.SubCamera.GetComponent<Camera>().cullingMask = ~0;
        }
        else
        {
            OnStartOrStopManager.AddActions();
            UIManager.ChangeScene(SceneSelection.InGameUI);

            MainCamera.SubCamera.GetComponent<Camera>().cullingMask = 1 << 7;
        }

        DebugPlayerPosTrace("ChangeFlow.AfterSceneSetup." + flow);

        switch (flow)
        {
            case GameFlow.Start:
                BuildManager.maxFrameTime = 5f;
                Cursor.lockState = CursorLockMode.Confined;

                if (UIManager != null &&
                    UIManager.WorldSeenUI != null)
                {
                    UIManager.WorldSeenUI.ResetForStartScreen();
                }

                MainCamera.stopCamera = true;
                MainCamera.ClearEditReturnPose();
                MainCamera.IgnoreMouseInputForFrames(2);

                Camera mainCameraComponent =
                    MainCamera.GetComponent<Camera>();

                if (mainCameraComponent != null)
                {
                    mainCameraComponent.cullingMask &= ~(1 << 7);
                }

                MainCamera.SubCamera.gameObject.SetActive(true);

                Camera subCameraComponent =
                    MainCamera.SubCamera.GetComponent<Camera>();

                if (subCameraComponent != null)
                {
                    subCameraComponent.cullingMask = ~0;
                }

                CameraShaderPutter subCameraShader =
                    MainCamera.SubCamera.GetComponent<CameraShaderPutter>();

                if (subCameraShader != null)
                {
                    subCameraShader.isUsingEffect = true;
                }

                if (LightManager != null)
                {
                    if (LightManager.MainLight != null)
                    {
                        Light mainLight =
                            LightManager.MainLight.GetComponent<Light>();

                        if (mainLight != null)
                            mainLight.cullingMask = ~0;
                    }

                    if (LightManager.SubLight != null)
                        LightManager.SubLight.SetActive(false);
                }

                CursorMover?.ChangeCursorState(cursorstate.Follow);
                CursorMover?.MouseSpriteSetting(false);

                if (UIManager != null)
                {
                    UIManager.isReady = false;

                    if (UIManager.Compass != null)
                        UIManager.Compass.SetActive(false);
                }

                RemoveInTimeSave();
                break;

            case GameFlow.WatchAllWorld:
                InputManager.nowMovingThing = CoinType.Player;

                DebugPlayerPosTrace("WatchAllWorld.BeforeSubCameraOff");
                MainCamera.SubCamera.gameObject.SetActive(false);
                DebugPlayerPosTrace("WatchAllWorld.AfterSubCameraOff");

                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Locked;
                CursorMover?.ChangeCursorState(cursorstate.Off);
                MainCamera.stopCamera = false;
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting , SelectionUIenum.MainLight, SelectionUIenum.PlayerPos, SelectionUIenum.CameraSetting });

                AddInTimeSave(SavePlayer);
                break;

            case GameFlow.PutWorldInSpace:
                InputManager.nowMovingThing = CoinType.Player;

                MainCamera.SubCamera.gameObject.SetActive(true);
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                CursorMover?.ChangeCursorState(cursorstate.Off);
                MainCamera.stopCamera = false;
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.CameraSetting, SelectionUIenum.SetWorldPosition});

                AddInTimeSave(SaveNowWorld);
                break;

            case GameFlow.WatchOneWorld:
                InputManager.nowMovingThing = CoinType.World;

                MainCamera.SubCamera.gameObject.SetActive(true);
                InputManager.WhenComeintoWatchOneWorld();
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Confined;
                CursorMover?.ChangeCursorState(cursorstate.Follow);
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.Color, SelectionUIenum.PutWorld, SelectionUIenum.HelpingInMake, SelectionUIenum.CameraSetting });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);

                AddInTimeSave(SaveNowWorld);
                break;

            case GameFlow.MakeOneWorld:
                InputManager.nowMovingThing = CoinType.World;

                MainCamera.SubCamera.gameObject.SetActive(true);
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Confined;
                CursorMover?.ChangeCursorState(cursorstate.Follow);
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.ChooseHowToMake,SelectionUIenum.HelpingInMake, SelectionUIenum.CameraSetting });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.ChooseHowToMake);

                AddInTimeSave(SaveNowWorld);
                break;

            case GameFlow.SelectOneWorld:
                InputManager.nowMovingThing = CoinType.None;

                MainCamera.SubCamera.gameObject.SetActive(false);
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                CursorMover?.ChangeCursorState(cursorstate.Follow);
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.LightOneWorld, SelectionUIenum.MoveOneWorld, SelectionUIenum.WorldSetting, SelectionUIenum.CameraSetting });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);

                AddInTimeSave(SaveSomeNowWorld);
                break;
        }

        DebugPlayerPosTrace("ChangeFlow.End." + flow);
    }

    public void StartGame()
    {
        StartPlayerPosDebugWatch(6f);

        DebugPlayerPosTrace("StartGame.BeforeTryCheckrot");
        MainCamera.TryCheckrot();
        DebugPlayerPosTrace("StartGame.AfterTryCheckrot");

        UIManager.Onit();
        DebugPlayerPosTrace("StartGame.AfterUIOnit");

        CursorMover?.MouseSpriteSetting(true);

        DebugPlayerPosTrace("StartGame.BeforeWatchAllWorld");
        ChangeFlow(GameFlow.WatchAllWorld);
        DebugPlayerPosTrace("StartGame.AfterWatchAllWorld");
    }

    public void SaveNowWorld()
    {
        if(InputManager.TargetGameObject == null) return;
        SaveAndLoadManager.SaveWorldData(InputManager.TargetGameObject.GetComponent<ChunkCode>());
    }

    public void SaveSomeNowWorld()
    {
        if (InputManager.TargetGameObject == null) return;
        SaveAndLoadManager.SaveWorldInfo(InputManager.TargetGameObject.GetComponent<ChunkCode>());
    }

    public void SavePlayer()
    {
        SaveAndLoadManager.SaveAll();
    }

    void AddInTimeSave(Action _action)
    {
        OnStartOrStopManager.AddTimeActions(_action);
    }

    void RemoveInTimeSave()
    {
        OnStartOrStopManager.StopTimeCoroutine();
    }

    public void ExitGame()
    {
        SaveAndLoadManager.SaveAll();

        if (OnStartOrStopManager != null)
            OnStartOrStopManager.DelectActions();

        Exit();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToStartPage()
    {
        if (isReturningToStart)
            return;

        StartCoroutine(ReturnToStartPageCoroutine());
    }

    IEnumerator ReturnToStartPageCoroutine()
    {
        isReturningToStart = true;
        RemoveInTimeSave();

        bool wasEditingWorld =
            InputManager != null &&
            InputManager.IsWorldEditActive();

        if (wasEditingWorld)
        {
            yield return StartCoroutine(
                InputManager.FinalizeWorldBeforeReturnToStart()
            );
        }
        else if (InputManager != null)
        {
            InputManager.ResetForStartScreenState();
        }

        if (wasEditingWorld && MainCamera != null)
        {
            MainCamera.RestoreEditReturnPose();
        }

        if (SaveAndLoadManager != null)
        {
            SaveAndLoadManager.SaveAll();
        }

        if (MainCamera != null)
        {
            MainCamera.ResetForStartScreen();
        }

        ChangeFlow(GameFlow.Start);
        isReturningToStart = false;
    }
}
