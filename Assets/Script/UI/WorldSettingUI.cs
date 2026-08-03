using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorldSettingUI : BasicUI
{
    ChunkCode TargetObject = null;
    public TMP_InputField TextBox;
    public TextMeshProUGUI BeforeName;
    string SaveBN;

    public SoundData OnEndName;
    public SoundData OnClickOkay;

    public override void OnSetting()
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null) return;
        TargetObject = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>();

        SaveBN = TargetObject._name;
        BeforeName.text = SaveBN;
    }

    public void SaveName()
    {
        string Inputname = TextBox.text;

        if (SaveBN == Inputname || Inputname == "") return;

        Instantiate(GameManager.Instance.UIManager.SavedImage, GameManager.Instance.UIManager.SelectionUI.transform);

        GameManager.Instance.SaveAndLoadManager.ChangeName(TargetObject._name, Inputname);
        GameManager.Instance.SoundManager.PlayEffect(OnEndName);

        TargetObject._name = Inputname;
        SaveBN = TargetObject._name;
        BeforeName.text = SaveBN;
        
        TextBox.text = "";
    }

    public void ResetName()
    {
        if (TextBox.text == "") return;

        TextBox.text = "";
    }

    public void OnClickEnd()
    {
        GameManager.Instance.SaveSomeNowWorld();
        GameManager.Instance.SoundManager.PlayEffect(OnClickOkay);
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }
}
