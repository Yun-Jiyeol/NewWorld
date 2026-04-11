using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartUIs 
{
    MakeNewWorldUI,
    LoadUI
}


public class StartAdditionalUI : MonoBehaviour
{
    public GameObject LoadUI;
    public GameObject MakeNewWorldUI;

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

    public void ClickStartBtn()
    {
        if(GameManager.Instance.NowWorldSaveFolder == null)
        {
            SpawnUI(StartUIs.LoadUI);
        }
        else
        {
            GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
        }
    }
}
