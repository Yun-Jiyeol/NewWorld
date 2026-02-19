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
        SaveAndLoadManager.SaveAllWorld();
        if (_flow == flow) return;

        flow = _flow;
        switch (flow)
        {
            case GameFlow.WatchAllWorld:
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                MainCamera.stopCamera = false;
                UIManager.OffThePreviewWorld();
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting });
                break;

            case GameFlow.PutWorldInSpace:
                BuildManager.maxFrameTime = 1f;
                InputManager.SettingWhenChangeToPutWorld();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                MainCamera.stopCamera = false;
                UIManager.OnThePreviewWorld();
                UIManager.Compass.SetActive(true);
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting });
                break;

            case GameFlow.WatchOneWorld:
                BuildManager.maxFrameTime = 10f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.OffThePreviewWorld();
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.Color});
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);
                break;

            case GameFlow.MakeOneWorld:
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.OffThePreviewWorld();
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.ChooseHowToMake });
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.ChooseHowToMake);
                break;

            case GameFlow.SelectOneWorld:
                BuildManager.maxFrameTime = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                UIManager.Compass.SetActive(false);
                UIManager.OffThePreviewWorld();
                UIManager.SelectionUI.ChooseToOnUIBtn(new List<SelectionUIenum> { SelectionUIenum.Setting, SelectionUIenum.LightOneWorld , SelectionUIenum.MoveOneWorld});
                UIManager.SelectionUI.ChangeNowUIEnum(SelectionUIenum.None);
                break;
        }
    }
}
