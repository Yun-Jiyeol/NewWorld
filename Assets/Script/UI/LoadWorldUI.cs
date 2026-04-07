using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadWorldUI : MonoBehaviour
{
    public Transform SpawnPos;
    public GameObject SaveFilePrefab;

    private void Start()
    {
        SettingSaveWorlds();
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
    }

    public void ClickExitKey()
    {
        Destroy(gameObject);
    }
}
