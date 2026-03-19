using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class WorldSaveData
{
    //번호
    public string Name;
    public string Time;

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

    public bool isLightBlock = false;
    public LightBlockSaveData lightblock;
}

[System.Serializable]
public class BlockSaveData
{
    //좌표
    public int x;
    public int y;
    public int z;

    //색
    public float r;
    public float g;
    public float b;
    public float a;
}

[System.Serializable]
public class LightBlockSaveData
{
    //좌표
    public float x;
    public float y;
    public float z;

    //방향
    public float xrot;
    public float yrot;
    public float zrot;
    public float wrot;

    //빛 관련 저장
    public float r;
    public float g;
    public float b;

    public int type; //1 -> Spot, 2 -> point
    public float SpotAngle;
    public float Range;
    public float Intensive;
}

[System.Serializable]
public class SaveAllData
{
    public int NumberOfWorld;

    public ColorSaveData maincolor;
    public List<ColorSaveData> precolor;
    public List<ColorSaveData> savecolor;

    public LightBlockSaveData MainLight;
}

[System.Serializable]
public class ColorSaveData
{
    public float r;
    public float g;
    public float b;
    public float a;
}

public class SaveAndLoadManager : MonoBehaviour
{
    public static string RootPath => Application.persistentDataPath;
    public static string SaveFolder => System.IO.Path.Combine(RootPath, "Saves");

    Queue<ChunkCode> saveQueue = new Queue<ChunkCode>();
    HashSet<ChunkCode> queuedWorlds = new HashSet<ChunkCode>();
    HashSet<ChunkCode> dirtyWorlds = new HashSet<ChunkCode>();
    bool isSaving = false;

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

        List<string> worldfile = new List<string>();

        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);

                if (fileName.StartsWith("Data_"))
                {
                    LoadData(file);
                }
                if (fileName.StartsWith("World_"))
                {
                    worldfile.Add(file);
                }
            }
        }

        if(worldfile.Count > 0)
        {
            foreach(string file in worldfile)
            {
                LoadWorldData(file);
            }

            GameManager.Instance.WorldManager.LoadAllWorld();
        }
    }

    public void SaveWorldData(ChunkCode _world)
    {
        // 이미 저장 중이면 dirty 처리
        if (isSaving && currentSavingWorld == _world)
        {
            dirtyWorlds.Add(_world);
            return;
        }

        // 이미 Queue에 있으면 덮어쓰기
        if (queuedWorlds.Contains(_world))
        {
            dirtyWorlds.Add(_world);
            return;
        }

        saveQueue.Enqueue(_world);
        queuedWorlds.Add(_world);

        if (!isSaving)
            StartCoroutine(SaveWorker());
    }

    ChunkCode currentSavingWorld;

    IEnumerator SaveWorker()
    {
        isSaving = true;

        while (saveQueue.Count > 0)
        {
            ChunkCode world = saveQueue.Dequeue();
            queuedWorlds.Remove(world);

            currentSavingWorld = world;

            yield return StartCoroutine(SaveWorldCoroutine(world));

            // 저장 중 추가 요청이 있었다면 다시 Queue
            if (dirtyWorlds.Contains(world))
            {
                dirtyWorlds.Remove(world);

                saveQueue.Enqueue(world);
                queuedWorlds.Add(world);
            }
        }

        isSaving = false;
    }

    IEnumerator SaveWorldCoroutine(ChunkCode _world)
    {
        Debug.Log("저장중");

        EnsureSaveFolder();

        string fileName = $"World_{_world.Savedtime}.json";
        string path = Path.Combine(SaveFolder, fileName);

        if (File.Exists(path))
            File.Delete(path);

        float start = Time.realtimeSinceStartup;

        using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8, 65536))
        {
            // World Header
            writer.Write("{");

            writer.Write($"\"Name\":\"{_world._name}\",");
            writer.Write($"\"Time\":\"{_world.Savedtime}\",");

            writer.Write($"\"x\":{_world.transform.position.x},");
            writer.Write($"\"y\":{_world.transform.position.y},");
            writer.Write($"\"z\":{_world.transform.position.z},");

            writer.Write($"\"xrot\":{_world.transform.rotation.x},");
            writer.Write($"\"yrot\":{_world.transform.rotation.y},");
            writer.Write($"\"zrot\":{_world.transform.rotation.z},");
            writer.Write($"\"wrot\":{_world.transform.rotation.w},");

            writer.Write($"\"size\":{_world.transform.localScale.x},");

            writer.Write("\"blocks\":[");

            bool first = true;

            foreach (var blocks in _world.BlocksinChunk.Values)
            {
                foreach (BlockInChunkData block in blocks.Values)
                {
                    if (!first)
                        writer.Write(",");

                    first = false;

                    writer.Write("{");
                    writer.Write($"\"x\":{Mathf.RoundToInt(block._position.x)},");
                    writer.Write($"\"y\":{Mathf.RoundToInt(block._position.y)},");
                    writer.Write($"\"z\":{Mathf.RoundToInt(block._position.z)},");
                    writer.Write($"\"r\":{block._color.r},");
                    writer.Write($"\"g\":{block._color.g},");
                    writer.Write($"\"b\":{block._color.b},");
                    writer.Write($"\"a\":{block._color.a}");
                    writer.Write("}\n");

                    // Time slicing
                    if ((Time.realtimeSinceStartup - start) * 1000f > 3f)
                    {
                        writer.Flush();
                        yield return null;
                        start = Time.realtimeSinceStartup;
                    }
                }
            }

            writer.Write("]");

            // LightBlock
            if (_world.LightBlock != null)
            {
                GameObject LB = _world.LightBlock.gameObject;
                Light L = LB.GetComponent<Light>();

                int type = 0;
                if (L.type == LightType.Spot) type = 1;
                if (L.type == LightType.Point) type = 2;

                writer.Write(",\"isLightBlock\":true");

                writer.Write(",\"lightblock\":{");

                writer.Write($"\"x\":{LB.transform.position.x},");
                writer.Write($"\"y\":{LB.transform.position.y},");
                writer.Write($"\"z\":{LB.transform.position.z},");

                writer.Write($"\"xrot\":{LB.transform.rotation.x},");
                writer.Write($"\"yrot\":{LB.transform.rotation.y},");
                writer.Write($"\"zrot\":{LB.transform.rotation.z},");
                writer.Write($"\"wrot\":{LB.transform.rotation.w},");

                writer.Write($"\"r\":{L.color.r},");
                writer.Write($"\"g\":{L.color.g},");
                writer.Write($"\"b\":{L.color.b},");

                writer.Write($"\"type\":{type},");
                writer.Write($"\"SpotAngle\":{L.spotAngle},");
                writer.Write($"\"Range\":{L.range},");
                writer.Write($"\"Intensive\":{L.intensity}");

                writer.Write("}");
            }
            else
            {
                writer.Write(",\"isLightBlock\":false");
            }

            writer.Write("}");
        }

        Debug.Log("저장끝");
    }

    public void DeleteData(ChunkCode _world)
    {
        EnsureSaveFolder();

        string fileName = $"World_{_world.Savedtime}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
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

        ChunkCode worldBaseCode = world.GetComponent<ChunkCode>();

        worldBaseCode._name = worldsave.Name;
        worldBaseCode.Savedtime = worldsave.Time;

        foreach (BlockSaveData block in worldsave.blocks)
        {
            worldBaseCode.MakeWorldQueue.Enqueue(new BlockInChunkData { _color = new Color(block.r, block.g, block.b, block.a), _position = new Vector3Int(block.x, block.y, block.z) });
        }

        if (worldsave.isLightBlock)
        {
            Light light = worldBaseCode.ChangeLightPower();
            light.gameObject.transform.position = new Vector3(worldsave.lightblock.x, worldsave.lightblock.y, worldsave.lightblock.z);
            light.gameObject.transform.rotation = new Quaternion(worldsave.lightblock.xrot, worldsave.lightblock.yrot, worldsave.lightblock.zrot, worldsave.lightblock.wrot);
            light.color = new Color(worldsave.lightblock.r, worldsave.lightblock.g, worldsave.lightblock.b);
            switch (worldsave.lightblock.type)
            {
                case 1:
                    light.type = LightType.Spot;
                    break;
                case 2:
                    light.type = LightType.Point;
                    break;
            }
            light.spotAngle = worldsave.lightblock.SpotAngle;
            light.range = worldsave.lightblock.Range;
            light.intensity = worldsave.lightblock.Intensive;
        }
    }

    public void SaveData()
    {
        string fileName = $"Data_personal.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        BuildManager BM = GameManager.Instance.BuildManager;

        SaveAllData savedata = new SaveAllData()
        {
            NumberOfWorld = GameManager.Instance.WorldManager.HowManyYouMakeWorld,

            maincolor = new ColorSaveData()
            {
                r = BM.MainColor.color.r,
                g = BM.MainColor.color.g,
                b = BM.MainColor.color.b,
                a = BM.alfar
            },

            precolor = new List<ColorSaveData>(),
            savecolor = new List<ColorSaveData>()
        };

        if (BM.PreColors.Count > 0)
        {
            foreach (Color color in BM.PreColors)
            {
                savedata.precolor.Add(new ColorSaveData()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
        }

        if (BM.SaveColors.Count > 0)
        {
            foreach (Color color in BM.SaveColors)
            {
                savedata.savecolor.Add(new ColorSaveData()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
        }

        GameObject ML = GameManager.Instance.LightManager.MainLight;
        savedata.MainLight = new LightBlockSaveData()
        {
            xrot = ML.transform.rotation.x,
            yrot = ML.transform.rotation.y,
            zrot = ML.transform.rotation.z,
            wrot = ML.transform.rotation.w,

            r = ML.GetComponent<Light>().color.r,
            g = ML.GetComponent<Light>().color.g,
            b = ML.GetComponent<Light>().color.b
        };

        string json = JsonUtility.ToJson(savedata, true);
        File.WriteAllText(path, json);
    }

    public void LoadData(string path)
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SaveAllData worldsave = JsonUtility.FromJson<SaveAllData>(json);

        GameManager.Instance.WorldManager.HowManyYouMakeWorld = worldsave.NumberOfWorld;

        BuildManager BM = GameManager.Instance.BuildManager;

        BM.MainColor.color =  new Color(worldsave.maincolor.r, worldsave.maincolor.g, worldsave.maincolor.b, worldsave.maincolor.a);
        BM.alfar = worldsave.maincolor.a;

        if (worldsave.precolor.Count > 0)
        {
            foreach (ColorSaveData color in worldsave.precolor)
            {
                BM.AddPreColor(new Color(color.r, color.g, color.b, color.a));
            }
        }
        if (worldsave.savecolor.Count > 0)
        {
            for(int i = 0; i < worldsave.savecolor.Count; i++)
            {
                BM.AddSaveColor(new Color(worldsave.savecolor[i].r, worldsave.savecolor[i].g, worldsave.savecolor[i].b, worldsave.savecolor[i].a), i);
            }
        }

        GameObject ML = GameManager.Instance.LightManager.MainLight;
        ML.transform.rotation = new Quaternion(worldsave.MainLight.xrot, worldsave.MainLight.yrot, worldsave.MainLight.zrot, worldsave.MainLight.wrot);
        ML.GetComponent<Light>().color = new Color(worldsave.MainLight.r, worldsave.MainLight.g, worldsave.MainLight.b);
    }

    public void SaveAllWorld()
    {
        foreach(GameObject wb in GameManager.Instance.WorldManager.Worlds)
        {
            SaveWorldData(wb.GetComponent<ChunkCode>());
        }
    }

    public void ChangeName(string Time, string changedName)
    {
        EnsureSaveFolder();

        string fileName = $"World_{Time}.json";

        string path = System.IO.Path.Combine(SaveFolder, fileName);

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        worldsave.Name = changedName;

        json = JsonUtility.ToJson(worldsave, true);
        File.WriteAllText(path, json);
    }
}
