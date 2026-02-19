using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockInChunkData
{
    public Color _color;

    public Vector3Int _position;
    public GameObject _go;
}

public class ChunkCode : MonoBehaviour
{
    public Dictionary<Vector3Int, GameObject> Chunks; //어느 좌표에 있는 청크가 무슨 게임 오브젝트인지
    public Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>> BlocksinChunk; //어느 좌표에 있는 청크인지 + 그 안에 있는 블럭들
    int ChunkSize = 8;

    float minX = int.MaxValue;
    float minY = int.MaxValue;
    float minZ = int.MaxValue;

    float maxX = int.MinValue;
    float maxY = int.MinValue;
    float maxZ = int.MinValue;

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

        for (int _x =0; _x < x; _x++)
        {
            for(int _y = 0; _y < y; _y++)
            {
                for (int _z = 0; _z < z; _z++)
                {
                    Vector3 pos = new Vector3(_x, _y, _z);
                    Vector3Int gridPos = Vector3Int.RoundToInt(pos);

                    CheckSize(gridPos);

                    if (BlocksinChunk.ContainsKey(GetChunkCoord(gridPos)))
                    {
                        if (BlocksinChunk[GetChunkCoord(gridPos)].ContainsKey(GetLocalPos(gridPos)))
                            continue;
                    }

                    GameManager.Instance.BuildManager.RequestBuild
                        (new WorldAndBlock
                        {
                            world = this,
                            blockposition = pos,
                            blockcolor = Color.white
                        });
                }
            }
        }
    }

    public bool ishaveblock(Vector3 _position)
    {
        Vector3Int gridPos = Vector3Int.RoundToInt(_position);

        if (!BlocksinChunk.ContainsKey(GetChunkCoord(gridPos))) return false;
        if (!BlocksinChunk[GetChunkCoord(gridPos)].ContainsKey(GetLocalPos(gridPos))) return false;

        return true;
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
            BlocksinChunk[ChunkPos].Add(LocalPos, new BlockInChunkData { _position = LocalPos, _color = color }); //데이터 저장
        }

        if (color.a < 1)
        {
            GameObject block = Instantiate(GameManager.Instance.BuildManager.ChooseBlock, transform);
            block.transform.localPosition = gridPos;
            block.GetComponent<Renderer>().material.color = color;
            block.tag = "Block";

            BlocksinChunk[ChunkPos].TryGetValue(LocalPos, out BlockInChunkData value);
            value._go = block;

            OnBlockPlaced(gridPos);

            UpdateBlockMesh(gridPos);
            foreach (var dir in directions) UpdateBlockMesh(gridPos + dir);
        }

        BuildMesh(gridPos);

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
            case GameFlow.MakeOneWorld:
                MoveMiddle();
                findselect();
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
        gameObject.GetComponent<BoxCollider>().enabled = false;

        foreach(GameObject chunk in Chunks.Values)
        {
            var col = chunk.GetComponent<Collider>();
            if (col != null)
                col.enabled = true; // 끄기
        } 

        foreach (var chunk in BlocksinChunk.Values)
        {
            foreach (var block in chunk.Values)
            {
                if (block._go == null)
                    continue;

                var col = block._go.GetComponent<BoxCollider>();
                if (col != null)
                    col.enabled = true; // 끄기
            }
        }
    }

    public void MoveMiddle()
    {
        Vector3 centerChunkCoord = new Vector3(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f,
            (minZ + maxZ) / 2f
        );

        StartChangeEveryThingSmooth(0.5f,true ,1 /(Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ) + 2),false, Vector3.zero ,false, new Quaternion(0,0,0,0));
    }

    public void PutThisWorld()
    {
        gameObject.GetComponent<BoxCollider>().enabled = true;

        foreach (GameObject chunk in Chunks.Values)
        {
            var col = chunk.GetComponent<Collider>();
            if (col != null)
                col.enabled = false; // 끄기
        }

        foreach (var chunk in BlocksinChunk.Values)
        {
            foreach (var block in chunk.Values)
            {
                if (block._go == null)
                    continue;

                var col = block._go.GetComponent<BoxCollider>();
                if (col != null)
                    col.enabled = false; // 끄기
            }
        }
    }

    void CheckSize(Vector3 position)
    {
        BoxCollider col = gameObject.GetComponent<BoxCollider>();

        if (position.x > maxX) maxX = position.x;
        if (position.y > maxY) maxY = position.y;
        if (position.z > maxZ) maxZ = position.z;

        if (position.x < minX) minX = position.x;
        if (position.y < minY) minY = position.y;
        if (position.z < minZ) minZ = position.z;

        col.size = new Vector3(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
        col.center = new Vector3((maxX + minX)/2, (maxY + minY)/2, (maxZ + minZ)/2);
    }

    void updatesize()
    {
        BoxCollider col = gameObject.GetComponent<BoxCollider>();

        col.size = new Vector3(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
        col.center = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);
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

        _chunk.AddComponent<MeshFilter>();
        _chunk.AddComponent<MeshRenderer>().material = new Material(GameManager.Instance.MaterialManager.MainMaterial);
        _chunk.AddComponent<MeshCollider>();

        Chunks.Add(_position, _chunk);
        BlocksinChunk.Add(_position, new Dictionary<Vector3Int, BlockInChunkData>());
    }

    public void BuildMesh(Vector3Int worldPos)
    {
        Vector3Int chunkCoord = GetChunkCoord(worldPos);

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

        updatesize();
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
        Vector3Int localPos = block._position;
        Vector3Int worldPos = chunkCoord * ChunkSize + localPos;

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
}
