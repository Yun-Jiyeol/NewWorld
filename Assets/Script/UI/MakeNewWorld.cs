using DG.Tweening;
using OpenCover.Framework.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeNewWorld : NeedToDoOnit
{
    public TMP_InputField NameField;

    public RectTransform ArrowImage;
    public Image IconSprite;
    int spritenum = 0;
    int OutPutNum = 0;

    public GameObject AdditionalUIIcon;
    GameObject _AdditionalUIIcon = null;

    string SettingName = "NewWorld";

    private void Start()
    {
        OutPutNum = UnityEngine.Random.Range(0, 10);
    }

    public override void Onit()
    {
        NameField.onValueChanged.AddListener(OnValueChanged);
    }

    public override void Onit(int _num)
    {
        spritenum = _num;
        IconSprite.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];
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

    public void ResetWorldSetting()
    {
        spritenum = 0;
        IconSprite.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];

        SettingName = "NewWorld";
        NameField.text = SettingName;
    }

    public void MakeWorld()
    {
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");

        if (SettingName == "")
        {
            SettingName = "NewWorld";
        }
        GameManager.Instance.NowWorldSaveFolder = SettingName + '_' + time + '_' + spritenum + '_' + OutPutNum;
        SaveAndLoadManager.EnsureSaveFolder();

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public void DeclineMakeWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    public void SpawnAdditionalUIIcons()
    {
        if (_AdditionalUIIcon == null)
        {
            _AdditionalUIIcon = Instantiate(AdditionalUIIcon, gameObject.transform);
            _AdditionalUIIcon.GetComponent<NeedToDoOnit>().GetParent(gameObject);
        }

        ArrowImage.DOScaleX(ArrowImage.localScale.x * -1f , 0f);
        _AdditionalUIIcon.GetComponent<NeedToDoOnit>().Onit();
    }
}
