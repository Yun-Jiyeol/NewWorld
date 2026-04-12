using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadWorldUI : NeedToDoOnit
{
    public Transform SpawnPos;
    public GameObject SaveFilePrefab;
    List<WorldSaveFile> WorldSaveFiles = new List<WorldSaveFile>();

    public GameObject StartLoadBtn;
    string NowChooseWorldName = null;

    GameObject BlockImage = null;
    bool isEndCancel = true;

    public override void Onit()
    {
        NowChooseWorldName= GameManager.Instance.NowWorldSaveFolder;

        SettingSaveWorlds();
        CheckLoadBtn();
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
        if(NowChooseWorldName == null || GameManager.Instance.NowWorldSaveFolder == NowChooseWorldName)
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
        StartLoadBtn.SetActive(false);
        StartCoroutine(CancelCoroutine());
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
        LoadNewWorld();
    }

    void CancelBefore()
    {
        GameManager.Instance.SaveAndLoadManager.CancelLoadWorldFolder();
        GameManager.Instance.WorldManager.DestroyAllWorld();

        isEndCancel = true;
    }

    void LoadNewWorld()
    {
        GameManager.Instance.NowWorldSaveFolder = NowChooseWorldName;
        GameManager.Instance.SaveAndLoadManager.LoadWorldFolder();
    }
}
