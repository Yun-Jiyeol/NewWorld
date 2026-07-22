using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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
public class SettingValue
{
    public CameraEffect type;
    public bool isOn;
    public float NowValue;
}

[System.Serializable]
public class CameraSettingSave
{
    public List<SettingValue> CameraAdditionalEffect;
    public List<bool> IsSubCameraUse;
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
public class MainLightSave
{
    public LightBlockSaveData MainLightColor;
}

[System.Serializable]
public class BlockColorsSave
{
    public ColorSaveData maincolor;
    public List<ColorSaveData> precolor;
    public List<ColorSaveData> savecolor;
}

[System.Serializable]
public class PlayerAchieve
{
    public int HowManyMakeWorld;
}

[System.Serializable]
public class ColorSaveData
{
    public float r;
    public float g;
    public float b;
    public float a;
}

[System.Serializable]
public class SoundSettingSave
{
    public float Main;
    public float Effect;
    public float BGM;
}

[System.Serializable]
public class LanguageSettingSave
{
    public LanguageEnum Language;
}

[System.Serializable]
public class PlayerPositionSave
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
}

[System.Serializable]
public class KeySaves
{
    public List<KeySave> keys = new List<KeySave>();
}

[System.Serializable]
public class KeySave 
{
    public KeyNameSaves keyname;
    public Keyboards keyboard;
}


public class SaveAndLoadManager : MonoBehaviour
{
    public static string RootPath => Application.persistentDataPath;
    public static string PlayerSettingFolder => System.IO.Path.Combine(RootPath, $"PlayerSetting");
    public static string SaveFolder => System.IO.Path.Combine(RootPath, $"Saves");
    public static string WorldSaveFolder => System.IO.Path.Combine(SaveFolder, $"{GameManager.Instance.NowWorldSaveFolder}");
    public static string WorldPlayerSetting => System.IO.Path.Combine(WorldSaveFolder, $"WorldPlayerSetting"); 
    public static string DefaultWorldTemplateFolder => System.IO.Path.Combine(Application.streamingAssetsPath, "DefaultWorld", "World_Default");

    Queue<ChunkCode> saveQueue = new Queue<ChunkCode>();
    HashSet<ChunkCode> queuedWorlds = new HashSet<ChunkCode>();
    HashSet<ChunkCode> dirtyWorlds = new HashSet<ChunkCode>();

    bool isSaving = false;
    bool isLoading = false;
    bool cancelLoading = false;
    List<Coroutine> loadingCoroutines = new List<Coroutine>();

    public static void EnsureSaveFolder(string _saveFolderName)
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }

        if (!string.IsNullOrEmpty(_saveFolderName))
        {
            string _worldSaveFolder = System.IO.Path.Combine(SaveFolder, _saveFolderName);
            string _worldPlayerSetting = System.IO.Path.Combine(_worldSaveFolder, "WorldPlayerSetting");

            Directory.CreateDirectory(_worldSaveFolder);
            Directory.CreateDirectory(_worldPlayerSetting);

            GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().ResetMaterial();
        }
    }

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }

        if(GameManager.Instance.NowWorldSaveFolder != null)
        {
            Directory.CreateDirectory(WorldSaveFolder);
            Directory.CreateDirectory(WorldPlayerSetting);
            GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().ResetMaterial();
        }
    }

    private void Start()
    {
        if (Directory.Exists(PlayerSettingFolder))
        {
            LoadStartFile(PlayerSettingFolder);
        }
    }

    public void CancelLoadWorldFolder()
    {
        CancelLoading();

        cancelLoading = false;
    }

    public void LoadWorldFolder()
    {
        isLoading = true;

        EnsureSaveFolder();

        GameManager.Instance.UIManager.StartAdditionalUI.ChangeCreationState(CreateState.Setting);
        if (Directory.Exists(WorldPlayerSetting) && Directory.EnumerateFiles(WorldPlayerSetting).Any())
            LoadFile(WorldPlayerSetting);
        else
            GameManager.Instance.MainCamera.GetComponent<CameraShaderPutter>().ResetMaterial();

        string[] worldFolders = Directory.GetDirectories(WorldSaveFolder);

        List<string> validWorlds = new List<string>();

        foreach (string folder in worldFolders)
        {
            string folderName = Path.GetFileName(folder);

            if (!folderName.StartsWith("World_"))
                continue;

            string worldInfoPath = Path.Combine(folder, "world.json");

            if (File.Exists(worldInfoPath))
                validWorlds.Add(folder);
        }

        foreach (string folder in validWorlds)
        {
            if (cancelLoading) break;

            LoadWorldData(folder);
        }

        GameManager.Instance.UIManager.StartAdditionalUI.ChangeCreationState(CreateState.Making);
        GameManager.Instance.WorldManager.LoadAllWorld();

        isLoading = false;
    }

    string GetWorldFolder(string time)
    {
        return Path.Combine(WorldSaveFolder, "World_" + time);
    }

    public void SaveWorldData(ChunkCode _world) //설치 한 이후 1회, 제작 중 1분 단위로 1회
    {
        _world.isSavedBefore = false;

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
        GameManager.Instance.UIManager.SavingEffectUI.StartSaving();

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
        GameManager.Instance.UIManager.SavingEffectUI.EndSaving();
    }

    IEnumerator SaveWorldCoroutine(ChunkCode _world)
    {
        EnsureSaveFolder();

        SaveWorldInfo(_world);

        yield return StartCoroutine(SaveChunksCoroutine(_world));
    }

    public void SaveWorldInfo(ChunkCode world)
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

        string folder = Path.Combine(WorldSaveFolder, $"World_{_world.Savedtime}");

        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }
    }

    public void DeleteSaveFolder(string saveFolderName)
    {
        if (string.IsNullOrEmpty(saveFolderName))
        {
            Debug.LogWarning("[Save] 삭제할 세이브 폴더명이 비어 있습니다.");
            return;
        }

        string folder = Path.Combine(SaveFolder, saveFolderName);

        if (!Directory.Exists(folder))
        {
            Debug.LogWarning($"[Save] 삭제할 세이브 폴더가 없습니다: {folder}");
            return;
        }

        Directory.Delete(folder, true);

        Debug.Log($"[Save] 세이브 폴더 삭제 완료: {folder}");
    }

    void LoadWorldData(string folder)
    {
        if (cancelLoading) return;

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
        worldCode.isSavedBefore = true;

        if (worldsave.isLightBlock)
        {
            Light _light = worldCode.ChangeLightPower();
            _light.gameObject.transform.position = new Vector3(worldsave.lightblock.x, worldsave.lightblock.y, worldsave.lightblock.z);
            _light.gameObject.transform.rotation = new Quaternion(worldsave.lightblock.xrot, worldsave.lightblock.yrot,
                worldsave.lightblock.zrot, worldsave.lightblock.wrot);
            _light.color = new Color(worldsave.lightblock.r, worldsave.lightblock.g, worldsave.lightblock.b);

            _light.type = worldsave.lightblock.type == 1 ? LightType.Spot : LightType.Point;
            _light.spotAngle = worldsave.lightblock.SpotAngle;
            _light.range = worldsave.lightblock.Range;
            _light.intensity = worldsave.lightblock.Intensive;
        }

        Coroutine co = StartCoroutine(LoadChunksCoroutine(folder, worldCode));
        loadingCoroutines.Add(co);
    }

    IEnumerator LoadChunksCoroutine(string folder, ChunkCode world)
    {
        string[] files = Directory.GetFiles(folder, "chunk_*.bin");

        float start = Time.realtimeSinceStartup;

        foreach (string file in files)
        {
            if (cancelLoading) yield break; 

            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    if (cancelLoading) yield break;

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
    public void CancelLoading()
    {
        if (!isLoading) return;

        cancelLoading = true;

        foreach (var co in loadingCoroutines)
        {
            if (co != null)
                StopCoroutine(co);
        }

        loadingCoroutines.Clear();

        isLoading = false;

        Debug.Log("로드 취소됨");
    }

    public void SaveAll()
    {
        PlayerPosSave();
        SaveMainLight();
        SaveBlockColors();
        SavePlayerAchive();
        SaveCameraSetting();
    }

    string TryMakeDataFolder()
    {
        string path = WorldPlayerSetting;   
        Directory.CreateDirectory(path);
        return path;
    }

    public void PlayerPosSave()
    {
        string _path = TryMakeDataFolder();
        _path = Path.Combine(_path, "PlayerPos.json");

        Transform Player = GameManager.Instance.MainCamera.gameObject.transform;

        PlayerPositionSave playerPositionSave = new PlayerPositionSave()
        {
            x = Player.position.x,
            y = Player.position.y,
            z = Player.position.z,

            xrot = Player.rotation.x,
            yrot = Player.rotation.y,
            zrot = Player.rotation.z,
            wrot = Player.rotation.w
        };

        File.WriteAllText(_path, JsonUtility.ToJson(playerPositionSave, true));
    }

    public void SaveMainLight()
    {
        string _path = TryMakeDataFolder();
        _path = Path.Combine(_path, "MainLight.json");

        GameObject MainLight = GameManager.Instance.LightManager.MainLight;
        Light _mainlight = MainLight.GetComponent<Light>();

        MainLightSave maincolor = new MainLightSave() 
        {
            MainLightColor = new LightBlockSaveData()
            {
                xrot = MainLight.transform.rotation.x,
                yrot = MainLight.transform.rotation.y,
                zrot = MainLight.transform.rotation.z,
                wrot = MainLight.transform.rotation.w,

                r = _mainlight.color.r,
                g = _mainlight.color.g,
                b = _mainlight.color.b
            }
        };

        File.WriteAllText(_path, JsonUtility.ToJson(maincolor, true));
    }

    public void SaveBlockColors()
    {
        string _path = TryMakeDataFolder();
        _path = Path.Combine(_path, "BlockColors.json");

        BuildManager BM = GameManager.Instance.BuildManager;

        BlockColorsSave blockcolors = new BlockColorsSave() 
        {
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
                blockcolors.precolor.Add(new ColorSaveData()
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
                blockcolors.savecolor.Add(new ColorSaveData()
                {
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
        }


        File.WriteAllText(_path, JsonUtility.ToJson(blockcolors, true));
    }

    public void SavePlayerAchive()
    {
        string _path = TryMakeDataFolder();
        _path = Path.Combine(_path, "PlayerAchive.json");

        PlayerAchieve PA = new PlayerAchieve()
        {
            HowManyMakeWorld = GameManager.Instance.WorldManager.HowManyYouMakeWorld
        };

        File.WriteAllText(_path, JsonUtility.ToJson(PA, true));
    }

    public void SaveCameraSetting()
    {
        string _path = TryMakeDataFolder();
        _path = Path.Combine(_path, "CameraSetting.json");

        Material CameraMaterial = GameManager.Instance.MainCamera.gameObject.GetComponent<CameraShaderPutter>().CamMaterial;

        CameraSettingSave CSS = new CameraSettingSave()
        {
            CameraAdditionalEffect = new List<SettingValue>(),
            IsSubCameraUse = new List<bool>()
        };

        int num = 0;
        CSS.CameraAdditionalEffect.Add(new SettingValue() {type = CameraEffect.Reverse, isOn = CameraMaterial.GetFloat(((SystemName)num).ToString()) == 1 });
        num += 2;
        CSS.CameraAdditionalEffect.Add(new SettingValue() { type = CameraEffect.BlackWhite, isOn = CameraMaterial.GetFloat(((SystemName)num).ToString()) == 1 });
        num += 2;
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.ContrastThreshold,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.Posterize,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.Pixelate,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.Vignette,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.EdgeDetect,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.FilmNoise,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });
        CSS.CameraAdditionalEffect.Add(new SettingValue()
        {
            type = CameraEffect.CRTScanline,
            isOn = CameraMaterial.GetFloat(((SystemName)num++).ToString()) == 1,
            NowValue = CameraMaterial.GetFloat(((SystemName)num++).ToString())
        });

        HelpingInMake HM = findHM();

        CSS.IsSubCameraUse.Add(HM.isLight);
        CSS.IsSubCameraUse.Add(HM.isFilter);

        File.WriteAllText(_path, JsonUtility.ToJson(CSS, true));
    }

    void LoadFile(string _path)
    {
        string[] files = Directory.GetFiles(_path, "*.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);

            switch (fileName)
            {
                case "PlayerPos.json":
                    LoadPlayer(file);
                    break;

                case "MainLight.json":
                    LoadLight(file);
                    break;

                case "BlockColors.json":
                    LoadBlockColors(file);
                    break;

                case "PlayerAchive.json":
                    LoadPlayerAchive(file);
                    break;

                case "CameraSetting.json":
                    LoadCameraSetting(file);
                    break;
            }
        }

        GameManager.Instance.MainCamera.stopCamera = false;
    }

    void LoadPlayer(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);

        PlayerPositionSave data = JsonUtility.FromJson<PlayerPositionSave>(json);

        Transform Player = GameManager.Instance.MainCamera.gameObject.transform;

        Player.position = new Vector3(data.x, data.y, data.z);
        Player.rotation = new Quaternion(data.xrot, data.yrot, data.zrot, data.wrot);
    }

    void LoadLight(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);

        MainLightSave data = JsonUtility.FromJson<MainLightSave>(json);

        GameObject MainLight = GameManager.Instance.LightManager.MainLight;
        Light _mainlight = MainLight.GetComponent<Light>();

        LightBlockSaveData LBSD = data.MainLightColor;
        MainLight.transform.rotation = new Quaternion(LBSD.xrot, LBSD.yrot, LBSD.zrot, LBSD.wrot);
        _mainlight.color = new Color(LBSD.r, LBSD.g, LBSD.b);
    }

    void LoadBlockColors(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);

        BlockColorsSave data = JsonUtility.FromJson<BlockColorsSave>(json);

        BuildManager BM = GameManager.Instance.BuildManager;

        BM.MainColor.color = new Color(data.maincolor.r, data.maincolor.g, data.maincolor.b);
        BM.alfar = data.maincolor.a;
        if (data.precolor.Count > 0)
        {
            foreach (ColorSaveData CSD in data.precolor)
            {
                BM.AddPreColor(new Color(CSD.r, CSD.g, CSD.b));
            }
        }
        if (data.savecolor.Count > 0)
        {
            int num = 0;
            foreach (ColorSaveData CSD in data.savecolor)
            {
                BM.AddSaveColor(new Color(CSD.r, CSD.g, CSD.b), num);
                num++;
            }
        }
    }

    void LoadPlayerAchive(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);

        PlayerAchieve data = JsonUtility.FromJson<PlayerAchieve>(json);
        GameManager.Instance.WorldManager.HowManyYouMakeWorld = data.HowManyMakeWorld;
    }

    void LoadCameraSetting(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);

        CameraSettingSave data = JsonUtility.FromJson<CameraSettingSave>(json);
        Material CameraMaterial = GameManager.Instance.MainCamera.gameObject.GetComponent<CameraShaderPutter>().CamMaterial;

        foreach(var SV in data.CameraAdditionalEffect)
        {
            switch (SV.type)
            {
                case CameraEffect.Reverse:
                    CameraMaterial.SetFloat(((SystemName)0).ToString(), SV.isOn ? 1f : 0f);
                    break;

                case CameraEffect.BlackWhite:
                    CameraMaterial.SetFloat(((SystemName)2).ToString(), SV.isOn ? 1f : 0f);
                    break;

                case CameraEffect.ContrastThreshold:
                    CameraMaterial.SetFloat(((SystemName)4).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)5).ToString(), SV.NowValue);
                    break;

                case CameraEffect.Posterize:
                    CameraMaterial.SetFloat(((SystemName)6).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)7).ToString(), SV.NowValue);
                    break;

                case CameraEffect.Pixelate:
                    CameraMaterial.SetFloat(((SystemName)8).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)9).ToString(), SV.NowValue);
                    break;

                case CameraEffect.Vignette:
                    CameraMaterial.SetFloat(((SystemName)10).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)11).ToString(), SV.NowValue);
                    break;

                case CameraEffect.EdgeDetect:
                    CameraMaterial.SetFloat(((SystemName)12).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)13).ToString(), SV.NowValue);
                    break;

                case CameraEffect.FilmNoise:
                    CameraMaterial.SetFloat(((SystemName)14).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)15).ToString(), SV.NowValue);
                    break;

                case CameraEffect.CRTScanline:
                    CameraMaterial.SetFloat(((SystemName)16).ToString(), SV.isOn ? 1f : 0f);
                    CameraMaterial.SetFloat(((SystemName)17).ToString(), SV.NowValue);
                    break;
            }
        }

        HelpingInMake HM = findHM();

        HM.isLight = data.IsSubCameraUse[0];
        HM.isFilter = data.IsSubCameraUse[1];
    }


    HelpingInMake findHM()
    {
        foreach (var find in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if (find.UIenum == SelectionUIenum.HelpingInMake)
            {
                return find.UISelected.GetComponent<HelpingInMake>();
            }
        }

        return null;
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

        string folder = Path.Combine(WorldSaveFolder, $"World_{Time}");
        string path = Path.Combine(folder, "world.json");

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        worldsave.Name = changedName;

        json = JsonUtility.ToJson(worldsave, true);
        File.WriteAllText(path, json);
    }

    public string GetName(string _path)
    {
        EnsureSaveFolder();

        string folder = Path.Combine(WorldSaveFolder, _path);
        string path = Path.Combine(folder, "world.json");

        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        WorldSaveData worldsave = JsonUtility.FromJson<WorldSaveData>(json);

        return worldsave.Name;
    }

    string TryMakePlayerSettingFolder()
    {
        string path = PlayerSettingFolder;
        Directory.CreateDirectory(path);
        return path;
    }

    public void PlayerSetting_SaveKey()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "KeySetting.json");
        KeyManager _key = GameManager.Instance.KeyManager;
        KeySaves keysaves = new KeySaves();

        for (int i = 0; i < _key.KeyboardDictionary.Count; i++)
        {
            keysaves.keys.Add(new KeySave { keyname = (KeyNameSaves)i, keyboard = _key.KeyboardDictionary[(KeyNameSaves)i] });
        }

        File.WriteAllText(path, JsonUtility.ToJson(keysaves, true));
    }

    public void PlayerSetting_SaveSound()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "SoundSetting.json");

        SoundManager soundManager = GameManager.Instance.SoundManager;

        SoundSettingSave soundSave = new SoundSettingSave()
        {
            Main = soundManager.MainSoundVolume,
            Effect = soundManager.EffectVolume,
            BGM = soundManager.BGMVolume
        };

        File.WriteAllText(path, JsonUtility.ToJson(soundSave, true));
    }

    public void PlayerSetting_LoadSound()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "SoundSetting.json");

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SoundSettingSave soundSave = JsonUtility.FromJson<SoundSettingSave>(json);

        SoundManager soundManager = GameManager.Instance.SoundManager;

        soundManager.SetMainVolume(soundSave.Main);
        soundManager.SetEffectVolume(soundSave.Effect);
        soundManager.SetBGMVolume(soundSave.BGM);
    }

    public void PlayerSetting_SaveLanguage()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "LanguageSetting.json");

        TranslationManager translationManager = GameManager.Instance.TranslationManager;

        LanguageSettingSave languageSave = new LanguageSettingSave()
        {
            Language = translationManager.GetLanguage()
        };

        File.WriteAllText(path, JsonUtility.ToJson(languageSave, true));
    }

    public void PlayerSetting_LoadLanguage()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "LanguageSetting.json");

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        LanguageSettingSave languageSave = JsonUtility.FromJson<LanguageSettingSave>(json);

        GameManager.Instance.TranslationManager.ChangeLanguage(languageSave.Language);
    }

    void LoadStartFile(string _path)
    {
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        string[] files = Directory.GetFiles(_path, "*.json"); 
        
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);

            switch (fileName)
            {
                case "KeySetting.json":
                    PlayerSetting_LoadKey(file);
                    break;

                case "SoundSetting.json":
                    PlayerSetting_LoadSound();
                    break;

                case "LanguageSetting.json":
                    PlayerSetting_LoadLanguage();
                    break;
            }
        }
    }

    void PlayerSetting_LoadKey(string _path)
    {
        if (!File.Exists(_path)) return;

        string json = File.ReadAllText(_path);
        KeyManager _key = GameManager.Instance.KeyManager;
        KeySaves keysaves = JsonUtility.FromJson<KeySaves>(json);

        foreach (KeySave save in keysaves.keys)
        {
            _key.KeyboardDictionary[save.keyname] = save.keyboard;
        }
    }

    public bool AwaitForMaking = false;

    public void CreateNewSaveWithDefaultWorld(string saveFolderName, int defaultWorldIndex = 0)
    {
        StartCoroutine(CreateNewSaveWithDefaultWorldCoroutine(saveFolderName, defaultWorldIndex));
    }

    IEnumerator CreateNewSaveWithDefaultWorldCoroutine(string saveFolderName, int defaultWorldIndex)
    {
        AwaitForMaking = false;

        EnsureSaveFolder(saveFolderName);

        yield return StartCoroutine(CopyDefaultWorldToSaveCoroutine(saveFolderName, defaultWorldIndex));

        while(!AwaitForMaking) 
        {
            yield return null;
        }

        GameManager.Instance.UIManager.StartAdditionalUI.SpawnUI(StartUIs.LoadUI);
    }

    public static string GetDefaultWorldTemplateFolder(int defaultWorldIndex)
    {
        defaultWorldIndex = Mathf.Clamp(defaultWorldIndex, 0, 9);

        return Path.Combine(
            Application.streamingAssetsPath,
            "DefaultWorld",
            $"World_Default_{defaultWorldIndex}"
        );
    }

    IEnumerator CopyDefaultWorldToSaveCoroutine(string saveFolderName, int defaultWorldIndex)
    {
        string defaultWorldFolder = GetDefaultWorldTemplateFolder(defaultWorldIndex);

        if (!Directory.Exists(defaultWorldFolder))
        {
            Debug.LogWarning($"[Save] 기본 월드 폴더가 없습니다: {defaultWorldFolder}");
            yield break;
        }

        string targetSaveFolder = Path.Combine(SaveFolder, saveFolderName);

        if (!Directory.Exists(targetSaveFolder))
        {
            Directory.CreateDirectory(targetSaveFolder);
            yield return null;
        }

        string[] templateWorldFolders = Directory.GetDirectories(defaultWorldFolder);

        // 하위 폴더 구조가 없고, 기존처럼 바로 world.json이 있는 경우
        if (File.Exists(Path.Combine(defaultWorldFolder, "world.json")))
        {
            string newWorldTime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            string targetWorldFolder = Path.Combine(targetSaveFolder, "World_" + newWorldTime);

            if (Directory.Exists(targetWorldFolder))
            {
                yield return StartCoroutine(DeleteDirectoryCoroutine(targetWorldFolder));
            }

            yield return StartCoroutine(CopyDirectoryCoroutine(defaultWorldFolder, targetWorldFolder));

            RewriteCopiedWorldInfo(targetWorldFolder, newWorldTime, defaultWorldIndex);

            yield break;
        }

        // World_Default_6 안에 여러 월드 폴더가 있는 경우
        int worldIndex = 0;

        foreach (string templateWorldFolder in templateWorldFolders)
        {
            string worldJsonPath = Path.Combine(templateWorldFolder, "world.json");

            if (!File.Exists(worldJsonPath))
                continue;

            string newWorldTime = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + worldIndex;
            string targetWorldFolder = Path.Combine(targetSaveFolder, "World_" + newWorldTime);

            if (Directory.Exists(targetWorldFolder))
            {
                yield return StartCoroutine(DeleteDirectoryCoroutine(targetWorldFolder));
            }

            yield return StartCoroutine(CopyDirectoryCoroutine(templateWorldFolder, targetWorldFolder));

            RewriteCopiedWorldInfo(targetWorldFolder, newWorldTime, defaultWorldIndex);

            worldIndex++;

            yield return null;
        }
    }

    IEnumerator CopyDirectoryCoroutine(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        string[] files = Directory.GetFiles(sourceDir);
        float start = Time.realtimeSinceStartup;

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);

            File.Copy(file, targetFile, true);

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        string[] directories = Directory.GetDirectories(sourceDir);

        foreach (string directory in directories)
        {
            string directoryName = Path.GetFileName(directory);
            string targetSubDir = Path.Combine(targetDir, directoryName);

            yield return StartCoroutine(CopyDirectoryCoroutine(directory, targetSubDir));

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }
    }

    void RewriteCopiedWorldInfo(string worldFolder, string newWorldTime, int defaultWorldIndex)
    {
        string worldInfoPath = Path.Combine(worldFolder, "world.json");

        if (!File.Exists(worldInfoPath))
        {
            Debug.LogWarning($"[Save] 복사된 기본 월드에 world.json이 없습니다: {worldInfoPath}");
            return;
        }

        string json = File.ReadAllText(worldInfoPath);
        WorldSaveData worldSaveData = JsonUtility.FromJson<WorldSaveData>(json);

        worldSaveData.Time = newWorldTime;

        if (string.IsNullOrEmpty(worldSaveData.Name))
        {
            worldSaveData.Name = $"World_Default_{defaultWorldIndex}";
        }

        json = JsonUtility.ToJson(worldSaveData, true);
        File.WriteAllText(worldInfoPath, json);
    }

    IEnumerator DeleteDirectoryCoroutine(string targetDir)
    {
        if (!Directory.Exists(targetDir))
            yield break;

        string[] files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);

        float start = Time.realtimeSinceStartup;

        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        string[] directories = Directory.GetDirectories(targetDir, "*", SearchOption.AllDirectories);

        for (int i = directories.Length - 1; i >= 0; i--)
        {
            string directory = directories[i];

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, false);
            }

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        if (Directory.Exists(targetDir))
        {
            Directory.Delete(targetDir, false);
        }
    }
}
