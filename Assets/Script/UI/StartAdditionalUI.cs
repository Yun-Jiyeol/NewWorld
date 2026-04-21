using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartUIs 
{
    MakeNewWorldUI,
    LoadUI,
    SettingUI
}


public class StartAdditionalUI : MonoBehaviour
{
    public GameObject LoadUI;
    public GameObject MakeNewWorldUI;
    public GameObject SettingUI;

    public GameObject NowUI = null;

    public void SpawnUI(StartUIs selectUI)
    {
        if(NowUI != null) Destroy(NowUI);

        GameObject _go = MakeNewWorldUI;
        switch (selectUI)
        {
            case StartUIs.MakeNewWorldUI:
                _go = MakeNewWorldUI;
                break;
            case StartUIs.LoadUI:
                _go = LoadUI;
                break;
            case StartUIs.SettingUI:
                _go = SettingUI;
                break;
        }

        GameObject go = Instantiate(_go, transform);
        NowUI = go;
        go.GetComponent<NeedToDoOnit>().Onit();
    }

    public void OffThisUI()
    {
        Destroy(NowUI);
        NowUI = null;
    }

    public void ClickLoadBtn()
    {
        SpawnUI(StartUIs.LoadUI);
    }

    public void ClickSettingBtn()
    {
        SpawnUI(StartUIs.SettingUI);
    }

    public void ClickStartBtn()
    {
        if(GameManager.Instance.NowWorldSaveFolder == null)
        {
            SpawnUI(StartUIs.LoadUI);
        }
        else
        {
            OffThisUI();
            GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        }
    }
}
