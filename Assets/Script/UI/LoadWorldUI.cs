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
    bool isEndCancel = true;

    [Header("Move")]
    public Image BG;
    public RectTransform Desk;
    public RectTransform Btns;

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

    void CheckLoadBtn()
    {
        if(GameManager.Instance.NowWorldSaveFolder == NowChooseWorldName || NowChooseWorldName == null)
        {
            StartLoadBtn.SetActive(false);
        }
        else
        {
            StartLoadBtn.SetActive(true);
        }
    }

    public void StartLoadWorld()
    {
        StartCoroutine(CancelCoroutine());
    }

    public void StartWorld()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.ClickStartBtn();
    }

    IEnumerator CancelCoroutine()
    {
        isEndCancel = false;
        if(BlockImage != null)
        {
            BlockImage = Instantiate(GameManager.Instance.UIManager.BlockImage, transform);
        }

        CancelBefore();

        if (!isEndCancel)
        {
            yield return null;
        }

        if (BlockImage != null)
        {
            Destroy(BlockImage);
            BlockImage = null;
        }

        GameManager.Instance.NowWorldSaveFolder = NowChooseWorldName;

        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
        string[] parts = NowChooseWorldName.Split('_');
        GameManager.Instance.UIManager.StartSceneUI.AddText('[' + parts[0] + "]테이프를 실행한다.");

        GameManager.Instance.SaveAndLoadManager.LoadWorldFolder();
    }

    void CancelBefore()
    {
        GameManager.Instance.SaveAndLoadManager.CancelLoadWorldFolder();
        GameManager.Instance.WorldManager.DestroyAllWorld();

        isEndCancel = true;
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
    }

    public void OnEnd()
    {
        startAdditionalUI.DestroyLoadUI();

        BG.DOColor(AfterColor, 0.1f);
        Btns.DOAnchorPos(AftersBtnsPos, 0.3f);
        Desk.DOAnchorPos(AfterDeskPos, 0.5f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
