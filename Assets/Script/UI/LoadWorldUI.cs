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

            if (!folderName.StartsWith("World_"))
                continue;

            // world.json 있는지 체크 (안전장치)
            string worldInfoPath = Path.Combine(folder, "world.json");

            if (File.Exists(worldInfoPath))
            {
                validWorlds.Add(folder);
            }
        }

        foreach (string folder in validWorlds)
        {
            MakeWorld(GameManager.Instance.SaveAndLoadManager.GetName(folder));
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
        }
        else
        {
            GameManager.Instance.NowWorldSaveFolder = null;
        }

        CheckLoadBtn();
    }

    public void ClickExitKey()
    {
        GameManager.Instance.UIManager.StartAdditionalUI.OffThisUI();
    }

    void CheckLoadBtn()
    {
        if(GameManager.Instance.NowWorldSaveFolder == null || GameManager.Instance.NowWorldSaveFolder == NowChooseWorldName)
        {
            StartLoadBtn.SetActive(false);
        }
        else
        {
            StartLoadBtn.SetActive(true);
        }
    }
}
