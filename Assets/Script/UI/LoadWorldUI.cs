using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadWorldUI : NeedToDoOnit, OnEndAction
{
    StartAdditionalUI startAdditionalUI;

    public Transform SpawnPos;
    public GameObject SaveFilePrefab;
    List<WorldSaveFile> WorldSaveFiles = new List<WorldSaveFile>();

    public GameObject StartLoadBtn;
    string NowChooseWorldName = null;

    GameObject BlockImage = null;
    bool isSwitchingWorld = false;

    public GameObject MakeWorldIcon;
    public GameObject DestroyWorldIcon;

    [Header("Move")]
    public Image BG;
    public RectTransform Desk;
    public RectTransform Btns;

    [Header("Sound")]
    public SoundData DeskOpenSound;
    public SoundData DeskCloseSound;

    private void Awake()
    {
        StartLoadBtn.SetActive(false);
        SaveMove();
    }

    public override void Onit()
    {
        NowChooseWorldName= GameManager.Instance.NowWorldSaveFolder;

        SettingSaveWorlds();
        CheckLoadBtn();

        StartMove();
    }

    public override void GetParent(GameObject _parent)
    {
        startAdditionalUI = _parent.GetComponent<StartAdditionalUI>();
    }

    void SettingSaveWorlds()
    {
        SaveAndLoadManager.EnsureSaveFolder();
        string[] worldFolders = Directory.GetDirectories(SaveAndLoadManager.SaveFolder);

        List<string> validWorlds = new List<string>();

        foreach (string folder in worldFolders)
        {
            string folderName = Path.GetFileName(folder);

            validWorlds.Add(folderName);
        }

        foreach (string folder in validWorlds)
        {
            MakeWorld(folder);
        }
    }

    void MakeWorld(string WorldName)
    {
        SpawnPos.gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 200f);

        GameObject go = Instantiate(SaveFilePrefab, SpawnPos);
        go.GetComponent<WorldSaveFile>().Onit(WorldName);

        WorldSaveFiles.Add(go.GetComponent<WorldSaveFile>());
    }

    public void CheckAllWorlds(WorldSaveFile _wsf)
    {
        if (_wsf.isCheck)
        {
            foreach (WorldSaveFile WSF in WorldSaveFiles)
            {
                if(WSF != _wsf) WSF.UnCheck();
            }

            NowChooseWorldName = _wsf.GetName();
        }
        else
        {
            NowChooseWorldName = null;
        }

        CheckLoadBtn();
    }

    public void ClickExitKey()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    public void ClickMakeNewWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.MakeNewWorldUI);
    }

    public void ClickDestroyWorld()
    {
        GameManager.Instance.SaveAndLoadManager.DeleteSaveFolder(NowChooseWorldName);

        NowChooseWorldName = null;
        GameManager.Instance.NowWorldSaveFolder = null;

        ResetTapes();
    }

    void ResetTapes()
    {
        foreach (WorldSaveFile WSF in WorldSaveFiles)
        {
            Destroy(WSF.gameObject); 
        }

        WorldSaveFiles.Clear();

        SettingSaveWorlds();
        CheckLoadBtn();
    }

    void CheckLoadBtn()
    {
        if (NowChooseWorldName == null)
        {
            StartLoadBtn.SetActive(false);
            MakeWorldIcon.SetActive(true);
            DestroyWorldIcon.SetActive(false);
        }
        else if(GameManager.Instance.NowWorldSaveFolder == NowChooseWorldName)
        {
            StartLoadBtn.SetActive(false);
            MakeWorldIcon.SetActive(false);
            DestroyWorldIcon.SetActive(true);
        }
        else
        {
            StartLoadBtn.SetActive(true);
            MakeWorldIcon.SetActive(false);
            DestroyWorldIcon.SetActive(true);
        }
    }

    public void StartLoadWorld()
    {
        if (isSwitchingWorld)
            return;

        StartCoroutine(CancelCoroutine());
    }

    public void StartWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.ClickStartBtn();
    }

    IEnumerator CancelCoroutine()
    {
        isSwitchingWorld = true;
        if (BlockImage == null)
        {
            BlockImage = Instantiate(
                GameManager.Instance.UIManager.BlockImage,
                transform
            );
        }

        SaveAndLoadManager saveManager =
            GameManager.Instance.SaveAndLoadManager;

        saveManager.CancelLoadWorldFolder();

        while (saveManager.IsLoading || saveManager.IsSaving)
        {
            yield return null;
        }

        GameManager.Instance.WorldManager.DestroyAllWorld();

        // Destroy is deferred until the end of the frame.
        yield return null;

        if (BlockImage != null)
        {
            Destroy(BlockImage);
            BlockImage = null;
        }

        GameManager.Instance.NowWorldSaveFolder = NowChooseWorldName;

        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
        GameManager.Instance.UIManager.StartSceneUI.AddText("TapeIn");
        GameManager.Instance.UIManager.StartAdditionalUI.ChangeCreationState(CreateState.Looking);

        saveManager.LoadWorldFolder();

        isSwitchingWorld = false;
    }

    Color OriginalColor;
    Vector2 OriginalDeskPos;
    Vector2 OriginalBtnsPos;

    Color AfterColor;
    Vector2 AfterDeskPos;
    Vector2 AftersBtnsPos;

    void SaveMove()
    {
        OriginalColor = BG.color;
        OriginalDeskPos = Desk.anchoredPosition;
        OriginalBtnsPos = Btns.anchoredPosition;

        Color AfterColor = OriginalColor;
        AfterColor.a = 0;
        BG.color = AfterColor;

        AfterDeskPos = Desk.anchoredPosition + new Vector2(0, 1300f);
        Desk.anchoredPosition = AfterDeskPos;

        AftersBtnsPos = Btns.anchoredPosition + new Vector2(-600f, 0);
        Btns.anchoredPosition = AftersBtnsPos;
    }

    void StartMove()
    {
        startAdditionalUI.SpawnLoadUI();

        BG.DOColor(OriginalColor, 0.2f);
        Desk.DOAnchorPos(OriginalDeskPos, 0.7f);
        Btns.DOAnchorPos(OriginalBtnsPos, 0.5f);

        GameManager.Instance.SoundManager.PlayEffect(DeskOpenSound);
    }

    public void OnEnd()
    {
        startAdditionalUI.DestroyLoadUI();

        BG.DOColor(AfterColor, 0.1f);
        Btns.DOAnchorPos(AftersBtnsPos, 0.3f);
        GameManager.Instance.SoundManager.PlayEffect(DeskCloseSound);

        Desk.DOAnchorPos(AfterDeskPos, 0.5f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
