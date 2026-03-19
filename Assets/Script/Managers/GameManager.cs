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

    public GameFlow flow = GameFlow.Start;

    public InputManager InputManager { get; private set; }
    public BuildManager BuildManager { get; private set; }
    public WorldManager WorldManager { get; private set; }
    public MaterialManager MaterialManager { get; private set; }
    public SaveAndLoadManager SaveAndLoadManager { get; private set; }
    public LightManager LightManager { get; private set; }

    public UIManager UIManager;

    public MainCamera MainCamera;

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
        MainCamera = Camera.main.GetComponent<MainCamera>();

        ChangeFlow(GameFlow.WatchAllWorld);
    }

    public void ChangeFlow(GameFlow _flow)
    {
        if (_flow == flow) return;

        flow = _flow;
        switch (flow)
        {
            case GameFlow.WatchAllWorld:
                MainCamera.SubCamera.gameObject.SetActive(false);
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                MainCamera.stopCamera = false;
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting , SelectionUIenum.MainLight, SelectionUIenum.PlayerPos});
                break;

            case GameFlow.PutWorldInSpace:
                MainCamera.SubCamera.gameObject.SetActive(true);
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                MainCamera.stopCamera = false;
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting });
                break;

            case GameFlow.WatchOneWorld:
                MainCamera.SubCamera.gameObject.SetActive(true);
                InputManager.WhenComeintoWatchOneWorld();
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.Color, SelectionUIenum.PutWorld });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);
                break;

            case GameFlow.MakeOneWorld:
                MainCamera.SubCamera.gameObject.SetActive(true);
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.ChooseHowToMake });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.ChooseHowToMake);
                break;

            case GameFlow.SelectOneWorld:
                MainCamera.SubCamera.gameObject.SetActive(false);
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.LightOneWorld, SelectionUIenum.MoveOneWorld, SelectionUIenum.WorldSetting });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);
                break;
        }
    }
}
