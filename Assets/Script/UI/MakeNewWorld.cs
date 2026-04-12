using OpenCover.Framework.Model;
using System;
using TMPro;

public class MakeNewWorld : NeedToDoOnit
{
    public TMP_InputField NameField;

    string SettingName = "NewWorld";

    public override void Onit()
    {
        NameField.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(string value)
    {
        string filtered = System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9°¡-ÆR]", "");

        if (value != filtered)
        {
            NameField.text = filtered;
        }

        SettingName = filtered;
    }

    public void MakeWorld()
    {
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");

        if (SettingName == "")
        {
            SettingName = "NewWorld";
        }
        GameManager.Instance.NowWorldSaveFolder = SettingName + "_" + time;
        SaveAndLoadManager.EnsureSaveFolder();

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public void DeclineMakeWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }
}
