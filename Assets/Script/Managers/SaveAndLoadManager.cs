using DG.Tweening.Plugins.Core.PathCore;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class WorldSaveData
{
    //번호
    public int Num;

    //좌표
    public float x;
    public float y;
    public float z;

    //방향
    public float xrot;
    public float yrot;
    public float zrot;
    public float wrot;

    //크기
    public float size;

    public List<BlockSaveData> blocks;
}

[System.Serializable]
public class BlockSaveData
{
    //좌표
    public float x;
    public float y;
    public float z;

    //색
    public float r;
    public float g;
    public float b;
    public float a;
}

public class SaveAndLoadManager : MonoBehaviour
{
    public static string RootPath => Application.persistentDataPath;
    public static string SaveFolder => System.IO.Path.Combine(RootPath, "Saves");

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }
    }

    private void Start()
    {
        EnsureSaveFolder();
        Debug.Log(Application.persistentDataPath);
        string[] files = Directory.GetFiles(SaveFolder);
        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);

                if (fileName.StartsWith("World_"))
                {
                    LoadWorldData(file);
                }
            }
        }

    }

    public void SaveWorldData(WorldBaseCode _world)
    {
        EnsureSaveFolder();

        string fileName = $"World_{_world.Num.ToString()}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[Save] 월드 삭제됨: " + _world.Num);
        }

        WorldSaveData worldsave = new WorldSaveData() 
        {
            Num = _world.Num,

            x = _world.gameObject.transform.position.x,
            y = _world.gameObject.transform.position.y,
            z = _world.gameObject.transform.position.z,

            xrot = _world.gameObject.transform.rotation.x,
            yrot = _world.gameObject.transform.rotation.y,
            zrot = _world.gameObject.transform.rotation.z,
            wrot = _world.gameObject.transform.rotation.w,

            size = _world.gameObject.transform.localScale.x,

            blocks = new List<BlockSaveData>()
        };

        foreach(Vector3Int blockposition in _world.Blocks.Keys)
        {
            BlockSaveData blocksave = new BlockSaveData()
            {
                x= blockposition.x,
                y= blockposition.y,
                z= blockposition.z,

                r = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.r,
                g = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.g,
                b = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.b,
                a = _world.Blocks[blockposition].go.GetComponent<MeshRenderer>().material.color.a
            };

            worldsave.blocks.Add(blocksave);
        }

        string json = JsonUtility.ToJson(worldsave, true);
        File.WriteAllText(path, json);
    }

    public void DeleteData(WorldBaseCode _world)
    {
        EnsureSaveFolder();

        string fileName = $"World_{_world.Num.ToString()}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[Save] 월드 삭제됨: " + _world.Num);
        }
    }

    public void LoadWorldData(string path)
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        GameObject world = GameManager.Instance.WorldManager.MakeWorld();

        world.transform.position = new Vector3(worldsave.x, worldsave.y, worldsave.z);
        world.transform.rotation = new Quaternion(
            worldsave.xrot,
            worldsave.yrot,
            worldsave.zrot,
            worldsave.wrot
        );
        world.transform.localScale = Vector3.one * worldsave.size;

        WorldBaseCode worldBaseCode = world.GetComponent<WorldBaseCode>();

        worldBaseCode.Num = worldsave.Num;
        foreach (BlockSaveData block in worldsave.blocks)
        {
            worldBaseCode.MakeNewBlocks(
                GameManager.Instance.BuildManager.BasicBlock,
                new Vector3(block.x, block.y, block.z),
                new Color(block.r, block.g, block.b),
                block.a
            );
        }
    }
}
