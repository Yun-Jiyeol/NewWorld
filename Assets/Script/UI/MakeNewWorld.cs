using DG.Tweening;
using OpenCover.Framework.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeNewWorld : NeedToDoOnit
{
    public Transform TapeSpawnTran;
    TapeSave _SpawnTapeSave;

    public TMP_InputField NameField;

    public RectTransform ArrowImage;
    public Image IconSprite;
    public int spritenum = 0;
    int OutPutNum = 0;

    public GameObject AdditionalUIIcon;
    GameObject _AdditionalUIIcon = null;
    public UI_BtnEffect AdditionalUIIconBtnEffect;
    public Transform SideSpawn;

    public Image WhiteCover;

    string SettingName = "NewWorld";
    string time;

    public RectTransform MainUI;

    [Header("Sound")]
    private AudioClip WordTypeSound;

    private void Awake()
    {
        MainUI.anchoredPosition = new Vector2(375f, -900f);
        WhiteCover.color = new Color(1,1,1,4/5f);
    }

    private void Start()
    {
        OutPutNum = UnityEngine.Random.Range(0, 10);
        time = DateTime.Now.ToString("yyyyMMddHHmmss");

        GameObject _go = Instantiate(GameManager.Instance.MaterialManager.Tapes[OutPutNum], TapeSpawnTran);
        _SpawnTapeSave = _go.GetComponent<TapeSave>();

        MainUI.DOAnchorPosY(-75f, 1f);
        WhiteCover.DOColor(new Color(0,0,0,0), 1.25f);

        GameManager.Instance.UIManager.StartSceneUI.AddText("CreateTape");

        WordTypeSound = ScreenTypingDotSoundFactory.CreateDot5();

        StartSettingTape();
    }

    public override void Onit()
    {
        NameField.onValueChanged.AddListener(OnValueChanged);
    }

    public override void Onit(int _num)
    {
        if (spritenum == _num) return;

        spritenum = _num;
        IconSprite.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];
        _SpawnTapeSave.WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];
        GameManager.Instance.SoundManager.PlayEffect(SoundChoose.CallAudio(ClickSound.IconSound));

        if (_AdditionalUIIcon != null) _AdditionalUIIcon.GetComponent<WorldIconParent>().CheckChild();
    }

    void OnValueChanged(string value)
    {
        string filtered = System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9°¡-ÆR]", "");

        if (value != filtered)
        {
            NameField.text = filtered;
        }

        SettingName = filtered;
        GameManager.Instance.SoundManager.PlayEffect(WordTypeSound);

        if (SettingName == "")
        {
            _SpawnTapeSave.WorldName.text = "NewWorld";
        }
        else
        {
            _SpawnTapeSave.WorldName.text = SettingName;
        }
    }

    public void ResetWorldSetting()
    {
        spritenum = 0;
        IconSprite.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];
        if(_AdditionalUIIcon != null) _AdditionalUIIcon.GetComponent<WorldIconParent>().CheckChild();

        SettingName = "NewWorld";
        NameField.SetTextWithoutNotify(SettingName);

        StartSettingTape();
    }

    public void MakeWorld()
    {
        if (SettingName == "")
        {
            SettingName = "NewWorld";
        }
        string SaveName = SettingName + '_' + time + '_' + spritenum + '_' + OutPutNum;
        SaveAndLoadManager.EnsureSaveFolder(SaveName);

        GameManager.Instance.UIManager.StartSceneUI.AddText("DoneTape");

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public void DeclineMakeWorld()
    {
        GameManager.Instance.UIManager.StartSceneUI.AddText("DestroyTape");

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public void SpawnAdditionalUIIcons()
    {
        if (_AdditionalUIIcon == null)
        {
            _AdditionalUIIcon = Instantiate(AdditionalUIIcon, SideSpawn);
            _AdditionalUIIcon.GetComponent<NeedToDoOnit>().GetParent(gameObject);
        }

        ArrowImage.DOScaleX(ArrowImage.localScale.x * -1f , 0f);

        _AdditionalUIIcon.GetComponent<NeedToDoOnit>().Onit();
    }

    void StartSettingTape()
    {
        _SpawnTapeSave.WorldName.text = SettingName;
        _SpawnTapeSave.WorldNum.text = "";

        _SpawnTapeSave.WorldMakeYear.text = time.Substring(0, 4) + '.';
        _SpawnTapeSave.WorldMakeMonth.text = time.Substring(4, 2) + '.';
        _SpawnTapeSave.WorldMakeDay.text = time.Substring(6, 2);

        _SpawnTapeSave.WorldIcon.sprite = GameManager.Instance.UIManager.WorldUIIcons.WorldUIIconsSetting[spritenum];
    }
}
