using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUI : NeedToDoOnit
{
    public override void Onit()
    {
        base.Onit();
    }

    public void ClickOffBtn()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }
}
