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
    public bool isSavedBefore = false;

    public GameObject CombinedColliderObject;
    MeshCollider CombinedWorldCollider;

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

    public IEnumerator BuildRoutine()
    {
        LoadState = WorldLoadState.Loading;

        if (BlocksinChunk == null)
            BlocksinChunk = new Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>>();

        if (Chunks == null)
            Chunks = new Dictionary<Vector3Int, GameObject>();

        float start = Time.realtimeSinceStartup;

        while (MakeWorldQueue.Count > 0)
        {
            start = Time.realtimeSinceStartup;

            while (MakeWorldQueue.Count > 0)
            {
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

        start = Time.realtimeSinceStartup;

        var chunkList = Chunks.Keys.ToList();

        for (int i = 0; i < chunkList.Count; i++)
        {
            BuildMesh(chunkList[i] * ChunkSize);

            if ((Time.realtimeSinceStartup - start) * 1000f > 10f)
            {
                yield return null;
                start = Time.realtimeSinceStartup;
            }
        }

        FinishSettingBlock();

        // 여기까지 왔으면 블록 + Chunk + Mesh 준비 완료
        LoadState = WorldLoadState.Complete;
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

        foreach(GameObject chunk in Chunks.Values)
        {
            Destroy(chunk);
        }
        Chunks.Clear();
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
        // 편집 상태에서는
        // Chunk별 Collider 사용
        UseChunkColliders();


        foreach (GameObject chunk in Chunks.Values)
        {
            chunk.tag = "Block";
        }
    }

    public void PutThisWorld()
    {
        foreach (GameObject chunk in Chunks.Values)
        {
            chunk.tag = "World";
        }


        // 설치 완료
        // →
        // 모든 Chunk Collider를 하나로 통합
        UseCombinedWorldCollider();
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

    void MakeNewChunk(Vector3Int _position)
    {
        GameObject _chunk = new GameObject($"Chunk_{_position}");

        _chunk.transform.parent = gameObject.transform;
        _chunk.transform.localPosition = _position * ChunkSize;
        _chunk.transform.localRotation = Quaternion.identity;
        _chunk.transform.localScale = Vector3.one;
        _chunk.tag = "Block";
        _chunk.layer = gameObject.layer;

        _chunk.AddComponent<MeshFilter>();
        _chunk.AddComponent<MeshRenderer>().material = new Material(GameManager.Instance.MaterialManager.MainMaterial);
        _chunk.AddComponent<MeshCollider>();

        Chunks.Add(_position, _chunk);
        BlocksinChunk.Add(_position, new Dictionary<Vector3Int, BlockInChunkData>());
    }

    public void BuildMesh(Vector3Int worldPos)
    {
        Vector3Int chunkCoord = GetChunkCoord(worldPos);

        if (!BlocksinChunk.ContainsKey(chunkCoord))
            return;


        // =========================
        // Render Mesh
        // =========================

        List<Vector3> renderVertices = new();
        List<int> renderTriangles = new();
        List<Color> renderColors = new();

        int renderVertexIndex = 0;


        foreach (var kvp in BlocksinChunk[chunkCoord])
        {
            BlockInChunkData block = kvp.Value;

            AddCube(
                chunkCoord,
                block,

                ref renderVertices,
                ref renderTriangles,
                ref renderColors,
                ref renderVertexIndex
            );
        }


        Mesh renderMesh = new Mesh();

        renderMesh.SetVertices(renderVertices);
        renderMesh.SetTriangles(renderTriangles, 0);
        renderMesh.SetColors(renderColors);

        renderMesh.RecalculateNormals();
        renderMesh.RecalculateBounds();


        MeshFilter mf =
            Chunks[chunkCoord].GetComponent<MeshFilter>();

        mf.mesh = renderMesh;


        // =========================
        // Collider Mesh
        // =========================

        BuildChunkCollider(chunkCoord);
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


        List<Vector3> vertices =
            new List<Vector3>();

        List<int> triangles =
            new List<int>();


        bool[,] mask =
            new bool[ChunkSize, ChunkSize];


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


                        Vector3Int worldPos =
                            chunkCoord * ChunkSize
                            + localPos;


                        // 현재 블록이 불투명이 아니면
                        // Collider 없음
                        if (!IsColliderBlock(worldPos))
                            continue;


                        Vector3Int neighborPos =
                            worldPos
                            + directions[face];


                        // 옆에도 Solid 블록이면
                        // 내부 면이므로 Collider 필요 없음
                        if (IsColliderBlock(neighborPos))
                            continue;


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
            GetLocalPos(worldPos);


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

    void BuildCombinedWorldCollider()
    {
        if (Chunks == null || Chunks.Count == 0)
            return;


        // =========================
        // 통합 Collider 오브젝트 준비
        // =========================

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


        // =========================
        // 청크 Collider Mesh 수집
        // =========================

        List<CombineInstance> combines =
            new List<CombineInstance>();


        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;


            MeshCollider chunkCollider =
                chunk.GetComponent<MeshCollider>();

            if (chunkCollider == null)
                continue;

            if (chunkCollider.sharedMesh == null)
                continue;


            CombineInstance combine =
                new CombineInstance();


            combine.mesh =
                chunkCollider.sharedMesh;


            // Chunk local mesh
            // →
            // CombinedCollider local 좌표로 변환
            combine.transform =
                CombinedColliderObject.transform.worldToLocalMatrix
                *
                chunk.transform.localToWorldMatrix;


            combines.Add(combine);
        }


        if (combines.Count == 0)
        {
            CombinedWorldCollider.sharedMesh = null;
            CombinedColliderObject.SetActive(false);

            return;
        }


        // =========================
        // 하나의 Mesh로 통합
        // =========================

        Mesh combinedMesh =
            new Mesh();

        combinedMesh.name =
            "CombinedWorldColliderMesh";


        // 큰 월드에서 정점 65535개 제한 방지
        combinedMesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;


        combinedMesh.CombineMeshes(
            combines.ToArray(),
            true,
            true
        );


        combinedMesh.RecalculateBounds();


        // 기존 통합 Mesh 정리
        Mesh oldMesh =
            CombinedWorldCollider.sharedMesh;

        CombinedWorldCollider.sharedMesh =
            null;


        if (oldMesh != null)
        {
            Destroy(oldMesh);
        }


        CombinedWorldCollider.sharedMesh =
            combinedMesh;


        CombinedColliderObject.SetActive(true);
    }

    void UseChunkColliders()
    {
        // 통합 Collider OFF
        if (CombinedColliderObject != null)
        {
            CombinedColliderObject.SetActive(false);
        }


        // 각 Chunk Collider ON
        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;


            MeshCollider collider =
                chunk.GetComponent<MeshCollider>();

            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    void UseCombinedWorldCollider()
    {
        // 현재 청크 Collider 결과를 합침
        BuildCombinedWorldCollider();


        // Chunk Collider 전부 OFF
        foreach (GameObject chunk in Chunks.Values)
        {
            if (chunk == null)
                continue;


            MeshCollider collider =
                chunk.GetComponent<MeshCollider>();

            if (collider != null)
            {
                collider.enabled = false;
            }
        }


        if (CombinedColliderObject != null)
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

        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = num;
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
