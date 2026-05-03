using DG.Tweening;
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

    [Header("Move")]
    public RectTransform Desk;
    Vector2 DeskOriginalPos;
    Vector2 DeskAfterPos = new Vector2(0, - 150f);

    private void Awake()
    {
        DeskOriginalPos = Desk.anchoredPosition;
    }

    public void SpawnUI(StartUIs selectUI)
    {
        if(NowUI != null) OffThisUI();

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
        go.GetComponent<NeedToDoOnit>().GetParent(gameObject);
        go.GetComponent<NeedToDoOnit>().Onit();
    }

    public void OffThisUI()
    {
        if(NowUI.TryGetComponent<OnEndAction>(out OnEndAction _code))
        {
            _code.OnEnd();
        }
        else 
        {
            Destroy(NowUI);
        }

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

    public void SpawnLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("서랍을 뒤적인다.");
        Desk.DOAnchorPos(DeskOriginalPos + DeskAfterPos, 0.7f);
    }

    public void DestroyLoadUI()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("서랍을 닫는다.");
        Desk.DOAnchorPos(DeskOriginalPos, 0.5f);
    }

    public void ClickExitGameBtn()
    {
        if (GameManager.Instance.UIManager.StartSceneUI.isStart)
        {
            GameManager.Instance.Exit();
        }
        else
        {
            GameManager.Instance.UIManager.StartSceneUI.ClickStart();
        }
    }
}
