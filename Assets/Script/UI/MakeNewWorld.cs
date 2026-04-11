using System;
using TMPro;

public class MakeNewWorld : NeedToDoOnit
{
    public TMP_InputField NameField;

    string SettingName = "NewWorld";

    public override void Onit()
    {
        NameField.onValueChanged.AddListener(OnValueChanged);
        NameField.onEndEdit.AddListener(ChangeSettingName);
    }

    void OnValueChanged(string value)
    {
        string filtered = System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9°¡-ÆR]", "");

        if (value != filtered)
        {
            NameField.text = filtered;
        }
    }

    public void ChangeSettingName(string _input)
    {
        SettingName = _input;
    }

    public void MakeWorld()
    {
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");

        GameManager.Instance.NowWorldSaveFolder = SettingName + "_" + time;
        SaveAndLoadManager.EnsureSaveFolder();

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public void DeclineMakeWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }
}
