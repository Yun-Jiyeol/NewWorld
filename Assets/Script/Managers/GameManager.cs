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

    public UIManager UIManager;

    public MainCamera MainCamera;

    public CursorMover CursorMover;

    public string NowWorldSaveFolder = null;

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

        switch (flow)
        {
            case GameFlow.Start:
                BuildManager.maxFrameTime = 5f;
                Cursor.lockState = CursorLockMode.Confined;

                MainCamera.SubCamera.gameObject.SetActive(true);
                CursorMover?.ChangeCursorState(cursorstate.Follow);

                RemoveInTimeSave();
                break;

            case GameFlow.WatchAllWorld:
                InputManager.nowMovingThing = CoinType.Player;

                MainCamera.SubCamera.gameObject.SetActive(false);
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
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.CameraSetting });

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
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.ChooseHowToMake,SelectionUIenum.HelpingInMake });
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
    }

    public void StartGame()
    {
        MainCamera.TryCheckrot();
        UIManager.Onit();

        ChangeFlow(GameFlow.WatchAllWorld);
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
        SaveAndLoadManager.SaveAll();
        ChangeFlow(GameFlow.Start);
    }
}
