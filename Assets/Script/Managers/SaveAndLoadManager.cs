using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class ScreenSettingSave
{
    public int ResolutionIndex = 2;
    public ScreenModeType ScreenMode = ScreenModeType.Windowed;
}

[System.Serializable]
public class SensitiveSettingSave
{
    public float PlayerMove = 1f;
    public float PlayerRotation = 1f;
    public float ObjectMove = 1f;
    public float ObjectRotation = 1f;
    public float ObjectScale = 1f;
}

[System.Serializable]
public class LanguageSettingSave
{
    public LanguageEnum Language;
}

[System.Serializable]
public class CanControlSettingSave
{
    // 기존 파일이 없을 때 기본값은 펼침.
    public bool IsExpanded = true;
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

    HashSet<string> deletedWorlds = new HashSet<string>();

    bool isSaving = false;
    bool isLoading = false;
    bool cancelLoading = false;

    Coroutine loadWorldFolderCoroutine;

    List<GameObject> worldsCreatedDuringLoad =
        new List<GameObject>();

    public bool IsLoading => isLoading;
    public bool IsSaving => isSaving;
    public bool IsCreatingNewSave => isCreatingNewSave;

    bool isCreatingNewSave = false;

    Vector3 defaultPlayerPosition = Vector3.zero;
    Quaternion defaultPlayerRotation = Quaternion.identity;
    bool hasDefaultPlayerTransform = false;

    public static bool HasActiveSaveFolder =>
        GameManager.Instance != null &&
        !string.IsNullOrWhiteSpace(
            GameManager.Instance.NowWorldSaveFolder
        );

    public static bool TryParseSaveFolderName(
        string folderName,
        out string saveName,
        out string saveTime,
        out int iconIndex,
        out int tapeIndex)
    {
        saveName = "";
        saveTime = "";
        iconIndex = -1;
        tapeIndex = -1;

        if (string.IsNullOrWhiteSpace(folderName))
            return false;

        string[] parts =
            folderName.Split('_');

        // 이름에 '_'가 포함된 예전 세이브도 허용하고,
        // 뒤의 3개 항목을 시간 / 아이콘 / 테이프 번호로 해석한다.
        if (parts.Length < 4)
            return false;

        int last = parts.Length - 1;

        saveTime = parts[last - 2];

        if (saveTime.Length != 14)
            return false;

        long timeNumber;

        if (!long.TryParse(saveTime, out timeNumber))
            return false;

        if (!int.TryParse(parts[last - 1], out iconIndex) ||
            iconIndex < 0)
        {
            return false;
        }

        if (!int.TryParse(parts[last], out tapeIndex) ||
            tapeIndex < 0)
        {
            return false;
        }

        saveName =
            string.Join(
                "_",
                parts.Take(parts.Length - 3).ToArray()
            );

        return !string.IsNullOrWhiteSpace(saveName);
    }

    public static bool IsValidSaveFolderName(
        string folderName)
    {
        string saveName;
        string saveTime;
        int iconIndex;
        int tapeIndex;

        return TryParseSaveFolderName(
            folderName,
            out saveName,
            out saveTime,
            out iconIndex,
            out tapeIndex
        );
    }

    public static void EnsureSaveFolder(string _saveFolderName)
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }

        if (string.IsNullOrWhiteSpace(_saveFolderName))
            return;

        if (!IsValidSaveFolderName(_saveFolderName))
        {
            Debug.LogWarning(
                "[Save] 올바르지 않은 세이브 폴더 이름입니다: " +
                _saveFolderName
            );
            return;
        }

        string _worldSaveFolder =
            System.IO.Path.Combine(
                SaveFolder,
                _saveFolderName
            );

        string _worldPlayerSetting =
            System.IO.Path.Combine(
                _worldSaveFolder,
                "WorldPlayerSetting"
            );

        Directory.CreateDirectory(_worldSaveFolder);
        Directory.CreateDirectory(_worldPlayerSetting);
    }

    public static void EnsureSaveFolder()
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
            Debug.Log($"[Save] 폴더 생성: {SaveFolder}");
        }

        if (!HasActiveSaveFolder)
            return;

        if (!IsValidSaveFolderName(
            GameManager.Instance.NowWorldSaveFolder))
        {
            Debug.LogWarning(
                "[Save] 현재 세이브 폴더 이름이 올바르지 않습니다: " +
                GameManager.Instance.NowWorldSaveFolder
            );
            return;
        }

        Directory.CreateDirectory(WorldSaveFolder);
        Directory.CreateDirectory(WorldPlayerSetting);
    }

    private void Start()
    {
        CaptureDefaultPlayerTransform();

        if (Directory.Exists(PlayerSettingFolder))
        {
            LoadStartFile(PlayerSettingFolder);
        }
    }

    void CaptureDefaultPlayerTransform()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.MainCamera == null)
        {
            return;
        }

        Transform player =
            GameManager.Instance.MainCamera.transform;

        defaultPlayerPosition = player.position;
        defaultPlayerRotation = player.rotation;
        hasDefaultPlayerTransform = true;
    }

    void RestoreDefaultPlayerTransform()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.MainCamera == null)
        {
            return;
        }

        if (!hasDefaultPlayerTransform)
            CaptureDefaultPlayerTransform();

        Transform player =
            GameManager.Instance.MainCamera.transform;

        player.position =
            hasDefaultPlayerTransform
            ? defaultPlayerPosition
            : Vector3.zero;

        player.rotation =
            hasDefaultPlayerTransform
            ? defaultPlayerRotation
            : Quaternion.identity;

        GameManager.Instance.MainCamera.TryCheckrot();
        GameManager.Instance.MainCamera.SavePos();
    }

    public void CancelLoadWorldFolder()
    {
        CancelLoading();
    }

    public void LoadWorldFolder()
    {
        // 이미 로드 중이라면 중복 로드 금지
        if (isLoading)
            return;

        if (!HasActiveSaveFolder)
        {
            Debug.LogWarning(
                "[Load] 선택된 세이브 폴더가 없습니다."
            );
            return;
        }

        if (!IsValidSaveFolderName(
            GameManager.Instance.NowWorldSaveFolder))
        {
            Debug.LogWarning(
                "[Load] 올바르지 않은 세이브 폴더 이름입니다: " +
                GameManager.Instance.NowWorldSaveFolder
            );
            return;
        }

        if (!Directory.Exists(WorldSaveFolder))
        {
            Debug.LogWarning(
                "[Load] 세이브 폴더가 존재하지 않습니다: " +
                WorldSaveFolder
            );
            return;
        }

        loadWorldFolderCoroutine =
            StartCoroutine(LoadWorldFolderCoroutine());
    }


    IEnumerator LoadWorldFolderCoroutine()
    {
        isLoading = true;
        cancelLoading = false;

        worldsCreatedDuringLoad.Clear();

        EnsureSaveFolder();

        Debug.Log(
            "[LoadTrace] LoadStart frame=" + Time.frameCount +
            " time=" + Time.realtimeSinceStartup +
            " save=" + GameManager.Instance.NowWorldSaveFolder +
            " path=" + WorldSaveFolder
        );


        // =====================================================
        // 월드 설정 불러오기
        // =====================================================

        GameManager.Instance.UIManager.StartAdditionalUI
            .ChangeCreationState(CreateState.Setting);

        // 이전 세이브의 Runtime 데이터가 새 세이브에 섞이지 않도록
        // 먼저 런타임 상태를 초기화한다.
        ResetWorldPlayerRuntime();

        if (
            Directory.Exists(WorldPlayerSetting) &&
            Directory.EnumerateFiles(WorldPlayerSetting).Any()
        )
        {
            LoadFile(WorldPlayerSetting);
        }
        else
        {
            RestoreDefaultPlayerTransform();
        }


        // =====================================================
        // 유효한 World 폴더 찾기
        // =====================================================

        string[] worldFolders =
            Directory.GetDirectories(WorldSaveFolder);

        List<string> validWorlds =
            new List<string>();


        foreach (string folder in worldFolders)
        {
            string folderName =
                Path.GetFileName(folder);

            if (!folderName.StartsWith("World_"))
                continue;


            string worldInfoPath =
                Path.Combine(
                    folder,
                    "world.json"
                );


            if (File.Exists(worldInfoPath))
            {
                validWorlds.Add(folder);
            }
        }


        Debug.Log(
            "[LoadTrace] WorldFolderScan frame=" + Time.frameCount +
            " total=" + worldFolders.Length +
            " valid=" + validWorlds.Count
        );

        // =====================================================
        // 모든 World의 Chunk 데이터를 먼저 읽는다
        // =====================================================

        foreach (string folder in validWorlds)
        {
            if (cancelLoading)
            {
                CleanupCancelledLoadedWorlds();
                FinishLoadingState();
                yield break;
            }


            // 중요
            // LoadWorldData가 끝날 때까지 기다림
            yield return StartCoroutine(
                LoadWorldData(folder)
            );


            if (cancelLoading)
            {
                CleanupCancelledLoadedWorlds();
                FinishLoadingState();
                yield break;
            }
        }


        // =====================================================
        // 여기까지 왔으면 모든 MakeWorldQueue가 준비됨
        // =====================================================

        if (cancelLoading)
        {
            CleanupCancelledLoadedWorlds();
            FinishLoadingState();
            yield break;
        }


        GameManager.Instance.UIManager.StartAdditionalUI
            .ChangeCreationState(CreateState.Making);


        // =====================================================
        // 이제 실제 월드 제작 시작
        // =====================================================

        yield return StartCoroutine(
            GameManager.Instance.WorldManager
                .LoadAllWorldCoroutine(
                    () => cancelLoading
                )
        );


        // 제작 중 취소됐을 경우
        if (cancelLoading)
        {
            CleanupCancelledLoadedWorlds();
            FinishLoadingState();
            yield break;
        }


        // 정상적으로 모두 로드 완료
        worldsCreatedDuringLoad.Clear();

        FinishLoadingState();
    }

    void CleanupCancelledLoadedWorlds()
    {
        for (
            int i = worldsCreatedDuringLoad.Count - 1;
            i >= 0;
            i--
        )
        {
            GameObject world =
                worldsCreatedDuringLoad[i];


            if (world == null)
                continue;


            // WorldManager 목록에서는 제거
            GameManager.Instance.WorldManager.Worlds
                .Remove(world);


            // GameObject만 제거
            Destroy(world);
        }


        worldsCreatedDuringLoad.Clear();
    }

    void FinishLoadingState()
    {
        isLoading = false;
        cancelLoading = false;
        loadWorldFolderCoroutine = null;
    }

    string GetWorldFolder(string time)
    {
        return Path.Combine(WorldSaveFolder, "World_" + time);
    }

    public void SaveWorldData(ChunkCode _world)
    {
        if (_world == null)
            return;

        if (!HasActiveSaveFolder)
            return;

        if (deletedWorlds.Contains(_world.Savedtime))
            return;

        _world.isSavedBefore = false;

        if (isSaving && currentSavingWorld == _world)
        {
            dirtyWorlds.Add(_world);
            return;
        }

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
            if (world != null)
            {
                string worldID = world.Savedtime;

                if (!deletedWorlds.Contains(worldID))
                {
                    if (dirtyWorlds.Contains(world))
                    {
                        dirtyWorlds.Remove(world);

                        saveQueue.Enqueue(world);
                        queuedWorlds.Add(world);
                    }
                }
                else
                {
                    dirtyWorlds.Remove(world);
                    queuedWorlds.Remove(world);
                }
            }
        }

        currentSavingWorld = null;
        isSaving = false;
        GameManager.Instance.UIManager.SavingEffectUI.EndSaving();
    }

    public IEnumerator WaitForPendingWorldSaves()
    {
        // SaveWorldData starts SaveWorker as a coroutine, so allow it to enter first.
        yield return null;

        while (isSaving || saveQueue.Count > 0)
        {
            yield return null;
        }
    }

    IEnumerator SaveWorldCoroutine(ChunkCode _world)
    {
        EnsureSaveFolder();

        SaveWorldInfo(_world);

        yield return StartCoroutine(SaveChunksCoroutine(_world));
    }

    public void SaveWorldInfo(ChunkCode world)
    {
        if (world == null ||
            !HasActiveSaveFolder)
        {
            return;
        }

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
        if (world == null)
            yield break;

        string worldID = world.Savedtime;

        // 이미 삭제된 월드라면 저장하지 않음
        if (deletedWorlds.Contains(worldID))
            yield break;

        string folder = GetWorldFolder(worldID);

        // 폴더가 없다면 생성
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }


        // =====================================================
        // 저장 시작 시점의 Chunk 좌표만 복사
        // =====================================================

        List<Vector3Int> chunkPositions =
            world.BlocksinChunk.Keys.ToList();


        // =====================================================
        // 현재 존재하는 Chunk 파일 목록 제작
        // =====================================================

        HashSet<string> currentChunkFiles =
            new HashSet<string>();

        foreach (Vector3Int chunkPos in chunkPositions)
        {
            string fileName =
                $"chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}.bin";

            currentChunkFiles.Add(fileName);
        }


        // 삭제 작업이 들어왔는지 확인
        if (deletedWorlds.Contains(worldID))
            yield break;


        // =====================================================
        // 더 이상 존재하지 않는 예전 Chunk 파일 삭제
        // =====================================================

        string[] oldChunkFiles =
            Directory.GetFiles(
                folder,
                "chunk_*.bin"
            );

        foreach (string oldFile in oldChunkFiles)
        {
            if (deletedWorlds.Contains(worldID))
                yield break;

            string fileName =
                Path.GetFileName(oldFile);

            if (!currentChunkFiles.Contains(fileName))
            {
                if (File.Exists(oldFile))
                {
                    File.Delete(oldFile);
                }
            }
        }


        // =====================================================
        // 저장 시작 시 존재했던 Chunk 전체 저장
        // =====================================================

        float start =
            Time.realtimeSinceStartup;

        foreach (Vector3Int chunkPos in chunkPositions)
        {
            // 저장 도중 월드 자체가 삭제됐는지 확인
            if (deletedWorlds.Contains(worldID))
            {
                yield break;
            }


            // -----------------------------------------
            // 저장 도중 해당 Chunk가 삭제될 수도 있음
            // -----------------------------------------

            if (!world.BlocksinChunk.TryGetValue(
                chunkPos,
                out Dictionary<Vector3Int, BlockInChunkData> blocks))
            {
                // 저장 시작 당시에는 있었는데
                // 현재는 Chunk가 사라진 경우

                string oldFile =
                    Path.Combine(
                        folder,
                        $"chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}.bin"
                    );

                if (File.Exists(oldFile))
                {
                    File.Delete(oldFile);
                }

                continue;
            }


            // -----------------------------------------
            // 현재 존재하는 Chunk 저장
            // -----------------------------------------

            SaveChunk(
                chunkPos,
                blocks,
                folder
            );


            // -----------------------------------------
            // 프레임 부하 분산
            // -----------------------------------------

            if (
                (Time.realtimeSinceStartup - start) * 1000f
                > 5f
            )
            {
                yield return null;

                start =
                    Time.realtimeSinceStartup;
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
        if (_world == null)
            return;

        if (!HasActiveSaveFolder)
            return;

        EnsureSaveFolder();

        string worldID = _world.Savedtime;

        // 이 월드는 이제 저장되면 안 됨
        deletedWorlds.Add(worldID);

        // dirty 목록 제거
        dirtyWorlds.Remove(_world);

        // queued 표시 제거
        queuedWorlds.Remove(_world);


        // =========================
        // Save Queue에서 제거
        // =========================

        if (saveQueue.Count > 0)
        {
            Queue<ChunkCode> newQueue =
                new Queue<ChunkCode>();

            while (saveQueue.Count > 0)
            {
                ChunkCode world = saveQueue.Dequeue();

                if (world == null)
                    continue;

                if (world == _world)
                    continue;

                newQueue.Enqueue(world);
            }

            saveQueue = newQueue;
        }


        // =========================
        // 실제 저장 폴더 삭제
        // =========================

        string folder =
            Path.Combine(
                WorldSaveFolder,
                $"World_{worldID}"
            );

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

        if (!IsValidSaveFolderName(saveFolderName))
        {
            Debug.LogWarning(
                "[Save] 올바르지 않은 세이브 폴더는 삭제하지 않습니다: " +
                saveFolderName
            );
            return;
        }

        string folder =
            Path.Combine(
                SaveFolder,
                saveFolderName
            );

        if (!Directory.Exists(folder))
        {
            Debug.LogWarning($"[Save] 삭제할 세이브 폴더가 없습니다: {folder}");
            return;
        }

        Directory.Delete(folder, true);

        Debug.Log($"[Save] 세이브 폴더 삭제 완료: {folder}");
    }

    IEnumerator LoadWorldData(string folder)
    {
        if (cancelLoading)
            yield break;


        string worldInfoPath =
            Path.Combine(
                folder,
                "world.json"
            );

        WorldSaveData worldsave;

        if (!TryReadJson(
            worldInfoPath,
            out worldsave))
        {
            Debug.LogWarning(
                "[Load] world.json을 읽지 못해 World 로드를 건너뜁니다: " +
                worldInfoPath
            );
            yield break;
        }

        if (cancelLoading)
            yield break;


        // =====================================================
        // World GameObject 생성
        // =====================================================

        GameObject world =
            GameManager.Instance.WorldManager.MakeWorld();


        // 취소 시 제거할 수 있도록 기억
        worldsCreatedDuringLoad.Add(world);

        Debug.Log(
            "[LoadTrace] WorldShellCreated frame=" + Time.frameCount +
            " folder=" + Path.GetFileName(folder)
        );


        Vector3 loadedWorldPosition =
            new Vector3(
                worldsave.x,
                worldsave.y,
                worldsave.z
            );

        world.transform.position =
            IsFiniteVector3(loadedWorldPosition)
            ? loadedWorldPosition
            : Vector3.zero;

        Quaternion loadedWorldRotation;

        world.transform.rotation =
            TryCreateValidQuaternion(
                worldsave.xrot,
                worldsave.yrot,
                worldsave.zrot,
                worldsave.wrot,
                out loadedWorldRotation)
            ? loadedWorldRotation
            : Quaternion.identity;

        float loadedWorldScale =
            IsFinite(worldsave.size) &&
            worldsave.size > 0.0001f
            ? worldsave.size
            : 1f;

        world.transform.localScale =
            Vector3.one * loadedWorldScale;


        ChunkCode worldCode =
            world.GetComponent<ChunkCode>();


        worldCode._name =
            worldsave.Name;

        worldCode.Savedtime =
            worldsave.Time;

        worldCode.isSavedBefore =
            true;


        // =====================================================
        // Light 복원
        // =====================================================

        if (worldsave.isLightBlock &&
            worldsave.lightblock != null)
        {
            Light _light =
                worldCode.ChangeLightPower();

            if (_light != null)
            {
                Vector3 loadedLightPosition =
                    new Vector3(
                        worldsave.lightblock.x,
                        worldsave.lightblock.y,
                        worldsave.lightblock.z
                    );

                if (IsFiniteVector3(loadedLightPosition))
                {
                    _light.gameObject.transform.position =
                        loadedLightPosition;
                }

                Quaternion loadedLightRotation;

                if (TryCreateValidQuaternion(
                    worldsave.lightblock.xrot,
                    worldsave.lightblock.yrot,
                    worldsave.lightblock.zrot,
                    worldsave.lightblock.wrot,
                    out loadedLightRotation))
                {
                    _light.gameObject.transform.rotation =
                        loadedLightRotation;
                }

                _light.color =
                    new Color(
                        worldsave.lightblock.r,
                        worldsave.lightblock.g,
                        worldsave.lightblock.b
                    );

                _light.type =
                    worldsave.lightblock.type == 1
                    ? LightType.Spot
                    : LightType.Point;

                _light.spotAngle =
                    worldsave.lightblock.SpotAngle;

                _light.range =
                    worldsave.lightblock.Range;

                _light.intensity =
                    worldsave.lightblock.Intensive;
            }
        }


        if (cancelLoading)
            yield break;


        // =====================================================
        // 이 World의 모든 Chunk 파일 읽기가 끝날 때까지 기다림
        // =====================================================

        yield return StartCoroutine(
            LoadChunksCoroutine(
                folder,
                worldCode
            )
        );

        int loadedChunkCount =
            worldCode.BlocksinChunk != null
            ? worldCode.BlocksinChunk.Count
            : 0;

        int loadedBlockCount = 0;

        if (worldCode.BlocksinChunk != null)
        {
            foreach (var loadedChunk in worldCode.BlocksinChunk)
            {
                if (loadedChunk.Value != null)
                    loadedBlockCount += loadedChunk.Value.Count;
            }
        }

        Debug.Log(
            "[LoadTrace] WorldDataReady frame=" + Time.frameCount +
            " folder=" + Path.GetFileName(folder) +
            " chunks=" + loadedChunkCount +
            " blocks=" + loadedBlockCount
        );
    }

    IEnumerator LoadChunksCoroutine(string folder, ChunkCode world)
    {
        float loadStartTime = Time.realtimeSinceStartup;
        bool slowLoadMessageShown = false;

        float fileListStart = Time.realtimeSinceStartup;

        string[] files =
            Directory.GetFiles(
                folder,
                "chunk_*.bin"
            );

        float fileListMs =
            (Time.realtimeSinceStartup - fileListStart) * 1000f;

        Debug.Log(
            "[LoadTiming] FileList folder=" +
            Path.GetFileName(folder) +
            " files=" +
            files.Length +
            " ms=" +
            fileListMs.ToString("F2")
        );

        int loadedFileCount = 0;
        int skippedFileCount = 0;
        int emptyFileCount = 0;
        int loadedBlockCount = 0;

        Debug.Log(
            "[LoadTrace] ChunkScan frame=" +
            Time.frameCount +
            " folder=" +
            Path.GetFileName(folder) +
            " files=" +
            files.Length
        );

        if (files.Length == 0)
        {
            Debug.LogWarning(
                "[LoadTrace] NoChunkFiles folder=" +
                folder
            );
        }

        float start =
            Time.realtimeSinceStartup;

        bool firstFileTimingLogged = false;

        foreach (string file in files)
        {
            if (cancelLoading)
                yield break;

            float fileStart =
                Time.realtimeSinceStartup;

            string fileName =
                Path.GetFileNameWithoutExtension(file);

            string[] parts =
                fileName.Split('_');

            if (parts.Length != 4)
            {
                skippedFileCount++;

                Debug.LogWarning(
                    "[LoadTrace] SkipChunkName file=" +
                    fileName
                );

                continue;
            }

            if (
                !int.TryParse(
                    parts[1],
                    out int chunkX
                ) ||
                !int.TryParse(
                    parts[2],
                    out int chunkY
                ) ||
                !int.TryParse(
                    parts[3],
                    out int chunkZ
                )
            )
            {
                skippedFileCount++;

                Debug.LogWarning(
                    "[LoadTrace] SkipChunkCoord file=" +
                    fileName
                );

                continue;
            }

            Vector3Int chunkPos =
                new Vector3Int(
                    chunkX,
                    chunkY,
                    chunkZ
                );

            float prepareMs = 0f;
            int count = 0;

            using (
                BinaryReader reader =
                    new BinaryReader(
                        File.Open(
                            file,
                            FileMode.Open
                        )
                    )
            )
            {
                count =
                    reader.ReadInt32();

                loadedFileCount++;
                loadedBlockCount += count;

                if (count <= 0)
                    emptyFileCount++;

                float prepareStart =
                    Time.realtimeSinceStartup;

                Dictionary<
                    Vector3Int,
                    BlockInChunkData
                > blocks =
                    world.PrepareLoadChunk(
                        chunkPos,
                        count
                    );

                prepareMs =
                    (
                        Time.realtimeSinceStartup -
                        prepareStart
                    ) * 1000f;

                for (int i = 0; i < count; i++)
                {
                    if (cancelLoading)
                        yield break;

                    int x =
                        reader.ReadInt32();

                    int y =
                        reader.ReadInt32();

                    int z =
                        reader.ReadInt32();

                    float r =
                        reader.ReadSingle();

                    float g =
                        reader.ReadSingle();

                    float b =
                        reader.ReadSingle();

                    float a =
                        reader.ReadSingle();

                    world.LoadPreparedChunkBlockData(
                        blocks,
                        chunkPos,
                        new Vector3Int(
                            x,
                            y,
                            z
                        ),
                        new Color(
                            r,
                            g,
                            b,
                            a
                        )
                    );
                }
            }

            float fileLoadMs =
                (
                    Time.realtimeSinceStartup -
                    fileStart
                ) * 1000f;

            if (
                !firstFileTimingLogged ||
                fileLoadMs >= 50f
            )
            {
                Debug.Log(
                    "[LoadTiming] File chunk=" +
                    chunkPos +
                    " blocks=" +
                    count +
                    " prepareMs=" +
                    prepareMs.ToString("F2") +
                    " fileLoadMs=" +
                    fileLoadMs.ToString("F2")
                );

                firstFileTimingLogged = true;
            }

            // Show this only when loading is actually taking a long time.
            if (
                !slowLoadMessageShown &&
                Time.realtimeSinceStartup -
                loadStartTime >= 7f
            )
            {
                slowLoadMessageShown = true;

                GameManager.Instance.UIManager.StartSceneUI
                    .AddText("SlowWorldLoad");
            }

            if (
                (Time.realtimeSinceStartup - start) *
                1000f >
                20f
            )
            {
                yield return null;

                start =
                    Time.realtimeSinceStartup;
            }
        }

        Debug.Log(
            "[LoadTrace] ChunkScanEnd frame=" +
            Time.frameCount +
            " folder=" +
            Path.GetFileName(folder) +
            " found=" +
            files.Length +
            " loaded=" +
            loadedFileCount +
            " skipped=" +
            skippedFileCount +
            " empty=" +
            emptyFileCount +
            " blocks=" +
            loadedBlockCount
        );
    }

    public void CancelLoading()
    {
        if (!isLoading)
            return;

        cancelLoading = true;

        Debug.Log("로드 취소 요청");
    }

    public void SaveAll()
    {
        if (!HasActiveSaveFolder)
            return;

        if (!IsValidSaveFolderName(
            GameManager.Instance.NowWorldSaveFolder))
        {
            Debug.LogWarning(
                "[Save] 올바르지 않은 현재 세이브 폴더입니다: " +
                GameManager.Instance.NowWorldSaveFolder
            );
            return;
        }

        PlayerPosSave();
        SaveMainLight();
        SaveBlockColors();
        SavePlayerAchive();
        SaveCameraSetting();
    }

    string TryMakeDataFolder()
    {
        if (!HasActiveSaveFolder)
            return null;

        if (!IsValidSaveFolderName(
            GameManager.Instance.NowWorldSaveFolder))
        {
            return null;
        }

        string path = WorldPlayerSetting;
        Directory.CreateDirectory(path);
        return path;
    }

    public void PlayerPosSave()
    {
        string _path = TryMakeDataFolder();

        if (string.IsNullOrEmpty(_path))
            return;

        _path = Path.Combine(_path, "PlayerPos.json");

        if (GameManager.Instance.MainCamera == null)
            return;

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

        if (string.IsNullOrEmpty(_path))
            return;

        _path = Path.Combine(_path, "MainLight.json");

        if (GameManager.Instance.LightManager == null ||
            GameManager.Instance.LightManager.MainLight == null)
        {
            return;
        }

        GameObject MainLight =
            GameManager.Instance.LightManager.MainLight;

        Light _mainlight =
            MainLight.GetComponent<Light>();

        if (_mainlight == null)
            return;

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

        if (string.IsNullOrEmpty(_path))
            return;

        _path = Path.Combine(_path, "BlockColors.json");

        BuildManager BM = GameManager.Instance.BuildManager;

        if (BM == null ||
            BM.MainColor == null)
        {
            return;
        }

        if (BM.PreColors == null)
            BM.PreColors = new List<Color>();

        if (BM.SaveColors == null)
            BM.SaveColors = new List<Color>();

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

        if (string.IsNullOrEmpty(_path))
            return;

        _path = Path.Combine(_path, "PlayerAchive.json");

        if (GameManager.Instance.WorldManager == null)
            return;

        PlayerAchieve PA = new PlayerAchieve()
        {
            HowManyMakeWorld = GameManager.Instance.WorldManager.HowManyYouMakeWorld
        };

        File.WriteAllText(_path, JsonUtility.ToJson(PA, true));
    }

    public void SaveCameraSetting()
    {
        string _path = TryMakeDataFolder();

        if (string.IsNullOrEmpty(_path))
            return;

        _path = Path.Combine(_path, "CameraSetting.json");

        CameraShaderPutter cameraShader =
            GameManager.Instance.MainCamera != null
            ? GameManager.Instance.MainCamera
                .GetComponent<CameraShaderPutter>()
            : null;

        if (cameraShader == null ||
            cameraShader.CamMaterial == null)
        {
            return;
        }

        Material CameraMaterial =
            cameraShader.CamMaterial;

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

        if (HM != null)
        {
            CSS.IsSubCameraUse.Add(HM.isLight);
            CSS.IsSubCameraUse.Add(HM.isFilter);
        }
        else
        {
            CSS.IsSubCameraUse.Add(false);
            CSS.IsSubCameraUse.Add(false);
        }

        File.WriteAllText(_path, JsonUtility.ToJson(CSS, true));
    }

    void LoadFile(string _path)
    {
        if (!Directory.Exists(_path))
        {
            RestoreDefaultPlayerTransform();
            return;
        }

        bool playerLoaded = false;

        string[] files =
            Directory.GetFiles(_path, "*.json");

        foreach (string file in files)
        {
            string fileName =
                Path.GetFileName(file);

            switch (fileName)
            {
                case "PlayerPos.json":
                    playerLoaded = LoadPlayer(file);
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

        if (!playerLoaded)
        {
            Debug.LogWarning(
                "[Load] PlayerPos.json이 없거나 올바르지 않아 " +
                "기본 플레이어 위치를 사용합니다."
            );

            RestoreDefaultPlayerTransform();
        }

        if (GameManager.Instance.MainCamera != null)
        {
            GameManager.Instance.MainCamera.stopCamera = false;
        }
    }

    bool LoadPlayer(string _path)
    {
        PlayerPositionSave data;

        if (!TryReadJson(_path, out data))
            return false;

        Vector3 loadedPosition =
            new Vector3(
                data.x,
                data.y,
                data.z
            );

        if (!IsFiniteVector3(loadedPosition))
        {
            Debug.LogWarning(
                "[Load] PlayerPos 위치 값이 올바르지 않습니다: " +
                _path
            );
            return false;
        }

        Quaternion loadedRotation;

        if (!TryCreateValidQuaternion(
            data.xrot,
            data.yrot,
            data.zrot,
            data.wrot,
            out loadedRotation))
        {
            Debug.LogWarning(
                "[Load] PlayerPos 회전 값이 올바르지 않아 " +
                "기본 회전을 사용합니다: " +
                _path
            );

            loadedRotation =
                hasDefaultPlayerTransform
                ? defaultPlayerRotation
                : Quaternion.identity;
        }

        MainCamera mainCamera =
            GameManager.Instance.MainCamera;

        if (mainCamera == null)
            return false;

        Transform player =
            mainCamera.transform;

        player.position = loadedPosition;
        player.rotation = loadedRotation;

        mainCamera.TryCheckrot();
        mainCamera.SavePos();

        return true;
    }

    void LoadLight(string _path)
    {
        MainLightSave data;

        if (!TryReadJson(_path, out data) ||
            data.MainLightColor == null)
        {
            return;
        }

        GameObject mainLightObject =
            GameManager.Instance.LightManager != null
            ? GameManager.Instance.LightManager.MainLight
            : null;

        if (mainLightObject == null)
            return;

        Light mainLight =
            mainLightObject.GetComponent<Light>();

        if (mainLight == null)
            return;

        LightBlockSaveData saved =
            data.MainLightColor;

        Quaternion loadedRotation;

        if (TryCreateValidQuaternion(
            saved.xrot,
            saved.yrot,
            saved.zrot,
            saved.wrot,
            out loadedRotation))
        {
            mainLightObject.transform.rotation =
                loadedRotation;
        }

        if (IsFinite(saved.r) &&
            IsFinite(saved.g) &&
            IsFinite(saved.b))
        {
            mainLight.color =
                new Color(
                    saved.r,
                    saved.g,
                    saved.b
                );
        }
    }

    void LoadBlockColors(string _path)
    {
        BlockColorsSave data;

        if (!TryReadJson(_path, out data))
            return;

        BuildManager BM =
            GameManager.Instance.BuildManager;

        if (BM == null)
            return;

        if (BM.PreColors == null)
            BM.PreColors = new List<Color>();

        if (BM.SaveColors == null)
            BM.SaveColors = new List<Color>();

        BM.PreColors.Clear();
        BM.SaveColors.Clear();

        if (data.maincolor != null &&
            BM.MainColor != null)
        {
            BM.MainColor.color =
                new Color(
                    data.maincolor.r,
                    data.maincolor.g,
                    data.maincolor.b
                );

            BM.alfar =
                Mathf.Clamp01(
                    data.maincolor.a
                );
        }

        if (data.precolor != null)
        {
            foreach (ColorSaveData CSD in data.precolor)
            {
                if (CSD == null)
                    continue;

                BM.PreColors.Add(
                    new Color(
                        CSD.r,
                        CSD.g,
                        CSD.b,
                        CSD.a
                    )
                );

                if (BM.PreColors.Count >= BM.precolornum)
                    break;
            }
        }

        if (data.savecolor != null)
        {
            foreach (ColorSaveData CSD in data.savecolor)
            {
                if (CSD == null)
                    continue;

                BM.SaveColors.Add(
                    new Color(
                        CSD.r,
                        CSD.g,
                        CSD.b,
                        CSD.a
                    )
                );

                if (BM.SaveColors.Count >= BM.savecolornum)
                    break;
            }
        }

        while (BM.SaveColors.Count < BM.savecolornum)
        {
            BM.SaveColors.Add(new Color(255f, 255f, 255f, 1f));
        }
    }

    void LoadPlayerAchive(string _path)
    {
        PlayerAchieve data;

        if (!TryReadJson(_path, out data))
            return;

        GameManager.Instance.WorldManager
            .HowManyYouMakeWorld =
            Mathf.Max(
                0,
                data.HowManyMakeWorld
            );
    }

    void LoadCameraSetting(string _path)
    {
        CameraSettingSave data;

        if (!TryReadJson(_path, out data))
            return;

        CameraShaderPutter cameraShader =
            GameManager.Instance.MainCamera != null
            ? GameManager.Instance.MainCamera
                .GetComponent<CameraShaderPutter>()
            : null;

        if (cameraShader == null ||
            cameraShader.CamMaterial == null)
        {
            return;
        }

        Material CameraMaterial =
            cameraShader.CamMaterial;

        if (data.CameraAdditionalEffect != null)
        {
            foreach (SettingValue SV in data.CameraAdditionalEffect)
            {
                if (SV == null)
                    continue;

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
        }

        HelpingInMake HM = findHM();

        if (HM != null &&
            data.IsSubCameraUse != null)
        {
            if (data.IsSubCameraUse.Count > 0)
                HM.isLight = data.IsSubCameraUse[0];

            if (data.IsSubCameraUse.Count > 1)
                HM.isFilter = data.IsSubCameraUse[1];
        }
    }

    bool TryReadJson<T>(
        string path,
        out T data)
        where T : class
    {
        data = null;

        if (!File.Exists(path))
            return false;

        try
        {
            string json =
                File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
                return false;

            data =
                JsonUtility.FromJson<T>(json);

            return data != null;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "[Load] JSON 읽기 실패: " +
                path +
                "\n" +
                ex.Message
            );

            return false;
        }
    }

    static bool IsFinite(float value)
    {
        return
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
    }

    static bool IsFiniteVector3(Vector3 value)
    {
        return
            IsFinite(value.x) &&
            IsFinite(value.y) &&
            IsFinite(value.z);
    }

    static bool TryCreateValidQuaternion(
        float x,
        float y,
        float z,
        float w,
        out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        if (!IsFinite(x) ||
            !IsFinite(y) ||
            !IsFinite(z) ||
            !IsFinite(w))
        {
            return false;
        }

        float magnitudeSquared =
            x * x +
            y * y +
            z * z +
            w * w;

        if (magnitudeSquared < 0.000001f)
            return false;

        float inverseMagnitude =
            1f / Mathf.Sqrt(magnitudeSquared);

        rotation =
            new Quaternion(
                x * inverseMagnitude,
                y * inverseMagnitude,
                z * inverseMagnitude,
                w * inverseMagnitude
            );

        return true;
    }

    bool IsValidPlayerPositionData(
        PlayerPositionSave data)
    {
        if (data == null)
            return false;

        if (!IsFiniteVector3(
            new Vector3(
                data.x,
                data.y,
                data.z
            )))
        {
            return false;
        }

        Quaternion rotation;

        return TryCreateValidQuaternion(
            data.xrot,
            data.yrot,
            data.zrot,
            data.wrot,
            out rotation
        );
    }

    void EnsureDefaultPlayerPositionFile(
        string targetFolder)
    {
        if (string.IsNullOrEmpty(targetFolder))
            return;

        Directory.CreateDirectory(targetFolder);

        string playerPosPath =
            Path.Combine(
                targetFolder,
                "PlayerPos.json"
            );

        PlayerPositionSave existingData;

        if (TryReadJson(
            playerPosPath,
            out existingData) &&
            IsValidPlayerPositionData(existingData))
        {
            return;
        }

        WriteDefaultPlayerPositionFile(
            playerPosPath
        );
    }

    void WriteDefaultPlayerPositionFile(
        string playerPosPath)
    {
        if (!hasDefaultPlayerTransform)
            CaptureDefaultPlayerTransform();

        Vector3 position =
            hasDefaultPlayerTransform
            ? defaultPlayerPosition
            : Vector3.zero;

        Quaternion rotation =
            hasDefaultPlayerTransform
            ? defaultPlayerRotation
            : Quaternion.identity;

        PlayerPositionSave data =
            new PlayerPositionSave()
            {
                x = position.x,
                y = position.y,
                z = position.z,

                xrot = rotation.x,
                yrot = rotation.y,
                zrot = rotation.z,
                wrot = rotation.w
            };

        File.WriteAllText(
            playerPosPath,
            JsonUtility.ToJson(data, true)
        );
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
        if (!HasActiveSaveFolder)
            return;

        foreach(GameObject wb in GameManager.Instance.WorldManager.Worlds)
        {
            if (wb == null)
                continue;

            ChunkCode chunk =
                wb.GetComponent<ChunkCode>();

            if (chunk != null)
                SaveWorldData(chunk);
        }
    }

    public void ChangeName(string Time, string changedName)
    {
        if (!HasActiveSaveFolder)
            return;

        EnsureSaveFolder();

        string folder = Path.Combine(WorldSaveFolder, $"World_{Time}");
        string path = Path.Combine(folder, "world.json");

        WorldSaveData worldsave;

        if (!TryReadJson(path, out worldsave))
            return;

        worldsave.Name = changedName;

        string json =
            JsonUtility.ToJson(
                worldsave,
                true
            );

        File.WriteAllText(path, json);
    }

    public string GetName(string _path)
    {
        if (!HasActiveSaveFolder)
            return null;

        EnsureSaveFolder();

        string folder = Path.Combine(WorldSaveFolder, _path);
        string path = Path.Combine(folder, "world.json");

        WorldSaveData worldsave;

        if (!TryReadJson(path, out worldsave))
            return null;

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

        if (soundSave == null)
            return;

        SoundManager soundManager = GameManager.Instance.SoundManager;

        soundManager.SetMainVolume(soundSave.Main);
        soundManager.SetEffectVolume(soundSave.Effect);
        soundManager.SetBGMVolume(soundSave.BGM);
    }

    public void PlayerSetting_SaveScreen(
        ScreenSettingManager screenSettingManager
    )
    {
        if (screenSettingManager == null)
            return;

        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "ScreenSetting.json");

        ScreenSettingSave screenSave = new ScreenSettingSave()
        {
            ResolutionIndex =
                screenSettingManager.CurrentResolutionIndex,
            ScreenMode =
                screenSettingManager.CurrentScreenMode
        };

        File.WriteAllText(
            path,
            JsonUtility.ToJson(screenSave, true)
        );
    }

    public bool PlayerSetting_LoadScreen(
        ScreenSettingManager screenSettingManager
    )
    {
        if (screenSettingManager == null)
            return false;

        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "ScreenSetting.json");

        if (!File.Exists(path))
            return false;

        string json = File.ReadAllText(path);

        ScreenSettingSave screenSave =
            JsonUtility.FromJson<ScreenSettingSave>(json);

        if (screenSave == null)
            return false;

        screenSettingManager.LoadScreenSetting(
            screenSave.ResolutionIndex,
            screenSave.ScreenMode
        );

        return true;
    }

    public void PlayerSetting_SaveSensitive()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "SensitiveSetting.json");

        InputManager inputManager = GameManager.Instance.InputManager;
        MainCamera mainCamera = GameManager.Instance.MainCamera;

        SensitiveSettingSave sensitiveSave = new SensitiveSettingSave()
        {
            PlayerMove = inputManager.PlayerMoveSensitivity,
            PlayerRotation = mainCamera.PlayerRotationSensitivity,
            ObjectMove = inputManager.ObjectMoveSensitivity,
            ObjectRotation = inputManager.ObjectRotationSensitivity,
            ObjectScale = inputManager.ObjectScaleSensitivity
        };

        File.WriteAllText(path, JsonUtility.ToJson(sensitiveSave, true));
    }

    public void PlayerSetting_LoadSensitive()
    {
        string path = TryMakePlayerSettingFolder();
        path = Path.Combine(path, "SensitiveSetting.json");

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        SensitiveSettingSave sensitiveSave =
            JsonUtility.FromJson<SensitiveSettingSave>(json);

        if (sensitiveSave == null)
            return;

        InputManager inputManager = GameManager.Instance.InputManager;
        MainCamera mainCamera = GameManager.Instance.MainCamera;

        inputManager.SetPlayerMoveSensitivity(sensitiveSave.PlayerMove);
        mainCamera.SetPlayerRotationSensitivity(sensitiveSave.PlayerRotation);
        inputManager.SetObjectMoveSensitivity(sensitiveSave.ObjectMove);
        inputManager.SetObjectRotationSensitivity(sensitiveSave.ObjectRotation);
        inputManager.SetObjectScaleSensitivity(sensitiveSave.ObjectScale);
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

        if (languageSave == null)
            return;

        GameManager.Instance.TranslationManager.ChangeLanguage(languageSave.Language);
    }

    public void PlayerSetting_SaveCanControl(
        bool isExpanded)
    {
        string path =
            TryMakePlayerSettingFolder();

        path =
            Path.Combine(
                path,
                "CanControlSetting.json"
            );

        CanControlSettingSave save =
            new CanControlSettingSave()
            {
                IsExpanded =
                    isExpanded
            };

        File.WriteAllText(
            path,
            JsonUtility.ToJson(
                save,
                true
            )
        );
    }

    public void PlayerSetting_LoadCanControl()
    {
        string path =
            TryMakePlayerSettingFolder();

        path =
            Path.Combine(
                path,
                "CanControlSetting.json"
            );

        if (!File.Exists(path))
        {
            // 구버전 / 최초 실행은 기본 펼침.
            CanControl.ApplyLoadedExpandedState(
                true
            );

            return;
        }

        string json =
            File.ReadAllText(path);

        CanControlSettingSave save =
            JsonUtility.FromJson<
                CanControlSettingSave
            >(json);

        if (save == null)
        {
            CanControl.ApplyLoadedExpandedState(
                true
            );

            return;
        }

        CanControl.ApplyLoadedExpandedState(
            save.IsExpanded
        );
    }

    void LoadStartFile(string _path)
    {
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }

        // 설정 파일이 없는 기존 사용자도 항상 기본 펼침으로 시작.
        CanControl.ApplyLoadedExpandedState(
            true
        );

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

                case "SensitiveSetting.json":
                    PlayerSetting_LoadSensitive();
                    break;

                case "LanguageSetting.json":
                    PlayerSetting_LoadLanguage();
                    break;

                case "CanControlSetting.json":
                    PlayerSetting_LoadCanControl();
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

        if (keysaves == null ||
            keysaves.keys == null)
        {
            return;
        }

        bool hasSavedCanControlToggle =
            false;

        foreach (KeySave save in keysaves.keys)
        {
            if (save == null ||
                save.keyboard == null)
            {
                continue;
            }

            if (save.keyname ==
                KeyNameSaves.CanControlToggle)
            {
                hasSavedCanControlToggle =
                    true;
            }

            if (!_key.KeyboardDictionary.ContainsKey(save.keyname))
                continue;

            _key.KeyboardDictionary[save.keyname] = save.keyboard;
        }

        // 구버전 KeySetting에는 CanControlToggle이 없고
        // CameraTurn 기본키가 F12였으므로 새 F12 토글과 충돌한다.
        // 구버전 파일에서만 CameraTurn을 F11로 1회 마이그레이션한다.
        if (!hasSavedCanControlToggle &&
            _key.KeyboardDictionary
                .TryGetValue(
                    KeyNameSaves.CameraTurn,
                    out Keyboards cameraTurn
                ) &&
            cameraTurn != null &&
            cameraTurn.inputType ==
                KeyInputType.Key &&
            cameraTurn.key ==
                KeyCode.F12)
        {
            cameraTurn.key =
                KeyCode.F11;
        }
    }

    public bool AwaitForMaking = false;

    public void CreateNewSaveWithDefaultWorld(
        string saveFolderName,
        int defaultWorldIndex = 0)
    {
        if (isCreatingNewSave)
            return;

        if (!IsValidSaveFolderName(saveFolderName))
        {
            Debug.LogWarning(
                "[Save] 새 세이브 폴더 이름이 올바르지 않습니다: " +
                saveFolderName
            );
            return;
        }

        string defaultWorldFolder =
            GetDefaultWorldTemplateFolder(defaultWorldIndex);

        if (!Directory.Exists(defaultWorldFolder))
        {
            Debug.LogWarning(
                "[Save] 기본 World 폴더가 없습니다: " +
                defaultWorldFolder
            );
            return;
        }

        isCreatingNewSave = true;

        StartCoroutine(
            CreateNewSaveWithDefaultWorldCoroutine(
                saveFolderName,
                defaultWorldIndex
            )
        );
    }

    IEnumerator CreateNewSaveWithDefaultWorldCoroutine(
    string saveFolderName,
    int defaultWorldIndex
)
    {
        AwaitForMaking = false;


        // =========================
        // 새 Save 폴더 생성
        // =========================

        EnsureSaveFolder(saveFolderName);


        // =========================
        // 기본 Player 설정 생성
        // =========================

        yield return StartCoroutine(
            CopyDefaultPlayerSettingToSaveCoroutine(
                saveFolderName
            )
        );
        // =========================
        // 기본 World 복사
        // =========================

        yield return StartCoroutine(
            CopyDefaultWorldToSaveCoroutine(
                saveFolderName,
                defaultWorldIndex
            )
        );


        float animationWaitStart =
            Time.realtimeSinceStartup;

        while (!AwaitForMaking &&
            Time.realtimeSinceStartup - animationWaitStart < 10f)
        {
            yield return null;
        }

        if (!AwaitForMaking)
        {
            Debug.LogWarning(
                "[Save] 새 세이브 제작 UI 대기 시간이 초과되었습니다. " +
                "세이브 생성은 완료된 상태로 Load UI로 이동합니다."
            );
        }

        isCreatingNewSave = false;

        GameManager.Instance.UIManager
            .StartAdditionalUI
            .SpawnUI(StartUIs.LoadUI);
    }

    void ResetWorldPlayerRuntime()
    {
        BuildManager BM =
            GameManager.Instance.BuildManager;

        if (BM != null)
        {
            if (BM.PreColors == null)
                BM.PreColors = new List<Color>();

            if (BM.SaveColors == null)
                BM.SaveColors = new List<Color>();

            BM.PreColors.Clear();
            BM.SaveColors.Clear();
        }

        if (GameManager.Instance.WorldManager != null)
        {
            GameManager.Instance
                .WorldManager
                .HowManyYouMakeWorld = 0;
        }

        CameraShaderPutter cameraShader =
            GameManager.Instance.MainCamera != null
            ? GameManager.Instance.MainCamera
                .GetComponent<CameraShaderPutter>()
            : null;

        if (cameraShader != null)
        {
            cameraShader.ResetMaterial();
        }
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

    public static string DefaultWorldPlayerSettingFolder =>
    Path.Combine(
        Application.streamingAssetsPath,
        "DefaultWorldPlayerSetting"
    );

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

    IEnumerator CopyDefaultPlayerSettingToSaveCoroutine(
    string saveFolderName
)
    {
        string sourceFolder =
            DefaultWorldPlayerSettingFolder;

        string targetFolder =
            Path.Combine(
                SaveFolder,
                saveFolderName,
                "WorldPlayerSetting"
            );

        // 새 세이브이므로 기존 폴더가 남아 있으면 완전히 초기화한다.
        if (Directory.Exists(targetFolder))
        {
            yield return StartCoroutine(
                DeleteDirectoryCoroutine(targetFolder)
            );
        }

        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogWarning(
                $"[Save] 기본 플레이어 설정 폴더가 없습니다: {sourceFolder}"
            );

            Directory.CreateDirectory(targetFolder);
            EnsureDefaultPlayerPositionFile(targetFolder);
            yield break;
        }

        yield return StartCoroutine(
            CopyDirectoryCoroutine(
                sourceFolder,
                targetFolder
            )
        );

        // 템플릿 PlayerPos.json이 없거나 회전값이 잘못되어도
        // 새 세이브에는 항상 정상적인 기본 PlayerPos를 보장한다.
        EnsureDefaultPlayerPositionFile(targetFolder);
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

        WorldSaveData worldSaveData;

        if (!TryReadJson(
            worldInfoPath,
            out worldSaveData))
        {
            Debug.LogWarning(
                "[Save] 복사된 world.json을 읽지 못했습니다: " +
                worldInfoPath
            );
            return;
        }

        worldSaveData.Time = newWorldTime;

        if (string.IsNullOrEmpty(worldSaveData.Name))
        {
            worldSaveData.Name = $"World_Default_{defaultWorldIndex}";
        }

        string json =
            JsonUtility.ToJson(
                worldSaveData,
                true
            );

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
