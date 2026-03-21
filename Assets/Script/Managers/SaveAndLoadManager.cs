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
        string[] files = Directory.GetFiles(SaveFolder);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);

            if (fileName.StartsWith("Data_"))
            {
                LoadData(file);
            }
        }

        string[] worldFolders = Directory.GetDirectories(SaveFolder);

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
            LoadWorldData(folder);
        }

        GameManager.Instance.WorldManager.LoadAllWorld();
    }

    string GetWorldFolder(string time)
    {
        return Path.Combine(SaveFolder, "World_" + time);
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
        EnsureSaveFolder();

        SaveWorldInfo(_world);

        yield return StartCoroutine(SaveChunksCoroutine(_world));
    }

    void SaveWorldInfo(ChunkCode world)
    {
        string folder = GetWorldFolder(world.Savedtime);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        WorldSaveData data = new WorldSaveData()
        {
            Name = world._name,
            Time = world.Savedtime,

            x = world.transform.position.x,
            y = world.transform.position.y,
            z = world.transform.position.z,

            xrot = world.transform.rotation.x,
            yrot = world.transform.rotation.y,
            zrot = world.transform.rotation.z,
            wrot = world.transform.rotation.w,

            size = world.transform.localScale.x
        };

        if (world.LightBlock != null)
        {
            Light L = world.LightBlock.GetComponent<Light>();

            int type = 0;
            if (L.type == LightType.Spot) type = 1;
            if (L.type == LightType.Point) type = 2;

            data.isLightBlock = true;

            data.lightblock = new LightBlockSaveData()
            {
                x = world.LightBlock.transform.position.x,
                y = world.LightBlock.transform.position.y,
                z = world.LightBlock.transform.position.z,

                xrot = world.LightBlock.transform.rotation.x,
                yrot = world.LightBlock.transform.rotation.y,
                zrot = world.LightBlock.transform.rotation.z,
                wrot = world.LightBlock.transform.rotation.w,

                r = L.color.r,
                g = L.color.g,
                b = L.color.b,

                type = type,
                SpotAngle = L.spotAngle,
                Range = L.range,
                Intensive = L.intensity
            };
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path.Combine(folder, "world.json"), json);
    }
    IEnumerator SaveChunksCoroutine(ChunkCode world)
    {
        string folder = GetWorldFolder(world.Savedtime);

        float start = Time.realtimeSinceStartup;

        foreach (var chunk in world.BlocksinChunk)
        {
            SaveChunk(chunk.Key, chunk.Value, folder);

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }
    }

    void SaveChunk(Vector3Int chunkPos, Dictionary<Vector3Int, BlockInChunkData> blocks, string folder)
    {
        string file = Path.Combine(folder,
            $"chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}.bin");

        using (BinaryWriter writer = new BinaryWriter(File.Open(file, FileMode.Create)))
        {
            writer.Write(blocks.Count);

            foreach (var block in blocks.Values)
            {
                writer.Write(block._position.x);
                writer.Write(block._position.y);
                writer.Write(block._position.z);

                writer.Write(block._color.r);
                writer.Write(block._color.g);
                writer.Write(block._color.b);
                writer.Write(block._color.a);
            }
        }
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

    void LoadWorldData(string folder)
    {
        string json = File.ReadAllText(Path.Combine(folder, "world.json"));
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        GameObject world = GameManager.Instance.WorldManager.MakeWorld();

        world.transform.position =
            new Vector3(worldsave.x, worldsave.y, worldsave.z);

        world.transform.rotation =
            new Quaternion(worldsave.xrot, worldsave.yrot, worldsave.zrot, worldsave.wrot);

        world.transform.localScale = Vector3.one * worldsave.size;

        ChunkCode worldCode = world.GetComponent<ChunkCode>();

        worldCode._name = worldsave.Name;
        worldCode.Savedtime = worldsave.Time;

        StartCoroutine(LoadChunksCoroutine(folder, worldCode));
    }

    IEnumerator LoadChunksCoroutine(string folder, ChunkCode world)
    {
        string[] files = Directory.GetFiles(folder, "chunk_*.bin");

        float start = Time.realtimeSinceStartup;

        foreach (string file in files)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    int z = reader.ReadInt32();

                    float r = reader.ReadSingle();
                    float g = reader.ReadSingle();
                    float b = reader.ReadSingle();
                    float a = reader.ReadSingle();

                    world.MakeWorldQueue.Enqueue(
                        new BlockInChunkData
                        {
                            _position = new Vector3Int(x, y, z),
                            _color = new Color(r, g, b, a)
                        });
                }
            }

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
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
