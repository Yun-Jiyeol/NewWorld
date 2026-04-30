using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettingPage : NeedToDoOnit
{
    public GameObject WarningToSaveAllWorld;
    public GameObject WarningToOffGame;

    Transform Parent;
    GameObject NowShow = null;

    public override void GetParent(Transform _parent)
    {
        Parent = _parent;
    }

    public void ClickAllSaveWorld()
    {
        NowShow = Instantiate(WarningToSaveAllWorld, Parent);
    }

    public void ClickGoToStartPage()
    {
        GameManager.Instance.GoToStartPage();
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    public void ClickGameOffBtn()
    {
        NowShow = Instantiate(WarningToOffGame, Parent);
    }
}
