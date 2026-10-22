using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockInChunkData
{
    public Color _color;

    public Vector3Int _position;
    public GameObject _go;
}

public enum WorldLoadState
{
    NotLoaded,  // 아직 제작 시작 전
    Loading,    // 데이터/메시 제작 중
    Complete    // 완전히 사용 가능
}

public class ChunkCode : MonoBehaviour
{
    struct BlockColorChangeRequest
    {
        public Vector3Int gridPos;
        public Color color;

        public BlockColorChangeRequest(Vector3Int _gridPos, Color _color)
        {
            gridPos = _gridPos;
            color = _color;
        }
    }

    [Header("Color Change")]
    [SerializeField] private float colorChangeFrameTime = 3f;

    [Header("World Combine")]
    [SerializeField] private float combineFrameTime = 3f;

    Queue<BlockColorChangeRequest> colorChangeQueue = new Queue<BlockColorChangeRequest>();
    HashSet<Vector3Int> colorChangeBuildMeshSet = new HashSet<Vector3Int>();
    HashSet<Vector3Int> colorChangeUpdateBlockMeshSet = new HashSet<Vector3Int>();

    bool isColorChangeRunning = false;
    bool isColorChangeRangeEnqueueRunning = false;

    public string _name = "NewWorld";
    public string Savedtime;

    public WorldLoadState LoadState = WorldLoadState.NotLoaded;

    public bool IsLoadComplete()
    {
        return LoadState == WorldLoadState.Complete;
    }

    public GameObject LightBlock;
    public GameObject ChunksRoot;
    public bool isSavedBefore = false;

    public GameObject CombinedColliderObject;
    MeshCollider CombinedWorldCollider;

    public GameObject CombinedRenderObject;
    MeshFilter CombinedWorldMeshFilter;
    MeshRenderer CombinedWorldMeshRenderer;

    // Reusable temporary buffers to reduce GC/allocation spikes during mesh/collider rebuilds.
    readonly List<Vector3> chunkRenderVerticesBuffer = new List<Vector3>();
    readonly List<int> chunkRenderTrianglesBuffer = new List<int>();
    readonly List<Color> chunkRenderColorsBuffer = new List<Color>();
    RenderMaskCell[,] chunkRenderMaskBuffer;

    readonly List<Vector3> chunkColliderVerticesBuffer = new List<Vector3>();
    readonly List<int> chunkColliderTrianglesBuffer = new List<int>();
    bool[,] chunkColliderMaskBuffer;

    readonly Dictionary<int, HashSet<Vector2Int>>[] combinedColliderCoroutinePlanes =
        CreateColliderPlaneBuffer();
    readonly Stack<HashSet<Vector2Int>> combinedColliderCoroutineCellPool =
        new Stack<HashSet<Vector2Int>>();
    readonly List<Vector3> combinedColliderCoroutineVertices = new List<Vector3>();
    readonly List<int> combinedColliderCoroutineTriangles = new List<int>();
    bool combinedColliderCoroutineBufferInUse = false;

    readonly Dictionary<int, HashSet<Vector2Int>>[] combinedColliderPlanes =
        CreateColliderPlaneBuffer();
    readonly Stack<HashSet<Vector2Int>> combinedColliderCellPool =
        new Stack<HashSet<Vector2Int>>();
    readonly List<Vector3> combinedColliderVertices = new List<Vector3>();
    readonly List<int> combinedColliderTriangles = new List<int>();

    public Dictionary<Vector3Int, GameObject> Chunks; //어느 좌표에 있는 청크가 무슨 게임 오브젝트인지
    public Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>> BlocksinChunk; //어느 좌표에 있는 청크인지 + 그 안에 있는 블럭들
    int ChunkSize = 8;

    int minX = int.MaxValue;
    int minY = int.MaxValue;
    int minZ = int.MaxValue;

    int maxX = int.MinValue;
    int maxY = int.MinValue;
    int maxZ = int.MinValue;

    public Queue<BlockInChunkData> MakeWorldQueue = new();

    bool hasLoggedFirstBuildTiming = false;

    List<System.Action> colorChangeCompleteActions = new List<System.Action>();

    Vector3Int[] directions = new Vector3Int[]
{
    new Vector3Int(0,0,1),  // Front
    new Vector3Int(0,0,-1), // Back
    new Vector3Int(-1,0,0), // Left
    new Vector3Int(1,0,0),  // Right
    new Vector3Int(0,1,0),  // Top
    new Vector3Int(0,-1,0)  // Bottom
};

    static Dictionary<int, HashSet<Vector2Int>>[] CreateColliderPlaneBuffer()
    {
        Dictionary<int, HashSet<Vector2Int>>[] planes =
            new Dictionary<int, HashSet<Vector2Int>>[6];

        for (int face = 0; face < 6; face++)
        {
            planes[face] = new Dictionary<int, HashSet<Vector2Int>>();
        }

        return planes;
    }

    void ResetColliderPlaneBuffer(
        Dictionary<int, HashSet<Vector2Int>>[] planes,
        Stack<HashSet<Vector2Int>> cellPool
    )
    {
        for (int face = 0; face < planes.Length; face++)
        {
            foreach (HashSet<Vector2Int> cells in planes[face].Values)
            {
                cells.Clear();
                cellPool.Push(cells);
            }

            planes[face].Clear();
        }
    }

    HashSet<Vector2Int> GetColliderPlaneCells(
        Dictionary<int, HashSet<Vector2Int>>[] planes,
        Stack<HashSet<Vector2Int>> cellPool,
        int face,
        int slice
    )
    {
        if (planes[face].TryGetValue(slice, out HashSet<Vector2Int> cells))
            return cells;

        if (cellPool.Count > 0)
        {
            cells = cellPool.Pop();
        }
        else
        {
            cells = new HashSet<Vector2Int>();
        }

        planes[face].Add(slice, cells);
        return cells;
    }

    RenderMaskCell[,] GetChunkRenderMaskBuffer()
    {
        if (chunkRenderMaskBuffer == null ||
            chunkRenderMaskBuffer.GetLength(0) != ChunkSize ||
            chunkRenderMaskBuffer.GetLength(1) != ChunkSize)
        {
            chunkRenderMaskBuffer =
                new RenderMaskCell[ChunkSize, ChunkSize];
        }

        return chunkRenderMaskBuffer;
    }

    bool[,] GetChunkColliderMaskBuffer()
    {
        if (chunkColliderMaskBuffer == null ||
            chunkColliderMaskBuffer.GetLength(0) != ChunkSize ||
            chunkColliderMaskBuffer.GetLength(1) != ChunkSize)
        {
            chunkColliderMaskBuffer =
                new bool[ChunkSize, ChunkSize];
        }

        return chunkColliderMaskBuffer;
    }

    void EnsureChunksRoot()
    {
        if (ChunksRoot != null)
            return;

        ChunksRoot = new GameObject("ChunksRoot");
        ChunksRoot.transform.SetParent(transform, false);
        ChunksRoot.transform.localPosition = Vector3.zero;
        ChunksRoot.transform.localRotation = Quaternion.identity;
        ChunksRoot.transform.localScale = Vector3.one;
        ChunksRoot.layer = gameObject.layer;
    }

    void SetEditingRepresentation()
    {
        EnsureChunksRoot();
        ChunksRoot.SetActive(true);

        if (CombinedRenderObject != null)
            CombinedRenderObject.SetActive(false);

        if (CombinedColliderObject != null)
            CombinedColliderObject.SetActive(false);
    }

    void SetInstalledRepresentation()
    {
        if (CombinedRenderObject != null &&
            CombinedWorldMeshFilter != null &&
            CombinedWorldMeshFilter.sharedMesh != null)
        {
            CombinedRenderObject.SetActive(true);
        }

        if (CombinedColliderObject != null &&
            CombinedWorldCollider != null &&
            CombinedWorldCollider.sharedMesh != null)
        {
            CombinedColliderObject.SetActive(true);
        }

        if (ChunksRoot != null)
            ChunksRoot.SetActive(false);
    }

    public void MakeNewWorld(int x, int y, int z)
    {
        if (BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();
        if (Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject>();
        ClearAll();

        if(x == 0 &  y == 0 & z == 0) return;

        for (int _x = -((x/2) + (x%2)); _x < (x / 2); _x++)
        {
            for (int _y = -((y / 2) + (y % 2)); _y < (y / 2); _y++)
            {
                for (int _z = -((z / 2) + (z % 2)); _z < (z / 2); _z++)
                {
                    Vector3Int gridpos = new Vector3Int(_x, _y, _z);

                    MakeWorldQueue.Enqueue(new BlockInChunkData{_color = Color.white, _position = gridpos});
                }
            }
        }

        StartBuildRoutine();
    }

    public void StartBuildRoutine()
    {
        if (BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();
        if (Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject>();

        StartCoroutine(BuildRoutine());
    }

    public IEnumerator BuildRoutine(
        bool finishSettingBlock = true,
        Func<bool> shouldCancel = null,
        bool buildCollider = true
    )
    {
        LoadState = WorldLoadState.Loading;

        if (BlocksinChunk == null)
            BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();

        if (Chunks == null)
            Chunks = new Dictionary<Vector3Int, GameObject>();

        if (shouldCancel != null && shouldCancel())
            yield break;

        float start = Time.realtimeSinceStartup;

        while (MakeWorldQueue.Count > 0)
        {
            if (shouldCancel != null && shouldCancel())
                yield break;

            start = Time.realtimeSinceStartup;

            while (MakeWorldQueue.Count > 0)
            {
                if (shouldCancel != null && shouldCancel())
                    yield break;

                BlockInChunkData newblock = MakeWorldQueue.Dequeue();
                Vector3Int chunkpos = GetChunkCoord(newblock._position);

                if (!BlocksinChunk.ContainsKey(chunkpos))
                {
                    MakeNewChunk(chunkpos);
                }

                var localPos = GetLocalPos(newblock._position);

                if (!BlocksinChunk[chunkpos].ContainsKey(localPos))
                {
                    if (newblock._color.a != 1)
                    {
                        MakeBlockData(newblock._position, newblock._color);
                    }
                    else
                    {
                        BlocksinChunk[chunkpos].Add(
                            localPos,
                            new BlockInChunkData
                            {
                                _position = newblock._position,
                                _color = newblock._color
                            }
                        );
                    }

                    CheckSize(newblock._position);
                }

                if ((Time.realtimeSinceStartup - start) * 1000f > 10f)
                    break;
            }

            yield return null;
        }

        if (shouldCancel != null && shouldCancel())
            yield break;

        start = Time.realtimeSinceStartup;

        var chunkList = Chunks.Keys.ToList();

        for (int i = 0; i < chunkList.Count; i++)
        {
            if (shouldCancel != null && shouldCancel())
                yield break;

            BuildMesh(
                chunkList[i] * ChunkSize,
                buildCollider
            );

            if (shouldCancel != null && shouldCancel())
                yield break;

            if ((Time.realtimeSinceStartup - start) * 1000f > 10f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        if (shouldCancel != null && shouldCancel())
            yield break;

        if (finishSettingBlock)
        {
            FinishSettingBlock();

            // Normal creation/edit path completes here.
            LoadState = WorldLoadState.Complete;
        }

        // 여기까지 왔으면 블록 + Chunk + Mesh 준비 완료
    }

    void ClearAll()
    {
        foreach (var chunk in BlocksinChunk.Values)
        {
            foreach(BlockInChunkData block in chunk.Values)
            {
                if(block._go != null)
                {
                    Destroy(block._go);
                }
            }
        }
        BlocksinChunk.Clear();

        foreach (GameObject chunk in Chunks.Values)
        {
            Destroy(chunk);
        }

        Chunks.Clear();


        // =========================
        // Combined Render 정리
        // =========================

        if (CombinedWorldMeshFilter != null)
        {
            Mesh combinedMesh =
                CombinedWorldMeshFilter.sharedMesh;

            CombinedWorldMeshFilter.sharedMesh =
                null;


            if (combinedMesh != null)
            {
                Destroy(combinedMesh);
            }
        }


        if (CombinedRenderObject != null)
        {
            Destroy(CombinedRenderObject);
        }


        CombinedRenderObject = null;
        CombinedWorldMeshFilter = null;
        CombinedWorldMeshRenderer = null;
    }

    public bool ishaveblock(Vector3 _position)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(_position);

        if (!BlocksinChunk.ContainsKey(GetChunkCoord(gridPos))) return false;
        if (!BlocksinChunk[GetChunkCoord(gridPos)].ContainsKey(GetLocalPos(gridPos))) return false;

        return true;
    }

    public void SetName()
    {
        Savedtime = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
    }

    public void MakeBlockData(Vector3 _position, Color color)
    {
        if(BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>> ();
        if(Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject> ();

        Vector3Int gridPos = Vector3Int.RoundToInt(_position);
        Vector3Int ChunkPos = GetChunkCoord(gridPos);
        Vector3Int LocalPos = GetLocalPos(gridPos);

        if (!Chunks.ContainsKey(ChunkPos))
        {
            MakeNewChunk(ChunkPos); //청크가 없을 시 청크 추가
        }

        if (BlocksinChunk[ChunkPos].ContainsKey(LocalPos))
        {
            Debug.LogWarning("이미 있는 좌표");
            return;
        }
        else
        {
            BlocksinChunk[ChunkPos].Add(LocalPos, new BlockInChunkData { _position = gridPos, _color = color }); //데이터 저장
        }

        if (color.a < 1)
        {
            GameObject block = Instantiate(GameManager.Instance.BuildManager.ChooseBlock, transform);
            block.transform.localPosition = gridPos;
            block.GetComponent<Renderer>().material.color = color;
            block.tag = "Block";
            block.layer = gameObject.layer;

            BlocksinChunk[ChunkPos].TryGetValue(LocalPos, out BlockInChunkData value);
            value._go = block;

            OnBlockPlaced(gridPos);

            UpdateBlockMesh(gridPos);
            foreach (var dir in directions) UpdateBlockMesh(gridPos + dir);
        }
        
        BuildMesh(gridPos);
        trytoBuildMesh(gridPos);
    }

    public Dictionary<Vector3Int, BlockInChunkData> PrepareLoadChunk(
        Vector3Int chunkPos,
        int blockCount
    )
    {
        if (BlocksinChunk == null)
            BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();

        if (Chunks == null)
            Chunks = new Dictionary<Vector3Int, GameObject>();

        if (!Chunks.ContainsKey(chunkPos))
        {
            MakeNewChunk(
                chunkPos,
                blockCount
            );
        }

        return BlocksinChunk[chunkPos];
    }

    public void LoadPreparedChunkBlockData(
        Dictionary<Vector3Int, BlockInChunkData> blocks,
        Vector3Int chunkPos,
        Vector3Int gridPos,
        Color color
    )
    {
        Vector3Int localPos =
            gridPos - (chunkPos * ChunkSize);

        if (blocks.ContainsKey(localPos))
            return;

        BlockInChunkData blockData =
            new BlockInChunkData
            {
                _position = gridPos,
                _color = color
            };

        blocks.Add(
            localPos,
            blockData
        );

        if (color.a < 1f)
        {
            MakeAlphaBlockObjectIfNeed(
                gridPos,
                blockData
            );
        }

        CheckSize(gridPos);
    }

    Queue<Vector3> destroyQueue = new Queue<Vector3>();
    HashSet<Vector3Int> buildMeshSet = new HashSet<Vector3Int>();

    bool isDestroyRunning = false;

    public void DestroyBlockData(Vector3 _position)
    {
        if (BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();
        if (Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject>();

        destroyQueue.Enqueue(_position);

        if (!isDestroyRunning)
        {
            StartCoroutine(DestroyWorker());
        }
    }

    public void DestroyBlockData(List<Vector3> _position)
    {
        if (BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();
        if (Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject>();

        foreach (var pos in _position)
        {
            destroyQueue.Enqueue(pos);
        }

        if (!isDestroyRunning)
        {
            StartCoroutine(DestroyWorker());
        }
    }
    IEnumerator DestroyWorker()
    {
        isDestroyRunning = true;

        while (destroyQueue.Count > 0)
        {
            float start = Time.realtimeSinceStartup;

            while (destroyQueue.Count > 0)
            {
                Vector3 pos = destroyQueue.Dequeue();

                Vector3Int gridPos = Vector3Int.RoundToInt(pos);
                Vector3Int chunkPos = GetChunkCoord(gridPos);
                Vector3Int localPos = GetLocalPos(gridPos);

                if (BlocksinChunk.TryGetValue(chunkPos, out var value))
                {
                    if (value.TryGetValue(localPos, out BlockInChunkData block))
                    {
                        if (block._go != null)
                        {
                            Destroy(block._go);
                        }

                        buildMeshSet.Add(chunkPos);

                        foreach (Vector3Int dir in directions)
                        {
                            buildMeshSet.Add(chunkPos + dir);
                        }

                        value.Remove(localPos);

                        if (value.Count == 0)
                        {
                            BlocksinChunk.Remove(chunkPos);

                            if (Chunks.TryGetValue(chunkPos, out GameObject chunkObj))
                            {
                                Destroy(chunkObj);
                                Chunks.Remove(chunkPos);
                            }
                        }
                    }
                }

                if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
                    break;
            }

            yield return null;
        }

        yield return StartCoroutine(BuildMeshWorker());

        isDestroyRunning = false;
    }

    IEnumerator BuildMeshWorker()
    {
        float start = Time.realtimeSinceStartup;

        foreach (var pos in buildMeshSet)
        {
            BuildMesh(pos * ChunkSize);

            if ((Time.realtimeSinceStartup - start) * 1000f > 5f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        buildMeshSet.Clear();

        CheckIsLive();
    }
    /*
    public BlockInChunkData DestroyBlockData(Vector3 _position)
    {
        if (BlocksinChunk == null) BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();
        if (Chunks == null) Chunks = new Dictionary<Vector3Int, GameObject>();

        Vector3Int gridPos = Vector3Int.RoundToInt(_position);
        Vector3Int ChunkPos = GetChunkCoord(gridPos);
        Vector3Int LocalPos = GetLocalPos(gridPos);

        if(!BlocksinChunk.ContainsKey(ChunkPos)) return null;
        if (!BlocksinChunk[ChunkPos].ContainsKey(LocalPos)) return null;

        BlocksinChunk[ChunkPos].TryGetValue(LocalPos, out BlockInChunkData value);
        BlocksinChunk[ChunkPos].Remove(LocalPos);

        if (value._color.a == 1) //색블럭이면 없는 블럭 생성
        {
            GameObject block = new GameObject();
            block.transform.parent = transform;

            block.transform.localPosition = gridPos;

            MeshFilter mf = block.AddComponent<MeshFilter>();
            mf.sharedMesh = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshFilter>().sharedMesh;

            MeshRenderer mr = block.AddComponent<MeshRenderer>();
            mr.sharedMaterials = GameManager.Instance.BuildManager.ChooseBlock.GetComponent<MeshRenderer>().sharedMaterials;

            block.transform.localScale = Vector3.one;
            block.transform.rotation = new Quaternion(0, 0, 0, 0);

            block.GetComponent<Renderer>().material.color = value._color;
            block.tag = "Block";

            value._go = block;
        }
        else
        {
            foreach (var dir in directions) UpdateBlockMesh(gridPos + dir);
        }

        if (BlocksinChunk[ChunkPos].Count == 0)
        {
            BlocksinChunk.Remove(ChunkPos);

            Destroy(Chunks[ChunkPos]);
            Chunks.Remove(ChunkPos);
        }
        else
        {
            BuildMesh(gridPos);
        }
        trytoBuildMesh(gridPos);

        return value;
    }*/

    void trytoBuildMesh(Vector3Int gridPos)
    {
        Vector3Int LocalPos = GetLocalPos(gridPos);
        Vector3Int ChunkPos = GetChunkCoord(gridPos);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3Int dir = directions[i];

            bool isBorder =
                (dir == Vector3Int.left && LocalPos.x == 0) ||
                (dir == Vector3Int.right && LocalPos.x == ChunkSize - 1) ||
                (dir == Vector3Int.down && LocalPos.y == 0) ||
                (dir == Vector3Int.up && LocalPos.y == ChunkSize - 1) ||
                (dir == new Vector3Int(0, 0, -1) && LocalPos.z == 0) ||
                (dir == new Vector3Int(0, 0, 1) && LocalPos.z == ChunkSize - 1);

            if (!isBorder)
                continue;

            Vector3Int neighborWorld = gridPos + dir;
            Vector3Int neighborChunk = GetChunkCoord(neighborWorld);

            if (neighborChunk != ChunkPos && Chunks.ContainsKey(neighborChunk))
            {
                BuildMesh(neighborWorld);
            }
        }
    }

    public void FinishSettingBlock()
    {
        switch (GameManager.Instance.flow)
        {
            case GameFlow.WatchAllWorld:
                PutThisWorld();
                break;

            case GameFlow.MakeOneWorld:
                findselect();
                StartChangeEveryThingSmooth(0.5f, false, 0, true, Camera.main.transform.position + Camera.main.transform.forward * (Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) + 20f),
                    false, new Quaternion(0, 0, 0, 0));
                break;

            case GameFlow.SelectOneWorld:
                GameManager.Instance.UIManager.SelectionUI.DestroyAllAdditionalUi();
                GameManager.Instance.InputManager.ChooseWorld();
                GameManager.Instance.ChangeFlow(GameFlow.WatchOneWorld);
                break;
        }
    }

    void findselect()
    {
        foreach(var ui in GameManager.Instance.UIManager.SelectionUI.uIBtnsSelectedUIMaterials)
        {
            if(ui.UIenum == SelectionUIenum.ChooseHowToMake)
            {
                ui.UISelected.GetComponent<ChooseHowToMake>().check();
            }
        }
    }

    public void ChooseThisWorld()
    {
        EnsureChunkCollidersForEditing();
        SetEditingRepresentation();
    }

    void EnsureChunkCollidersForEditing()
    {
        if (Chunks == null)
            return;

        foreach (var chunkEntry in Chunks)
        {
            GameObject chunkObject = chunkEntry.Value;

            if (chunkObject == null)
                continue;

            MeshCollider meshCollider =
                chunkObject.GetComponent<MeshCollider>();

            if (meshCollider != null &&
                meshCollider.sharedMesh != null)
            {
                continue;
            }

            BuildChunkCollider(chunkEntry.Key);
        }
    }

    public void PutThisWorld()
    {
        UseCombinedWorldCollider();
        UseCombinedWorldRenderer();
        SetInstalledRepresentation();
    }

    public IEnumerator PutThisWorldCoroutine()
    {
        LoadState = WorldLoadState.Loading;

        yield return StartCoroutine(UseCombinedWorldColliderCoroutine());
        yield return StartCoroutine(UseCombinedWorldRendererCoroutine());

        SetInstalledRepresentation();
        LoadState = WorldLoadState.Complete;
    }


    void CheckSize(Vector3 position)
    {
        if (position.x > maxX) maxX = (int)position.x;
        if (position.y > maxY) maxY = (int)position.y;
        if (position.z > maxZ) maxZ = (int)position.z;

        if (position.x < minX) minX = (int)position.x;
        if (position.y < minY) minY = (int)position.y;
        if (position.z < minZ) minZ = (int)position.z;
    }

    public Vector3Int GiveThreeSize()
    {
        return new Vector3Int(maxX - minX +1, maxY - minY + 1, maxZ - minZ + 1);
    }

    Vector3Int GetLocalPos(Vector3Int worldPos)
    {
        Vector3Int chunkCoord = GetChunkCoord(worldPos);
        Vector3Int chunkOrigin = chunkCoord * ChunkSize;

        return worldPos - chunkOrigin;
    }

    Vector3Int GetChunkCoord(Vector3Int blockPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt((float)blockPos.x / ChunkSize),
            Mathf.FloorToInt((float)blockPos.y / ChunkSize),
            Mathf.FloorToInt((float)blockPos.z / ChunkSize)
        );
    }

    void MakeNewChunk(
        Vector3Int _position,
        int blockCapacity = 0
    )
    {
        GameObject _chunk =
            new GameObject($"Chunk_{_position}");

        EnsureChunksRoot();

        _chunk.transform.SetParent(
            ChunksRoot.transform,
            false
        );

        _chunk.transform.localPosition =
            _position * ChunkSize;

        _chunk.transform.localRotation =
            Quaternion.identity;

        _chunk.transform.localScale =
            Vector3.one;

        _chunk.tag = "Block";
        _chunk.layer = gameObject.layer;


        _chunk.AddComponent<MeshFilter>();


        MeshRenderer renderer =
            _chunk.AddComponent<MeshRenderer>();

        renderer.sharedMaterial =
            GameManager.Instance.MaterialManager.MainMaterial;


        _chunk.AddComponent<MeshCollider>();


        Chunks.Add(
            _position,
            _chunk
        );

        BlocksinChunk.Add(
            _position,
            new Dictionary<Vector3Int, BlockInChunkData>(
                Mathf.Max(0, blockCapacity)
            )
        );
    }

    public void BuildMesh(
        Vector3Int worldPos,
        bool buildCollider = true
    )
    {
        Vector3Int chunkCoord =
            GetChunkCoord(worldPos);

        if (!BlocksinChunk.ContainsKey(chunkCoord))
            return;

        float totalStart = Time.realtimeSinceStartup;

        float renderStart = Time.realtimeSinceStartup;
        BuildChunkRenderMesh(chunkCoord);
        float renderMs =
            (Time.realtimeSinceStartup - renderStart) * 1000f;

        float colliderMs = 0f;

        if (buildCollider)
        {
            float colliderStart = Time.realtimeSinceStartup;
            BuildChunkCollider(chunkCoord);
            colliderMs =
                (Time.realtimeSinceStartup - colliderStart) * 1000f;
        }

        float totalMs =
            (Time.realtimeSinceStartup - totalStart) * 1000f;

        if (!hasLoggedFirstBuildTiming || totalMs >= 50f)
        {
            Debug.Log(
                "[LoadTiming] Mesh chunk=" + chunkCoord +
                " renderMs=" + renderMs.ToString("F2") +
                " colliderMs=" + colliderMs.ToString("F2") +
                " totalMs=" + totalMs.ToString("F2")
            );

            hasLoggedFirstBuildTiming = true;
        }
    }

    struct RenderMaskCell
    {
        public bool active;
        public Color color;

        public RenderMaskCell(
            bool _active,
            Color _color
        )
        {
            active = _active;
            color = _color;
        }
    }

    bool IsOpaqueRenderBlock(Vector3Int worldPos)
    {
        Vector3Int chunkCoord =
            GetChunkCoord(worldPos);

        Vector3Int localPos =
            worldPos - (chunkCoord * ChunkSize);


        if (!BlocksinChunk.TryGetValue(
            chunkCoord,
            out Dictionary<Vector3Int, BlockInChunkData> blocks
        ))
        {
            return false;
        }


        if (!blocks.TryGetValue(
            localPos,
            out BlockInChunkData block
        ))
        {
            return false;
        }


        return block._color.a >= 1f;
    }

    bool TryGetNeighborBlock(
        Vector3Int chunkCoord,
        Dictionary<Vector3Int, BlockInChunkData> currentBlocks,
        Vector3Int localPos,
        Vector3Int direction,
        out BlockInChunkData block
    )
    {
        Vector3Int neighborLocal =
            localPos + direction;

        if (
            neighborLocal.x >= 0 &&
            neighborLocal.x < ChunkSize &&
            neighborLocal.y >= 0 &&
            neighborLocal.y < ChunkSize &&
            neighborLocal.z >= 0 &&
            neighborLocal.z < ChunkSize
        )
        {
            return currentBlocks.TryGetValue(
                neighborLocal,
                out block
            );
        }

        Vector3Int neighborChunk =
            chunkCoord + direction;

        neighborLocal -=
            direction * ChunkSize;

        if (!BlocksinChunk.TryGetValue(
            neighborChunk,
            out Dictionary<Vector3Int, BlockInChunkData> neighborBlocks
        ))
        {
            block = null;
            return false;
        }

        return neighborBlocks.TryGetValue(
            neighborLocal,
            out block
        );
    }

    void BuildChunkRenderMesh(Vector3Int chunkCoord)
    {
        if (!Chunks.TryGetValue(
            chunkCoord,
            out GameObject chunkObject
        ))
        {
            return;
        }


        if (!BlocksinChunk.TryGetValue(
            chunkCoord,
            out Dictionary<Vector3Int, BlockInChunkData> blocks
        ))
        {
            return;
        }


        List<Vector3> vertices = chunkRenderVerticesBuffer;
        List<int> triangles = chunkRenderTrianglesBuffer;
        List<Color> colors = chunkRenderColorsBuffer;

        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        RenderMaskCell[,] mask = GetChunkRenderMaskBuffer();


        // =========================
        // 6방향
        // =========================

        for (int face = 0; face < 6; face++)
        {
            for (
                int slice = 0;
                slice < ChunkSize;
                slice++
            )
            {
                Array.Clear(
                    mask,
                    0,
                    mask.Length
                );


                // =========================
                // 보이는 면 찾기
                // =========================

                for (
                    int v = 0;
                    v < ChunkSize;
                    v++
                )
                {
                    for (
                        int u = 0;
                        u < ChunkSize;
                        u++
                    )
                    {
                        Vector3Int localPos =
                            GetColliderLocalPosition(
                                face,
                                slice,
                                u,
                                v
                            );


                        if (!blocks.TryGetValue(
                            localPos,
                            out BlockInChunkData block
                        ))
                        {
                            continue;
                        }


                        // 투명 블록은 기존 개별 Renderer 사용
                        if (block._color.a < 1f)
                            continue;


                        if (
                            TryGetNeighborBlock(
                                chunkCoord,
                                blocks,
                                localPos,
                                directions[face],
                                out BlockInChunkData neighborBlock
                            )
                            &&
                            neighborBlock._color.a >= 1f
                        )
                        {
                            continue;
                        }


                        mask[u, v] =
                            new RenderMaskCell(
                                true,
                                block._color
                            );
                    }
                }


                // =========================
                // 같은 색 면 Greedy Merge
                // =========================

                MergeRenderMask(
                    face,
                    slice,
                    mask,

                    vertices,
                    triangles,
                    colors
                );
            }
        }


        // =========================
        // Mesh 적용
        // =========================

        MeshFilter mf =
            chunkObject.GetComponent<MeshFilter>();


        Mesh mesh =
            mf.sharedMesh;


        if (mesh == null)
        {
            mesh = new Mesh();

            mesh.name =
                $"Render_{chunkCoord}";

            mf.sharedMesh =
                mesh;
        }
        else
        {
            mesh.Clear();
        }


        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();


        // =========================
        // 빈 Chunk Renderer OFF
        // =========================

        MeshRenderer mr =
            chunkObject.GetComponent<MeshRenderer>();

        if (mr != null)
        {
            mr.enabled =
                vertices.Count > 0;
        }
    }

    void MergeRenderMask(
    int face,
    int slice,

    RenderMaskCell[,] mask,

    List<Vector3> vertices,
    List<int> triangles,
    List<Color> colors
)
    {
        for (
            int v = 0;
            v < ChunkSize;
            v++
        )
        {
            int u = 0;


            while (u < ChunkSize)
            {
                if (!mask[u, v].active)
                {
                    u++;
                    continue;
                }


                Color color =
                    mask[u, v].color;


                // =========================
                // 가로 확장
                // =========================

                int width = 1;


                while (
                    u + width < ChunkSize
                    &&
                    mask[u + width, v].active
                    &&
                    SameRenderColor(
                        mask[u + width, v].color,
                        color
                    )
                )
                {
                    width++;
                }


                // =========================
                // 세로 확장
                // =========================

                int height = 1;

                bool canGrow = true;


                while (
                    v + height < ChunkSize
                    &&
                    canGrow
                )
                {
                    for (
                        int x = 0;
                        x < width;
                        x++
                    )
                    {
                        RenderMaskCell cell =
                            mask[
                                u + x,
                                v + height
                            ];


                        if (
                            !cell.active
                            ||
                            !SameRenderColor(
                                cell.color,
                                color
                            )
                        )
                        {
                            canGrow = false;
                            break;
                        }
                    }


                    if (canGrow)
                    {
                        height++;
                    }
                }


                // =========================
                // 사용한 영역 제거
                // =========================

                for (
                    int y = 0;
                    y < height;
                    y++
                )
                {
                    for (
                        int x = 0;
                        x < width;
                        x++
                    )
                    {
                        mask[
                            u + x,
                            v + y
                        ].active = false;
                    }
                }


                // =========================
                // 큰 Quad 하나 생성
                // =========================

                AddRenderQuad(
                    face,
                    slice,

                    u,
                    v,

                    width,
                    height,

                    color,

                    vertices,
                    triangles,
                    colors
                );


                u += width;
            }
        }
    }

    bool SameRenderColor(
    Color a,
    Color b
)
    {
        return
            a.r == b.r &&
            a.g == b.g &&
            a.b == b.b &&
            a.a == b.a;
    }

    void AddRenderQuad(
    int face,
    int slice,

    int u,
    int v,

    int width,
    int height,

    Color color,

    List<Vector3> vertices,
    List<int> triangles,
    List<Color> colors
)
    {
        float u0 =
            u - 0.5f;

        float u1 =
            u + width - 0.5f;

        float v0 =
            v - 0.5f;

        float v1 =
            v + height - 0.5f;


        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;


        switch (face)
        {
            // Front +Z
            case 0:
                {
                    float z =
                        slice + 0.5f;

                    p0 = new Vector3(u0, v0, z);
                    p1 = new Vector3(u1, v0, z);
                    p2 = new Vector3(u1, v1, z);
                    p3 = new Vector3(u0, v1, z);

                    break;
                }


            // Back -Z
            case 1:
                {
                    float z =
                        slice - 0.5f;

                    p0 = new Vector3(u1, v0, z);
                    p1 = new Vector3(u0, v0, z);
                    p2 = new Vector3(u0, v1, z);
                    p3 = new Vector3(u1, v1, z);

                    break;
                }


            // Left -X
            case 2:
                {
                    float x =
                        slice - 0.5f;

                    p0 = new Vector3(x, v0, u0);
                    p1 = new Vector3(x, v0, u1);
                    p2 = new Vector3(x, v1, u1);
                    p3 = new Vector3(x, v1, u0);

                    break;
                }


            // Right +X
            case 3:
                {
                    float x =
                        slice + 0.5f;

                    p0 = new Vector3(x, v0, u1);
                    p1 = new Vector3(x, v0, u0);
                    p2 = new Vector3(x, v1, u0);
                    p3 = new Vector3(x, v1, u1);

                    break;
                }


            // Top +Y
            case 4:
                {
                    float y =
                        slice + 0.5f;

                    p0 = new Vector3(u0, y, v1);
                    p1 = new Vector3(u1, y, v1);
                    p2 = new Vector3(u1, y, v0);
                    p3 = new Vector3(u0, y, v0);

                    break;
                }


            // Bottom -Y
            default:
                {
                    float y =
                        slice - 0.5f;

                    p0 = new Vector3(u0, y, v0);
                    p1 = new Vector3(u1, y, v0);
                    p2 = new Vector3(u1, y, v1);
                    p3 = new Vector3(u0, y, v1);

                    break;
                }
        }


        int index =
            vertices.Count;


        vertices.Add(p0);
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);


        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index + 0);
        triangles.Add(index + 2);
        triangles.Add(index + 3);


        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    void AddCube(
      Vector3Int chunkCoord,
      BlockInChunkData block,

      ref List<Vector3> renderVertices,
      ref List<int> renderTriangles,
      ref List<Color> renderColors,
      ref int renderVertexIndex
  )
    {
        Vector3Int localPos =
            GetLocalPos(block._position);

        Vector3Int worldPos =
            block._position;


        bool currentOpaque =
            block._color.a >= 1f;


        Vector3[,] faceVerts = new Vector3[6, 4]
        {
        {
            new(-0.5f,-0.5f, 0.5f),
            new( 0.5f,-0.5f, 0.5f),
            new( 0.5f, 0.5f, 0.5f),
            new(-0.5f, 0.5f, 0.5f)
        },

        {
            new( 0.5f,-0.5f,-0.5f),
            new(-0.5f,-0.5f,-0.5f),
            new(-0.5f, 0.5f,-0.5f),
            new( 0.5f, 0.5f,-0.5f)
        },

        {
            new(-0.5f,-0.5f,-0.5f),
            new(-0.5f,-0.5f, 0.5f),
            new(-0.5f, 0.5f, 0.5f),
            new(-0.5f, 0.5f,-0.5f)
        },

        {
            new( 0.5f,-0.5f, 0.5f),
            new( 0.5f,-0.5f,-0.5f),
            new( 0.5f, 0.5f,-0.5f),
            new( 0.5f, 0.5f, 0.5f)
        },

        {
            new(-0.5f, 0.5f, 0.5f),
            new( 0.5f, 0.5f, 0.5f),
            new( 0.5f, 0.5f,-0.5f),
            new(-0.5f, 0.5f,-0.5f)
        },

        {
            new(-0.5f,-0.5f,-0.5f),
            new( 0.5f,-0.5f,-0.5f),
            new( 0.5f,-0.5f, 0.5f),
            new(-0.5f,-0.5f, 0.5f)
        }
        };


        for (int face = 0; face < 6; face++)
        {
            Vector3Int neighborWorld =
                worldPos + directions[face];

            Vector3Int neighborChunk =
                GetChunkCoord(neighborWorld);

            Vector3Int neighborLocal =
                GetLocalPos(neighborWorld);


            bool neighborOpaque = false;


            if (
                BlocksinChunk.ContainsKey(neighborChunk) &&
                BlocksinChunk[neighborChunk].TryGetValue(
                    neighborLocal,
                    out BlockInChunkData neighborBlock
                )
            )
            {
                neighborOpaque =
                    neighborBlock._color.a >= 1f;
            }


            Vector3 p = localPos;


            // 불투명 블록의 보이는 면만 Render
            if (!neighborOpaque && currentOpaque)
            {
                for (int i = 0; i < 4; i++)
                {
                    renderVertices.Add(
                        p + faceVerts[face, i]
                    );
                }


                renderTriangles.Add(
                    renderVertexIndex + 0
                );

                renderTriangles.Add(
                    renderVertexIndex + 1
                );

                renderTriangles.Add(
                    renderVertexIndex + 2
                );


                renderTriangles.Add(
                    renderVertexIndex + 0
                );

                renderTriangles.Add(
                    renderVertexIndex + 2
                );

                renderTriangles.Add(
                    renderVertexIndex + 3
                );


                for (int i = 0; i < 4; i++)
                {
                    renderColors.Add(
                        block._color
                    );
                }


                renderVertexIndex += 4;
            }
        }
    }

    void BuildChunkCollider(
    Vector3Int chunkCoord
)
    {
        if (!Chunks.TryGetValue(
            chunkCoord,
            out GameObject chunkObject
        ))
        {
            return;
        }


        if (!BlocksinChunk.TryGetValue(
            chunkCoord,
            out Dictionary<Vector3Int, BlockInChunkData> blocks
        ))
        {
            return;
        }


        List<Vector3> vertices = chunkColliderVerticesBuffer;
        List<int> triangles = chunkColliderTrianglesBuffer;

        vertices.Clear();
        triangles.Clear();

        bool[,] mask = GetChunkColliderMaskBuffer();


        // 6방향 검사
        for (int face = 0; face < 6; face++)
        {
            // 청크 내부의 각 단면
            for (
                int slice = 0;
                slice < ChunkSize;
                slice++
            )
            {
                Array.Clear(
                    mask,
                    0,
                    mask.Length
                );


                // =========================
                // 노출된 Solid 면 찾기
                // =========================

                for (
                    int v = 0;
                    v < ChunkSize;
                    v++
                )
                {
                    for (
                        int u = 0;
                        u < ChunkSize;
                        u++
                    )
                    {
                        Vector3Int localPos =
                            GetColliderLocalPosition(
                                face,
                                slice,
                                u,
                                v
                            );


                        if (!blocks.ContainsKey(localPos))
                            continue;


                        if (
                            TryGetNeighborBlock(
                                chunkCoord,
                                blocks,
                                localPos,
                                directions[face],
                                out _
                            )
                        )
                        {
                            continue;
                        }


                        mask[u, v] = true;
                    }
                }


                // =========================
                // 같은 면들을 큰 사각형으로 합치기
                // =========================

                MergeColliderMask(
                    face,
                    slice,
                    mask,
                    vertices,
                    triangles
                );
            }
        }


        Mesh colliderMesh =
            new Mesh();

        colliderMesh.name =
            $"Collider_{chunkCoord}";


        colliderMesh.SetVertices(
            vertices
        );

        colliderMesh.SetTriangles(
            triangles,
            0
        );

        colliderMesh.RecalculateBounds();


        MeshCollider mc =
            chunkObject.GetComponent<MeshCollider>();


        if (mc == null)
        {
            mc =
                chunkObject.AddComponent<MeshCollider>();
        }


        mc.sharedMesh = null;
        mc.sharedMesh = colliderMesh;
    }

    bool IsColliderBlock(Vector3Int worldPos)
    {
        Vector3Int chunkCoord =
            GetChunkCoord(worldPos);

        Vector3Int localPos =
            worldPos - (chunkCoord * ChunkSize);


        if (!BlocksinChunk.TryGetValue(
            chunkCoord,
            out Dictionary<Vector3Int, BlockInChunkData> blocks
        ))
        {
            return false;
        }


        // Alpha와 상관없이
        // 블록 데이터가 존재하면 Collider 대상
        return blocks.ContainsKey(localPos);
    }

    Vector3Int GetColliderLocalPosition(
    int face,
    int slice,
    int u,
    int v
)
    {
        switch (face)
        {
            // Front / Back
            // X, Y 평면
            case 0:
            case 1:

                return new Vector3Int(
                    u,
                    v,
                    slice
                );


            // Left / Right
            // Z, Y 평면
            case 2:
            case 3:

                return new Vector3Int(
                    slice,
                    v,
                    u
                );


            // Top / Bottom
            // X, Z 평면
            case 4:
            case 5:

                return new Vector3Int(
                    u,
                    slice,
                    v
                );
        }


        return Vector3Int.zero;
    }

    void MergeColliderMask(
    int face,
    int slice,
    bool[,] mask,
    List<Vector3> vertices,
    List<int> triangles
)
    {
        for (
            int v = 0;
            v < ChunkSize;
            v++
        )
        {
            int u = 0;


            while (u < ChunkSize)
            {
                if (!mask[u, v])
                {
                    u++;
                    continue;
                }


                // =========================
                // 가로 길이 찾기
                // =========================

                int width = 1;


                while (
                    u + width < ChunkSize &&
                    mask[u + width, v]
                )
                {
                    width++;
                }


                // =========================
                // 세로 길이 찾기
                // =========================

                int height = 1;

                bool canGrow = true;


                while (
                    v + height < ChunkSize &&
                    canGrow
                )
                {
                    for (
                        int x = 0;
                        x < width;
                        x++
                    )
                    {
                        if (
                            !mask[
                                u + x,
                                v + height
                            ]
                        )
                        {
                            canGrow = false;
                            break;
                        }
                    }


                    if (canGrow)
                    {
                        height++;
                    }
                }


                // =========================
                // 사용된 영역 제거
                // =========================

                for (
                    int y = 0;
                    y < height;
                    y++
                )
                {
                    for (
                        int x = 0;
                        x < width;
                        x++
                    )
                    {
                        mask[
                            u + x,
                            v + y
                        ] = false;
                    }
                }


                // 큰 Quad 하나 생성
                AddColliderQuad(
                    face,
                    slice,

                    u,
                    v,

                    width,
                    height,

                    vertices,
                    triangles
                );


                u += width;
            }
        }
    }

    void AddColliderQuad(
    int face,
    int slice,

    int u,
    int v,

    int width,
    int height,

    List<Vector3> vertices,
    List<int> triangles
)
    {
        float u0 =
            u - 0.5f;

        float u1 =
            u + width - 0.5f;

        float v0 =
            v - 0.5f;

        float v1 =
            v + height - 0.5f;


        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;


        switch (face)
        {
            // Front +Z
            case 0:
                {
                    float z =
                        slice + 0.5f;

                    p0 = new Vector3(u0, v0, z);
                    p1 = new Vector3(u1, v0, z);
                    p2 = new Vector3(u1, v1, z);
                    p3 = new Vector3(u0, v1, z);

                    break;
                }


            // Back -Z
            case 1:
                {
                    float z =
                        slice - 0.5f;

                    p0 = new Vector3(u1, v0, z);
                    p1 = new Vector3(u0, v0, z);
                    p2 = new Vector3(u0, v1, z);
                    p3 = new Vector3(u1, v1, z);

                    break;
                }


            // Left -X
            case 2:
                {
                    float x =
                        slice - 0.5f;

                    p0 = new Vector3(x, v0, u0);
                    p1 = new Vector3(x, v0, u1);
                    p2 = new Vector3(x, v1, u1);
                    p3 = new Vector3(x, v1, u0);

                    break;
                }


            // Right +X
            case 3:
                {
                    float x =
                        slice + 0.5f;

                    p0 = new Vector3(x, v0, u1);
                    p1 = new Vector3(x, v0, u0);
                    p2 = new Vector3(x, v1, u0);
                    p3 = new Vector3(x, v1, u1);

                    break;
                }


            // Top +Y
            case 4:
                {
                    float y =
                        slice + 0.5f;

                    p0 = new Vector3(u0, y, v1);
                    p1 = new Vector3(u1, y, v1);
                    p2 = new Vector3(u1, y, v0);
                    p3 = new Vector3(u0, y, v0);

                    break;
                }


            // Bottom -Y
            default:
                {
                    float y =
                        slice - 0.5f;

                    p0 = new Vector3(u0, y, v0);
                    p1 = new Vector3(u1, y, v0);
                    p2 = new Vector3(u1, y, v1);
                    p3 = new Vector3(u0, y, v1);

                    break;
                }
        }


        int index =
            vertices.Count;


        vertices.Add(p0);
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);


        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index + 0);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    bool IsFullyCovered(Vector3Int pos)
    {
        foreach (var dir in directions)
        {
            Vector3Int nPos = pos + dir;

            if (!BlocksinChunk.ContainsKey(GetChunkCoord(nPos)))
                return false;

            if (!BlocksinChunk[GetChunkCoord(nPos)].TryGetValue(GetLocalPos(nPos), out BlockInChunkData data))
                return false;

            if (data._color.a == 1)
                return false;
        }
        return true;
    }

    public void OnBlockPlaced(Vector3Int pos)
    {
        UpdateSelfVisibility(pos);

        UpdateAroundVisibility(pos);
    }

    void UpdateSelfVisibility(Vector3Int pos)
    {
        if (!BlocksinChunk.ContainsKey(GetChunkCoord(pos)))
            return;

        if (!BlocksinChunk[GetChunkCoord(pos)].TryGetValue(GetLocalPos(pos), out BlockInChunkData data))
            return;

        bool covered = IsFullyCovered(pos);
        data._go.SetActive(!covered);
    }

    void UpdateAroundVisibility(Vector3Int pos)
    {
        foreach (var dir in directions)
        {
            Vector3Int nPos = pos + dir;

            if (!BlocksinChunk.ContainsKey(GetChunkCoord(nPos)))
                continue;

            if (!BlocksinChunk[GetChunkCoord(nPos)].TryGetValue(GetLocalPos(nPos), out BlockInChunkData data))
                continue;

            if(data._go == null)
                continue;

            bool covered = IsFullyCovered(nPos);
            data._go.SetActive(!covered);
        }
    }


    IEnumerator BuildCombinedWorldColliderCoroutine()
    {
        if (BlocksinChunk == null || BlocksinChunk.Count == 0)
            yield break;

        // Combined Collider Object 준비
        if (CombinedColliderObject == null)
        {
            CombinedColliderObject = new GameObject("CombinedWorldCollider");
            CombinedColliderObject.transform.SetParent(transform, false);
            CombinedColliderObject.transform.localPosition = Vector3.zero;
            CombinedColliderObject.transform.localRotation = Quaternion.identity;
            CombinedColliderObject.transform.localScale = Vector3.one;
            CombinedWorldCollider = CombinedColliderObject.AddComponent<MeshCollider>();
        }
        else if (CombinedWorldCollider == null)
        {
            CombinedWorldCollider = CombinedColliderObject.GetComponent<MeshCollider>();
            if (CombinedWorldCollider == null)
                CombinedWorldCollider = CombinedColliderObject.AddComponent<MeshCollider>();
        }

        CombinedColliderObject.layer = gameObject.layer;
        CombinedColliderObject.tag = "World";

        bool useReusableColliderBuffers =
            !combinedColliderCoroutineBufferInUse;

        Dictionary<int, HashSet<Vector2Int>>[] planes;
        Stack<HashSet<Vector2Int>> cellPool;
        List<Vector3> vertices;
        List<int> triangles;

        if (useReusableColliderBuffers)
        {
            combinedColliderCoroutineBufferInUse = true;

            planes = combinedColliderCoroutinePlanes;
            cellPool = combinedColliderCoroutineCellPool;
            vertices = combinedColliderCoroutineVertices;
            triangles = combinedColliderCoroutineTriangles;

            ResetColliderPlaneBuffer(planes, cellPool);
            vertices.Clear();
            triangles.Clear();
        }
        else
        {
            // Same ChunkCode coroutine started again before the previous one finished:
            // use temporary buffers so the two builds cannot overwrite each other.
            planes = CreateColliderPlaneBuffer();
            cellPool = new Stack<HashSet<Vector2Int>>();
            vertices = new List<Vector3>();
            triangles = new List<int>();
        }

        float start = Time.realtimeSinceStartup;

        // 월드 전체 노출면 수집
        foreach (var chunk in BlocksinChunk.Values)
        {
            foreach (BlockInChunkData block in chunk.Values)
            {
                Vector3Int worldPos = block._position;

                for (int face = 0; face < 6; face++)
                {
                    Vector3Int neighborPos = worldPos + directions[face];

                    if (IsColliderBlock(neighborPos))
                        continue;

                    GetWorldColliderPlanePosition(
                        face,
                        worldPos,
                        out int slice,
                        out int u,
                        out int v
                    );

                    HashSet<Vector2Int> cells = GetColliderPlaneCells(
                        planes,
                        cellPool,
                        face,
                        slice
                    );

                    cells.Add(new Vector2Int(u, v));
                }

                if ((Time.realtimeSinceStartup - start) * 1000f >= combineFrameTime)
                {
                    yield return null;
                    start = Time.realtimeSinceStartup;
                }
            }
        }

        // Greedy Merge도 프레임 분할
        for (int face = 0; face < 6; face++)
        {
            foreach (var plane in planes[face])
            {
                yield return StartCoroutine(
                    MergeWorldColliderPlaneCoroutine(
                        face,
                        plane.Key,
                        plane.Value,
                        vertices,
                        triangles
                    )
                );
            }
        }

        Mesh oldMesh = CombinedWorldCollider.sharedMesh;
        CombinedWorldCollider.sharedMesh = null;

        if (oldMesh != null)
            Destroy(oldMesh);

        if (vertices.Count == 0)
        {
            CombinedColliderObject.SetActive(false);

            if (useReusableColliderBuffers)
                combinedColliderCoroutineBufferInUse = false;

            yield break;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedWorldGreedyCollider";
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        combinedMesh.SetVertices(vertices);
        yield return null;

        combinedMesh.SetTriangles(triangles, 0);
        combinedMesh.RecalculateBounds();
        yield return null;

        // PhysX cooking 자체는 한 번에 실행되므로 앞 작업과 프레임을 분리해서 호출
        CombinedWorldCollider.sharedMesh = combinedMesh;
        CombinedColliderObject.SetActive(true);

        if (useReusableColliderBuffers)
            combinedColliderCoroutineBufferInUse = false;
    }

    IEnumerator MergeWorldColliderPlaneCoroutine(
        int face,
        int slice,
        HashSet<Vector2Int> cells,
        List<Vector3> vertices,
        List<int> triangles
    )
    {
        float startTime = Time.realtimeSinceStartup;

        while (cells.Count > 0)
        {
            Vector2Int first = cells.First();

            int minU = first.x;
            int maxU = first.x;

            while (cells.Contains(new Vector2Int(minU - 1, first.y)))
                minU--;

            while (cells.Contains(new Vector2Int(maxU + 1, first.y)))
                maxU++;

            int minV = first.y;
            int maxV = first.y;

            while (HasCompleteWorldColliderRow(cells, minU, maxU, minV - 1))
                minV--;

            while (HasCompleteWorldColliderRow(cells, minU, maxU, maxV + 1))
                maxV++;

            int width = maxU - minU + 1;
            int height = maxV - minV + 1;

            for (int v = minV; v <= maxV; v++)
            {
                for (int u = minU; u <= maxU; u++)
                {
                    cells.Remove(new Vector2Int(u, v));

                    if ((Time.realtimeSinceStartup - startTime) * 1000f >= combineFrameTime)
                    {
                        yield return null;
                        startTime = Time.realtimeSinceStartup;
                    }
                }
            }

            AddWorldColliderQuad(
                face,
                slice,
                minU,
                minV,
                width,
                height,
                vertices,
                triangles
            );

            if ((Time.realtimeSinceStartup - startTime) * 1000f >= combineFrameTime)
            {
                yield return null;
                startTime = Time.realtimeSinceStartup;
            }
        }
    }

    IEnumerator UseCombinedWorldColliderCoroutine()
    {
        yield return StartCoroutine(BuildCombinedWorldColliderCoroutine());

        if (CombinedColliderObject != null &&
            CombinedWorldCollider != null &&
            CombinedWorldCollider.sharedMesh != null)
        {
            CombinedColliderObject.SetActive(true);
        }
    }

    IEnumerator BuildCombinedWorldRenderCoroutine()
    {
        if (Chunks == null || Chunks.Count == 0)
            yield break;

        // Combined Render Object 준비
        if (CombinedRenderObject == null)
        {
            CombinedRenderObject = new GameObject("CombinedWorldRenderer");
            CombinedRenderObject.transform.SetParent(transform, false);
            CombinedRenderObject.transform.localPosition = Vector3.zero;
            CombinedRenderObject.transform.localRotation = Quaternion.identity;
            CombinedRenderObject.transform.localScale = Vector3.one;

            CombinedWorldMeshFilter = CombinedRenderObject.AddComponent<MeshFilter>();
            CombinedWorldMeshRenderer = CombinedRenderObject.AddComponent<MeshRenderer>();
            CombinedWorldMeshRenderer.sharedMaterial =
                GameManager.Instance.MaterialManager.MainMaterial;
        }
        else
        {
            if (CombinedWorldMeshFilter == null)
                CombinedWorldMeshFilter = CombinedRenderObject.GetComponent<MeshFilter>();

            if (CombinedWorldMeshRenderer == null)
                CombinedWorldMeshRenderer = CombinedRenderObject.GetComponent<MeshRenderer>();
        }

        CombinedRenderObject.layer = gameObject.layer;
        CombinedRenderObject.tag = "World";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();

        float start = Time.realtimeSinceStartup;

        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;

            MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh.vertexCount == 0)
                continue;

            Vector3[] sourceVertices = mesh.vertices;
            int[] sourceTriangles = mesh.triangles;
            Color[] sourceColors = mesh.colors;
            Vector3[] sourceNormals = mesh.normals;

            Matrix4x4 matrix =
                CombinedRenderObject.transform.worldToLocalMatrix *
                chunk.transform.localToWorldMatrix;

            Matrix4x4 normalMatrix = matrix.inverse.transpose;
            int vertexOffset = vertices.Count;

            for (int i = 0; i < sourceVertices.Length; i++)
            {
                vertices.Add(matrix.MultiplyPoint3x4(sourceVertices[i]));

                if (sourceColors != null && sourceColors.Length == sourceVertices.Length)
                    colors.Add(sourceColors[i]);
                else
                    colors.Add(Color.white);

                if (sourceNormals != null && sourceNormals.Length == sourceVertices.Length)
                    normals.Add(normalMatrix.MultiplyVector(sourceNormals[i]).normalized);
                else
                    normals.Add(Vector3.zero);

                if ((Time.realtimeSinceStartup - start) * 1000f >= combineFrameTime)
                {
                    yield return null;
                    start = Time.realtimeSinceStartup;
                }
            }

            for (int i = 0; i < sourceTriangles.Length; i++)
            {
                triangles.Add(vertexOffset + sourceTriangles[i]);

                if ((Time.realtimeSinceStartup - start) * 1000f >= combineFrameTime)
                {
                    yield return null;
                    start = Time.realtimeSinceStartup;
                }
            }
        }

        if (vertices.Count == 0)
        {
            Mesh oldEmptyMesh = CombinedWorldMeshFilter.sharedMesh;
            CombinedWorldMeshFilter.sharedMesh = null;

            if (oldEmptyMesh != null)
                Destroy(oldEmptyMesh);

            CombinedRenderObject.SetActive(false);
            yield break;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedWorldRenderMesh";
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        combinedMesh.SetVertices(vertices);
        yield return null;

        combinedMesh.SetTriangles(triangles, 0);
        yield return null;

        combinedMesh.SetColors(colors);
        combinedMesh.SetNormals(normals);
        combinedMesh.RecalculateBounds();
        yield return null;

        Mesh previousMesh = CombinedWorldMeshFilter.sharedMesh;
        CombinedWorldMeshFilter.sharedMesh = null;

        if (previousMesh != null)
            Destroy(previousMesh);

        CombinedWorldMeshFilter.sharedMesh = combinedMesh;
        CombinedWorldMeshRenderer.sharedMaterial =
            GameManager.Instance.MaterialManager.MainMaterial;

        CombinedRenderObject.SetActive(true);
    }

    IEnumerator UseCombinedWorldRendererCoroutine()
    {
        yield return StartCoroutine(BuildCombinedWorldRenderCoroutine());

        if (CombinedRenderObject != null &&
            CombinedWorldMeshFilter != null &&
            CombinedWorldMeshFilter.sharedMesh != null)
        {
            CombinedRenderObject.SetActive(true);
        }
    }

    void BuildCombinedWorldCollider()
    {
        if (BlocksinChunk == null ||
            BlocksinChunk.Count == 0)
        {
            return;
        }


        // =====================================
        // Combined Collider Object 준비
        // =====================================

        if (CombinedColliderObject == null)
        {
            CombinedColliderObject =
                new GameObject("CombinedWorldCollider");

            CombinedColliderObject.transform.SetParent(
                transform,
                false
            );

            CombinedColliderObject.transform.localPosition =
                Vector3.zero;

            CombinedColliderObject.transform.localRotation =
                Quaternion.identity;

            CombinedColliderObject.transform.localScale =
                Vector3.one;


            CombinedWorldCollider =
                CombinedColliderObject.AddComponent<MeshCollider>();
        }
        else
        {
            if (CombinedWorldCollider == null)
            {
                CombinedWorldCollider =
                    CombinedColliderObject.GetComponent<MeshCollider>();

                if (CombinedWorldCollider == null)
                {
                    CombinedWorldCollider =
                        CombinedColliderObject.AddComponent<MeshCollider>();
                }
            }
        }


        CombinedColliderObject.layer =
            gameObject.layer;

        CombinedColliderObject.tag =
            "World";


        // =====================================
        // 월드 전체의 노출면 수집
        // =====================================

        Dictionary<int, HashSet<Vector2Int>>[] planes =
            combinedColliderPlanes;

        ResetColliderPlaneBuffer(
            planes,
            combinedColliderCellPool
        );


        foreach (var chunk in BlocksinChunk.Values)
        {
            foreach (BlockInChunkData block in chunk.Values)
            {
                Vector3Int worldPos =
                    block._position;


                for (int face = 0; face < 6; face++)
                {
                    Vector3Int neighborPos =
                        worldPos + directions[face];


                    // 옆에도 블록이 있으면 내부면
                    if (IsColliderBlock(neighborPos))
                        continue;


                    GetWorldColliderPlanePosition(
                        face,
                        worldPos,

                        out int slice,
                        out int u,
                        out int v
                    );


                    HashSet<Vector2Int> cells = GetColliderPlaneCells(
                        planes,
                        combinedColliderCellPool,
                        face,
                        slice
                    );

                    cells.Add(
                        new Vector2Int(u, v)
                    );
                }
            }
        }


        // =====================================
        // 월드 전체 Greedy Merge
        // =====================================

        List<Vector3> vertices = combinedColliderVertices;
        List<int> triangles = combinedColliderTriangles;

        vertices.Clear();
        triangles.Clear();


        for (int face = 0; face < 6; face++)
        {
            foreach (var plane in planes[face])
            {
                int slice =
                    plane.Key;

                HashSet<Vector2Int> cells =
                    plane.Value;


                MergeWorldColliderPlane(
                    face,
                    slice,
                    cells,

                    vertices,
                    triangles
                );
            }
        }


        // =====================================
        // 기존 Combined Mesh 제거
        // =====================================

        Mesh oldMesh =
            CombinedWorldCollider.sharedMesh;

        CombinedWorldCollider.sharedMesh =
            null;


        if (oldMesh != null)
        {
            Destroy(oldMesh);
        }


        if (vertices.Count == 0)
        {
            CombinedColliderObject.SetActive(false);

            return;
        }


        // =====================================
        // 새 Combined Collider Mesh
        // =====================================

        Mesh combinedMesh =
            new Mesh();

        combinedMesh.name =
            "CombinedWorldGreedyCollider";


        combinedMesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;


        combinedMesh.SetVertices(
            vertices
        );

        combinedMesh.SetTriangles(
            triangles,
            0
        );

        combinedMesh.RecalculateBounds();


        CombinedWorldCollider.sharedMesh =
            combinedMesh;


        CombinedColliderObject.SetActive(true);
    }

    void BuildCombinedWorldRender()
    {
        if (Chunks == null || Chunks.Count == 0)
            return;


        // =========================
        // Combined Render Object 준비
        // =========================

        if (CombinedRenderObject == null)
        {
            CombinedRenderObject =
                new GameObject("CombinedWorldRenderer");

            CombinedRenderObject.transform.SetParent(
                transform,
                false
            );

            CombinedRenderObject.transform.localPosition =
                Vector3.zero;

            CombinedRenderObject.transform.localRotation =
                Quaternion.identity;

            CombinedRenderObject.transform.localScale =
                Vector3.one;


            CombinedWorldMeshFilter =
                CombinedRenderObject.AddComponent<MeshFilter>();

            CombinedWorldMeshRenderer =
                CombinedRenderObject.AddComponent<MeshRenderer>();


            CombinedWorldMeshRenderer.sharedMaterial =
                GameManager.Instance
                    .MaterialManager
                    .MainMaterial;
        }
        else
        {
            if (CombinedWorldMeshFilter == null)
            {
                CombinedWorldMeshFilter =
                    CombinedRenderObject.GetComponent<MeshFilter>();
            }

            if (CombinedWorldMeshRenderer == null)
            {
                CombinedWorldMeshRenderer =
                    CombinedRenderObject.GetComponent<MeshRenderer>();
            }
        }


        CombinedRenderObject.layer =
            gameObject.layer;

        CombinedRenderObject.tag =
            "World";


        // =========================
        // Chunk Render Mesh 수집
        // =========================

        List<CombineInstance> combines =
            new List<CombineInstance>();


        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;


            MeshFilter chunkMeshFilter =
                chunk.GetComponent<MeshFilter>();

            if (chunkMeshFilter == null)
                continue;


            Mesh chunkMesh =
                chunkMeshFilter.sharedMesh;

            if (chunkMesh == null)
                continue;

            if (chunkMesh.vertexCount == 0)
                continue;


            CombineInstance combine =
                new CombineInstance();

            combine.mesh =
                chunkMesh;


            // Chunk Local 좌표
            // ↓
            // CombinedWorldRenderer Local 좌표
            combine.transform =
                CombinedRenderObject.transform.worldToLocalMatrix
                *
                chunk.transform.localToWorldMatrix;


            combines.Add(combine);
        }


        // =========================
        // 그릴 것이 없음
        // =========================

        if (combines.Count == 0)
        {
            Mesh oldMesh =
                CombinedWorldMeshFilter.sharedMesh;

            CombinedWorldMeshFilter.sharedMesh =
                null;


            if (oldMesh != null)
            {
                Destroy(oldMesh);
            }


            CombinedRenderObject.SetActive(false);

            return;
        }


        // =========================
        // 통합 Mesh 생성
        // =========================

        Mesh combinedMesh =
            new Mesh();

        combinedMesh.name =
            "CombinedWorldRenderMesh";


        combinedMesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;


        combinedMesh.CombineMeshes(
            combines.ToArray(),

            true,   // 모든 SubMesh 하나로
            true    // Transform 적용
        );


        combinedMesh.RecalculateBounds();


        // =========================
        // 기존 통합 Mesh 제거
        // =========================

        Mesh previousMesh =
            CombinedWorldMeshFilter.sharedMesh;

        CombinedWorldMeshFilter.sharedMesh =
            null;


        if (previousMesh != null)
        {
            Destroy(previousMesh);
        }


        // =========================
        // 새 Mesh 적용
        // =========================

        CombinedWorldMeshFilter.sharedMesh =
            combinedMesh;


        CombinedWorldMeshRenderer.sharedMaterial =
            GameManager.Instance
                .MaterialManager
                .MainMaterial;


        CombinedRenderObject.SetActive(true);
    }

    void UseChunkRenderers()
    {
        SetEditingRepresentation();
    }

    void UseCombinedWorldRenderer()
    {
        BuildCombinedWorldRender();

        if (CombinedRenderObject != null &&
            CombinedWorldMeshFilter != null &&
            CombinedWorldMeshFilter.sharedMesh != null)
        {
            CombinedRenderObject.SetActive(true);
        }
    }

    void GetWorldColliderPlanePosition(
    int face,
    Vector3Int worldPos,

    out int slice,
    out int u,
    out int v
)
    {
        switch (face)
        {
            // Front / Back
            // X,Y 평면
            case 0:
            case 1:

                slice = worldPos.z;
                u = worldPos.x;
                v = worldPos.y;

                break;


            // Left / Right
            // Z,Y 평면
            case 2:
            case 3:

                slice = worldPos.x;
                u = worldPos.z;
                v = worldPos.y;

                break;


            // Top / Bottom
            // X,Z 평면
            default:

                slice = worldPos.y;
                u = worldPos.x;
                v = worldPos.z;

                break;
        }
    }

    void MergeWorldColliderPlane(
    int face,
    int slice,

    HashSet<Vector2Int> cells,

    List<Vector3> vertices,
    List<int> triangles
)
    {
        while (cells.Count > 0)
        {
            Vector2Int start =
                cells.First();


            int minU =
                start.x;

            int maxU =
                start.x;


            // 왼쪽으로 확장
            while (
                cells.Contains(
                    new Vector2Int(
                        minU - 1,
                        start.y
                    )
                )
            )
            {
                minU--;
            }


            // 오른쪽으로 확장
            while (
                cells.Contains(
                    new Vector2Int(
                        maxU + 1,
                        start.y
                    )
                )
            )
            {
                maxU++;
            }


            int minV =
                start.y;

            int maxV =
                start.y;


            // 아래 방향으로 확장
            while (
                HasCompleteWorldColliderRow(
                    cells,
                    minU,
                    maxU,
                    minV - 1
                )
            )
            {
                minV--;
            }


            // 위 방향으로 확장
            while (
                HasCompleteWorldColliderRow(
                    cells,
                    minU,
                    maxU,
                    maxV + 1
                )
            )
            {
                maxV++;
            }


            int width =
                maxU - minU + 1;

            int height =
                maxV - minV + 1;


            // 사용한 Cell 제거
            for (
                int v = minV;
                v <= maxV;
                v++
            )
            {
                for (
                    int u = minU;
                    u <= maxU;
                    u++
                )
                {
                    cells.Remove(
                        new Vector2Int(u, v)
                    );
                }
            }


            AddWorldColliderQuad(
                face,
                slice,

                minU,
                minV,

                width,
                height,

                vertices,
                triangles
            );
        }
    }

    bool HasCompleteWorldColliderRow(
    HashSet<Vector2Int> cells,

    int minU,
    int maxU,

    int v
)
    {
        for (
            int u = minU;
            u <= maxU;
            u++
        )
        {
            if (
                !cells.Contains(
                    new Vector2Int(u, v)
                )
            )
            {
                return false;
            }
        }


        return true;
    }

    void AddWorldColliderQuad(
    int face,
    int slice,

    int u,
    int v,

    int width,
    int height,

    List<Vector3> vertices,
    List<int> triangles
)
    {
        float u0 =
            u - 0.5f;

        float u1 =
            u + width - 0.5f;

        float v0 =
            v - 0.5f;

        float v1 =
            v + height - 0.5f;


        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;


        switch (face)
        {
            // Front +Z
            case 0:
                {
                    float z =
                        slice + 0.5f;

                    p0 = new Vector3(u0, v0, z);
                    p1 = new Vector3(u1, v0, z);
                    p2 = new Vector3(u1, v1, z);
                    p3 = new Vector3(u0, v1, z);

                    break;
                }


            // Back -Z
            case 1:
                {
                    float z =
                        slice - 0.5f;

                    p0 = new Vector3(u1, v0, z);
                    p1 = new Vector3(u0, v0, z);
                    p2 = new Vector3(u0, v1, z);
                    p3 = new Vector3(u1, v1, z);

                    break;
                }


            // Left -X
            case 2:
                {
                    float x =
                        slice - 0.5f;

                    p0 = new Vector3(x, v0, u0);
                    p1 = new Vector3(x, v0, u1);
                    p2 = new Vector3(x, v1, u1);
                    p3 = new Vector3(x, v1, u0);

                    break;
                }


            // Right +X
            case 3:
                {
                    float x =
                        slice + 0.5f;

                    p0 = new Vector3(x, v0, u1);
                    p1 = new Vector3(x, v0, u0);
                    p2 = new Vector3(x, v1, u0);
                    p3 = new Vector3(x, v1, u1);

                    break;
                }


            // Top +Y
            case 4:
                {
                    float y =
                        slice + 0.5f;

                    p0 = new Vector3(u0, y, v1);
                    p1 = new Vector3(u1, y, v1);
                    p2 = new Vector3(u1, y, v0);
                    p3 = new Vector3(u0, y, v0);

                    break;
                }


            // Bottom -Y
            default:
                {
                    float y =
                        slice - 0.5f;

                    p0 = new Vector3(u0, y, v0);
                    p1 = new Vector3(u1, y, v0);
                    p2 = new Vector3(u1, y, v1);
                    p3 = new Vector3(u0, y, v1);

                    break;
                }
        }


        int index =
            vertices.Count;


        vertices.Add(p0);
        vertices.Add(p1);
        vertices.Add(p2);
        vertices.Add(p3);


        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index + 0);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
    }

    void UseChunkColliders()
    {
        SetEditingRepresentation();
    }

    void UseCombinedWorldCollider()
    {
        BuildCombinedWorldCollider();

        if (CombinedColliderObject != null &&
            CombinedWorldCollider != null &&
            CombinedWorldCollider.sharedMesh != null)
        {
            CombinedColliderObject.SetActive(true);
        }
    }

    public void OnBlockDestroyed(Vector3Int pos)
    {
        foreach (var dir in directions)
        {
            Vector3Int nPos = pos + dir;

            if (!BlocksinChunk.ContainsKey(GetChunkCoord(nPos)))
                continue;

            if (!BlocksinChunk[GetChunkCoord(nPos)].TryGetValue(GetLocalPos(nPos), out BlockInChunkData data))
                continue;

            if (data._go == null)
                continue;

            data._go.SetActive(true);
        }
    }

    BlockFace GetVisibleFaces(Vector3Int pos, BlockInChunkData data)
    {
        BlockFace faces = BlockFace.All;

        foreach (var dir in directions)
        {
            Vector3Int nPos = pos + dir;

            if (!BlocksinChunk.ContainsKey(GetChunkCoord(nPos)))
                continue;

            if (!BlocksinChunk[GetChunkCoord(nPos)].TryGetValue(GetLocalPos(nPos), out BlockInChunkData n))
                continue;

            if ((n._color.a == 1) != (data._color.a == 1))
                continue;

            if (dir == Vector3Int.up) faces &= ~BlockFace.Up;
            if (dir == Vector3Int.down) faces &= ~BlockFace.Down;
            if (dir == Vector3Int.left) faces &= ~BlockFace.Left;
            if (dir == Vector3Int.right) faces &= ~BlockFace.Right;
            if (dir == new Vector3Int(0, 0, 1)) faces &= ~BlockFace.Front;
            if (dir == new Vector3Int(0, 0, -1)) faces &= ~BlockFace.Back;
        }

        return faces;
    }

    void UpdateBlockMesh(Vector3Int pos)
    {
        if (!BlocksinChunk.ContainsKey(GetChunkCoord(pos)))
            return;

        if (!BlocksinChunk[GetChunkCoord(pos)].TryGetValue(GetLocalPos(pos), out BlockInChunkData data))
            return;

        if (data._go == null)
            return;

        var meshGen = data._go.GetComponent<BlockMeshGenerator>();
        meshGen.Build(GetVisibleFaces(pos, data));
    }

    public void StartChangeEveryThingSmooth(float duration, 
        bool changescale, float _scale, 
        bool changeposition, Vector3 _position, 
        bool changerotation, Quaternion _rotation)
    {
        StartCoroutine(ChangeEveryThingSmooth(duration, changescale, _scale, changeposition, _position, changerotation, _rotation));
    }

    IEnumerator ChangeEveryThingSmooth(float duration,
        bool changescale, float _scale,
        bool changeposition, Vector3 _position,
        bool changerotation, Quaternion _rotation)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * _scale;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = _position;

        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = _rotation;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            if (changescale) transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            if (changeposition) transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
            if (changerotation) transform.localRotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
            yield return null;
        }
    }

    public Light ChangeLightPower()
    {
        if (LightBlock == null)
        {
            LightBlock =
                new GameObject("LightObject");

            LightBlock.transform.SetParent(
                transform,
                false
            );

            // 최초 생성 순간의 임시 위치
            LightBlock.transform.localPosition =
                Vector3.zero;

            LightBlock.transform.localRotation =
                Quaternion.identity;
        }

        LightBlock.TryGetComponent<Light>(
            out Light light
        );

        if (light == null)
        {
            light =
                LightBlock.AddComponent<Light>();

            light.type =
                LightType.Point;

            light.range = 0;
            light.intensity = 0;

            light.cullingMask =
                ~(1 << 7);
        }

        return light;
    }

    public void SetLightPosition()
    {
        if (LightBlock != null)
            LightBlock.transform.position = gameObject.transform.position;
    }

    void CheckIsLive()
    {
        if(BlocksinChunk.Count == 0)
        {
            DestroyWorld();
        }
    }

    public void ChangeWorldLayer(int num)
    {
        gameObject.layer = num;

        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform)
                continue;

            children[i].gameObject.layer = num;
        }
    }

    public Color CheckColor(Vector3 pos)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(pos);
        Vector3Int ChunkPos = GetChunkCoord(gridPos);
        Vector3Int LocalPos = GetLocalPos(gridPos);

        return BlocksinChunk[ChunkPos][LocalPos]._color;
    }

    public bool ChangeBlockColor(Vector3 pos, Color color)
    {
        if (BlocksinChunk == null) return false;
        if (Chunks == null) return false;

        Vector3Int gridPos = Vector3Int.RoundToInt(pos);
        Vector3Int chunkPos = GetChunkCoord(gridPos);
        Vector3Int localPos = GetLocalPos(gridPos);

        if (!BlocksinChunk.TryGetValue(chunkPos, out var chunkBlocks))
            return false;

        if (!chunkBlocks.TryGetValue(localPos, out BlockInChunkData block))
            return false;

        bool beforeAlphaBlock = block._color.a < 1f;
        bool afterAlphaBlock = color.a < 1f;

        block._color = color;

        if (afterAlphaBlock)
        {
            MakeAlphaBlockObjectIfNeed(gridPos, block);

            if (block._go != null)
            {
                block._go.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            if (block._go != null)
            {
                Destroy(block._go);
                block._go = null;
            }
        }

        RebuildAfterBlockColorChange(gridPos);

        return true;
    }

    public void ChangeBlockColor(List<Vector3> positions, Color color)
    {
        if (positions == null) return;

        for (int i = 0; i < positions.Count; i++)
        {
            ChangeBlockColor(positions[i], color);
        }
    }

    private void MakeAlphaBlockObjectIfNeed(Vector3Int gridPos, BlockInChunkData block)
    {
        if (block == null) return;
        if (block._go != null) return;

        GameObject alphaBlock = Instantiate(GameManager.Instance.BuildManager.ChooseBlock, transform);
        alphaBlock.transform.localPosition = gridPos;
        alphaBlock.transform.localRotation = Quaternion.identity;
        alphaBlock.transform.localScale = Vector3.one;

        alphaBlock.GetComponent<Renderer>().material.color = block._color;
        alphaBlock.tag = "Block";
        alphaBlock.layer = gameObject.layer;

        block._go = alphaBlock;
    }

    private void RebuildAfterBlockColorChange(Vector3Int gridPos)
    {
        BuildMesh(gridPos);
        trytoBuildMesh(gridPos);

        foreach (Vector3Int dir in directions)
        {
            BuildMesh(gridPos + dir);
            UpdateBlockMesh(gridPos + dir);
        }

        UpdateBlockMesh(gridPos);

        if (BlocksinChunk.ContainsKey(GetChunkCoord(gridPos)))
        {
            if (BlocksinChunk[GetChunkCoord(gridPos)].TryGetValue(GetLocalPos(gridPos), out BlockInChunkData data))
            {
                if (data._go != null)
                {
                    UpdateSelfVisibility(gridPos);
                }
            }
        }

        UpdateAroundVisibility(gridPos);
    }

    public void ChangeBlockColorAsync(Vector3 pos, Color color)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(pos);

        colorChangeQueue.Enqueue(new BlockColorChangeRequest(gridPos, color));

        StartColorChangeWorkerIfNeed();
    }

    public void ChangeBlockColorRangeAsync(Vector3 posA, Vector3 posB, Color color)
    {
        StartCoroutine(EnqueueColorChangeRangeCoroutine(posA, posB, color));
    }

    IEnumerator EnqueueColorChangeRangeCoroutine(Vector3 posA, Vector3 posB, Color color)
    {
        isColorChangeRangeEnqueueRunning = true;

        Vector3 A = posA;
        Vector3 B = posB;

        int minX = Mathf.Min(Mathf.RoundToInt(A.x), Mathf.RoundToInt(B.x));
        int maxX = Mathf.Max(Mathf.RoundToInt(A.x), Mathf.RoundToInt(B.x));

        int minY = Mathf.Min(Mathf.RoundToInt(A.y), Mathf.RoundToInt(B.y));
        int maxY = Mathf.Max(Mathf.RoundToInt(A.y), Mathf.RoundToInt(B.y));

        int minZ = Mathf.Min(Mathf.RoundToInt(A.z), Mathf.RoundToInt(B.z));
        int maxZ = Mathf.Max(Mathf.RoundToInt(A.z), Mathf.RoundToInt(B.z));

        float start = Time.realtimeSinceStartup;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3Int gridPos = new Vector3Int(x, y, z);

                    colorChangeQueue.Enqueue(new BlockColorChangeRequest(gridPos, color));

                    if ((Time.realtimeSinceStartup - start) * 1000f > colorChangeFrameTime)
                    {
                        StartColorChangeWorkerIfNeed();

                        yield return null;
                        start = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        StartColorChangeWorkerIfNeed();

        isColorChangeRangeEnqueueRunning = false;
    }

    void StartColorChangeWorkerIfNeed()
    {
        if (isColorChangeRunning) return;

        StartCoroutine(ColorChangeWorker());
    }

    IEnumerator ColorChangeWorker()
    {
        isColorChangeRunning = true;

        while (true)
        {
            float start = Time.realtimeSinceStartup;

            while (colorChangeQueue.Count > 0)
            {
                BlockColorChangeRequest request = colorChangeQueue.Dequeue();

                ChangeBlockColorDataOnly(request.gridPos, request.color);

                if ((Time.realtimeSinceStartup - start) * 1000f > colorChangeFrameTime)
                {
                    yield return null;
                    start = Time.realtimeSinceStartup;
                }
            }

            yield return StartCoroutine(RebuildColorChangedMeshes());

            if (colorChangeQueue.Count <= 0 && !isColorChangeRangeEnqueueRunning)
                break;
        }

        isColorChangeRunning = false;

        InvokeColorChangeCompleteActions();
    }

    void InvokeColorChangeCompleteActions()
    {
        if (colorChangeCompleteActions.Count <= 0)
            return;

        List<System.Action> actions = new List<System.Action>(colorChangeCompleteActions);
        colorChangeCompleteActions.Clear();

        for (int i = 0; i < actions.Count; i++)
        {
            actions[i]?.Invoke();
        }
    }

    void ChangeBlockColorDataOnly(Vector3Int gridPos, Color color)
    {
        if (BlocksinChunk == null) return;
        if (Chunks == null) return;

        Vector3Int chunkPos = GetChunkCoord(gridPos);
        Vector3Int localPos = GetLocalPos(gridPos);

        if (!BlocksinChunk.TryGetValue(chunkPos, out var chunkBlocks))
            return;

        if (!chunkBlocks.TryGetValue(localPos, out BlockInChunkData block))
            return;

        bool beforeTransparent = block._color.a < 1f;
        bool afterTransparent = color.a < 1f;

        block._color = color;

        if (afterTransparent)
        {
            MakeAlphaBlockObjectIfNeed(gridPos, block);

            if (block._go != null)
            {
                block._go.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            if (block._go != null)
            {
                Destroy(block._go);
                block._go = null;
            }
        }

        AddColorChangeRebuildTarget(gridPos);
    }

    void AddColorChangeRebuildTarget(Vector3Int gridPos)
    {
        Vector3Int chunkPos = GetChunkCoord(gridPos);

        colorChangeBuildMeshSet.Add(chunkPos);
        colorChangeUpdateBlockMeshSet.Add(gridPos);

        foreach (Vector3Int dir in directions)
        {
            Vector3Int nearPos = gridPos + dir;

            colorChangeBuildMeshSet.Add(GetChunkCoord(nearPos));
            colorChangeUpdateBlockMeshSet.Add(nearPos);
        }
    }

    public bool TryGetWorldBounds(out Bounds bounds)
    {
        bounds = new Bounds();

        bool hasBounds = false;

        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;

            Renderer renderer =
                chunk.GetComponent<Renderer>();

            if (renderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(
                    renderer.bounds
                );
            }
        }

        return hasBounds;
    }

    IEnumerator RebuildColorChangedMeshes()
    {
        float start = Time.realtimeSinceStartup;

        foreach (Vector3Int chunkPos in colorChangeBuildMeshSet)
        {
            BuildMesh(chunkPos * ChunkSize);

            if ((Time.realtimeSinceStartup - start) * 1000f > colorChangeFrameTime)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        colorChangeBuildMeshSet.Clear();

        start = Time.realtimeSinceStartup;

        foreach (Vector3Int blockPos in colorChangeUpdateBlockMeshSet)
        {
            UpdateBlockMesh(blockPos);
            UpdateSelfVisibilityIfAlphaObject(blockPos);
            UpdateAroundVisibility(blockPos);

            if ((Time.realtimeSinceStartup - start) * 1000f > colorChangeFrameTime)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        colorChangeUpdateBlockMeshSet.Clear();
    }

    void UpdateSelfVisibilityIfAlphaObject(Vector3Int pos)
    {
        if (!BlocksinChunk.ContainsKey(GetChunkCoord(pos)))
            return;

        if (!BlocksinChunk[GetChunkCoord(pos)].TryGetValue(GetLocalPos(pos), out BlockInChunkData data))
            return;

        if (data._go == null)
            return;

        bool covered = IsFullyCovered(pos);
        data._go.SetActive(!covered);
    }

    public void ChangeBlockColorAsync(Vector3 pos, Color color, System.Action onComplete = null)
    {
        if (onComplete != null)
            colorChangeCompleteActions.Add(onComplete);

        Vector3Int gridPos = Vector3Int.RoundToInt(pos);

        colorChangeQueue.Enqueue(new BlockColorChangeRequest(gridPos, color));

        StartColorChangeWorkerIfNeed();
    }

    public void ChangeBlockColorRangeAsync(Vector3 posA, Vector3 posB, Color color, System.Action onComplete = null)
    {
        if (onComplete != null)
            colorChangeCompleteActions.Add(onComplete);

        StartCoroutine(EnqueueColorChangeRangeCoroutine(posA, posB, color));
    }

    public void DestroyWorld()
    {
        GameManager.Instance.WorldManager.DestroyWorld(gameObject);
        GameManager.Instance.UIManager.WorldSeenUI.offScreenDirect();
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }
}
