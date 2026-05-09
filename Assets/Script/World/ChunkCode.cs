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

public class ChunkCode : MonoBehaviour
{
    public string _name = "NewWorld";
    public string Savedtime;

    public GameObject LightBlock;
    public bool isSavedBefore = false;

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

    IEnumerator BuildRoutine()
    {
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
                    if(newblock._color.a != 1)
                    {
                        MakeBlockData(newblock._position, newblock._color);
                    }
                    else
                    {
                        BlocksinChunk[chunkpos].Add(localPos,
                            new BlockInChunkData
                            {
                                _position = newblock._position,
                                _color = newblock._color
                            });
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
        Savedtime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
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
            case GameFlow.Start:
            case GameFlow.WatchAllWorld:
                PutThisWorld();
                GameManager.Instance.WorldManager.ClickLoadBtn();
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
        foreach(GameObject chunk in Chunks.Values)
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
        if(!BlocksinChunk.ContainsKey(chunkCoord)) return;

        List<Vector3> renderVertices = new();
        List<int> renderTriangles = new();
        List<Color> renderColors = new();
        int renderVertexIndex = 0;

        List<Vector3> colliderVertices = new();
        List<int> colliderTriangles = new();
        int colliderVertexIndex = 0;

        foreach (var kvp in BlocksinChunk[chunkCoord])
        {
            var block = kvp.Value;

            AddCube(
                chunkCoord,
                block,

                ref renderVertices,
                ref renderTriangles,
                ref renderColors,
                ref renderVertexIndex,

                ref colliderVertices,
                ref colliderTriangles,
                ref colliderVertexIndex
            );
        }

        Mesh renderMesh = new Mesh();
        renderMesh.SetVertices(renderVertices);
        renderMesh.SetTriangles(renderTriangles, 0);
        renderMesh.SetColors(renderColors);
        renderMesh.RecalculateNormals();
        renderMesh.RecalculateBounds();

        MeshFilter mf = Chunks[chunkCoord].GetComponent<MeshFilter>();
        mf.mesh = renderMesh;

        Mesh colliderMesh = new Mesh();
        colliderMesh.SetVertices(colliderVertices);
        colliderMesh.SetTriangles(colliderTriangles, 0);
        colliderMesh.RecalculateNormals();
        colliderMesh.RecalculateBounds();

        MeshCollider mc = Chunks[chunkCoord].GetComponent<MeshCollider>();
        if (mc == null)
            mc = Chunks[chunkCoord].AddComponent<MeshCollider>();

        mc.sharedMesh = null; // 중요
        mc.sharedMesh = colliderMesh;
    }

    void AddCube(
     Vector3Int chunkCoord,
     BlockInChunkData block,

     ref List<Vector3> renderVertices,
     ref List<int> renderTriangles,
     ref List<Color> renderColors,
     ref int renderVertexIndex,

     ref List<Vector3> colliderVertices,
     ref List<int> colliderTriangles,
     ref int colliderVertexIndex
 )
    {
        Vector3Int localPos = GetLocalPos(block._position);
        Vector3Int worldPos = block._position;

        bool currentOpaque = block._color.a >= 1f;

        Vector3[,] faceVerts = new Vector3[6, 4]
        {
        { new(-0.5f,-0.5f, 0.5f), new( 0.5f,-0.5f, 0.5f), new( 0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f) },
        { new( 0.5f,-0.5f,-0.5f), new(-0.5f,-0.5f,-0.5f), new(-0.5f, 0.5f,-0.5f), new( 0.5f, 0.5f,-0.5f) },
        { new(-0.5f,-0.5f,-0.5f), new(-0.5f,-0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f,-0.5f) },
        { new( 0.5f,-0.5f, 0.5f), new( 0.5f,-0.5f,-0.5f), new( 0.5f, 0.5f,-0.5f), new( 0.5f, 0.5f, 0.5f) },
        { new(-0.5f, 0.5f, 0.5f), new( 0.5f, 0.5f, 0.5f), new( 0.5f, 0.5f,-0.5f), new(-0.5f, 0.5f,-0.5f) },
        { new(-0.5f,-0.5f,-0.5f), new( 0.5f,-0.5f,-0.5f), new( 0.5f,-0.5f, 0.5f), new(-0.5f,-0.5f, 0.5f) }
        };

        for (int face = 0; face < 6; face++)
        {
            Vector3Int neighborWorld = worldPos + directions[face];
            Vector3Int neighborChunk = GetChunkCoord(neighborWorld);
            Vector3Int neighborLocal = GetLocalPos(neighborWorld);

            bool neighborExists = false;
            bool neighborOpaque = false;

            if (BlocksinChunk.ContainsKey(neighborChunk) &&
                BlocksinChunk[neighborChunk].TryGetValue(neighborLocal, out var neighborBlock))
            {
                neighborExists = true;
                neighborOpaque = neighborBlock._color.a >= 1f;
            }

            Vector3 p = localPos;

            if (!neighborExists)
            {
                for (int i = 0; i < 4; i++)
                    colliderVertices.Add(p + faceVerts[face, i]);

                colliderTriangles.Add(colliderVertexIndex + 0);
                colliderTriangles.Add(colliderVertexIndex + 1);
                colliderTriangles.Add(colliderVertexIndex + 2);
                colliderTriangles.Add(colliderVertexIndex + 0);
                colliderTriangles.Add(colliderVertexIndex + 2);
                colliderTriangles.Add(colliderVertexIndex + 3);

                colliderVertexIndex += 4;
            }

            if (!neighborOpaque && currentOpaque)
            {
                for (int i = 0; i < 4; i++)
                    renderVertices.Add(p + faceVerts[face, i]);

                renderTriangles.Add(renderVertexIndex + 0);
                renderTriangles.Add(renderVertexIndex + 1);
                renderTriangles.Add(renderVertexIndex + 2);
                renderTriangles.Add(renderVertexIndex + 0);
                renderTriangles.Add(renderVertexIndex + 2);
                renderTriangles.Add(renderVertexIndex + 3);

                for (int i = 0; i < 4; i++)
                    renderColors.Add(block._color);

                renderVertexIndex += 4;
            }
        }
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
        if (LightBlock == null) LightBlock = Instantiate(new GameObject("LightObject"), gameObject.transform);

        Light light;
        LightBlock.TryGetComponent<Light>(out light);
        if (light == null)
        {
            light = LightBlock.AddComponent<Light>();
            light.type = UnityEngine.LightType.Point;
            light.range = 0;
            light.intensity = 0; 
            light.cullingMask = ~(1 << 7);
            LightBlock.transform.position = gameObject.transform.position;
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

    public void DestroyWorld()
    {
        GameManager.Instance.WorldManager.DestroyWorld(gameObject);
        GameManager.Instance.UIManager.WorldSeenUI.offScreenDirect();
        GameManager.Instance.ChangeFlow(GameFlow.WatchAllWorld);
    }
}
