using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PutWorldUI : BasicUI
{
    ChunkCode TargetObject = null;
    public TMP_InputField TextBox;
    public TextMeshProUGUI BeforeName;
    string SaveBN;

    public override void OnSetting()
    {
        if (GameManager.Instance.InputManager.TargetGameObject == null) return;
        TargetObject = GameManager.Instance.InputManager.TargetGameObject.GetComponent<ChunkCode>();

        SaveBN = TargetObject._name;
        BeforeName.text = SaveBN;
    }

    public void ChangeWorldName()
    {
        string Inputname = TextBox.text;

        if (SaveBN == Inputname || Inputname == "") return;

        Instantiate(GameManager.Instance.UIManager.SavedImage, gameObject.transform);

        GameManager.Instance.SaveAndLoadManager.ChangeName(TargetObject._name, Inputname);

        TargetObject._name = Inputname;
        SaveBN = TargetObject._name;
        BeforeName.text = SaveBN;

        TextBox.text = "";
    }

    public void PutWorld()
    {
        GameManager.Instance.InputManager.PutThisWorldInSpace();
    }

    public void DestroyWorld()
    {
        TargetObject.DestroyWorld();
    }
}
