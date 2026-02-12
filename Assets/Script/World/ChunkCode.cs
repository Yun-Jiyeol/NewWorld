using System.Collections.Generic;
using UnityEngine;

public class BlockInChunkData
{
    public Vector3Int _position;
    public Color _color;
}

public class ChunkCode : MonoBehaviour
{
    public Dictionary<Vector3Int, GameObject> Chunks; //어느 좌표에 있는 청크가 무슨 게임 오브젝트인지
    public Dictionary<Vector3Int, Dictionary<Vector3Int, BlockInChunkData>> BlocksinChunk; //어느 좌표에 있는 청크인지 + 그 안에 있는 블럭들
    int ChunkSize = 8;

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
        for(int _x =0; _x < x; _x++)
        {
            for(int _y = 0; _y < y; _y++)
            {
                for (int _z = 0; _z < z; _z++)
                {
                    if(x == 1) MakeBlockData(new Vector3(_x, _y, _z), new Color(1, 1, 0, 1 / 2f), false);
                    else MakeBlockData(new Vector3(_x, _y, _z), Color.white, false);
                }
            }
        }

        foreach(Vector3Int _pos in Chunks.Keys)
        {
            BuildMesh(_pos * ChunkSize);
        }
    }


    public void MakeBlockData(Vector3 _position, Color color, bool updateChunk = true)
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

        if (BlocksinChunk[ChunkPos].Count > 0)
        {
            if (BlocksinChunk[ChunkPos].ContainsKey(LocalPos))
            {
                Debug.LogWarning("이미 있는 좌표");
                return;
            }
        }

        Debug.Log(color);
        BlocksinChunk[ChunkPos].Add(LocalPos, new BlockInChunkData { _position = LocalPos, _color = color }); //데이터 저장

        if (updateChunk) BuildMesh(gridPos);
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

        _chunk.AddComponent<MeshFilter>();
        _chunk.AddComponent<MeshRenderer>().material = new Material(GameManager.Instance.MaterialManager.MainMaterial);
        _chunk.AddComponent<MeshCollider>();

        Chunks.Add(_position, _chunk);
        BlocksinChunk.Add(_position, new Dictionary<Vector3Int, BlockInChunkData>());
    }

    public void BuildMesh(Vector3Int worldPos)
    {
        Vector3Int chunkCoord = GetChunkCoord(worldPos);

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Color> colors = new();

        int vertexIndex = 0;

        foreach (var kvp in BlocksinChunk[chunkCoord])
        {
            AddCube(chunkCoord, kvp.Value, ref vertices, ref triangles, ref colors, ref vertexIndex);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter mf = Chunks[chunkCoord].GetComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshCollider mc = Chunks[chunkCoord].GetComponent<MeshCollider>();
        if (mc == null) mc = Chunks[chunkCoord].AddComponent<MeshCollider>();

        mc.sharedMesh = null;   // 중요
        mc.sharedMesh = mesh;
    }

    void AddCube(Vector3Int chunkCoord,BlockInChunkData block, ref List<Vector3> vertices,ref List<int> triangles, ref List<Color> colors,ref int vertexIndex)
    {
        Vector3Int localPos = block._position;
        Vector3Int worldPos = chunkCoord * ChunkSize + localPos;

        bool currentOpaque = block._color.a >= 1f;

        for (int face = 0; face < 6; face++)
        {
            Vector3Int neighborWorld = worldPos + directions[face];
            Vector3Int neighborChunk = GetChunkCoord(neighborWorld);
            Vector3Int neighborLocal = GetLocalPos(neighborWorld);

            BlockInChunkData neighbor = null;

            if (BlocksinChunk.ContainsKey(neighborChunk) &&
                BlocksinChunk[neighborChunk].ContainsKey(neighborLocal))
            {
                neighbor = BlocksinChunk[neighborChunk][neighborLocal];
            }

            bool removeFace = false;

            if (neighbor != null)
            {
                bool neighborOpaque = neighbor._color.a >= 1f;

                if (currentOpaque && neighborOpaque)
                    removeFace = true;

                if (!currentOpaque && !neighborOpaque)
                    removeFace = true;
            }

            if (!removeFace)
            {
                AddFace(localPos, face, block._color,ref vertices, ref triangles, ref colors, ref vertexIndex);
            }
        }
    }

    void AddFace(Vector3Int pos, int face, Color color, ref List<Vector3> vertices, ref List<int> triangles,ref List<Color> colors,ref int vertexIndex)
    {
        Vector3 p = pos;

        Vector3[,] faceVerts = new Vector3[6, 4]
        {
        { new(-0.5f,-0.5f, 0.5f), new( 0.5f,-0.5f, 0.5f), new( 0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f) },
        { new( 0.5f,-0.5f,-0.5f), new(-0.5f,-0.5f,-0.5f), new(-0.5f, 0.5f,-0.5f), new( 0.5f, 0.5f,-0.5f) },
        { new(-0.5f,-0.5f,-0.5f), new(-0.5f,-0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f,-0.5f) },
        { new( 0.5f,-0.5f, 0.5f), new( 0.5f,-0.5f,-0.5f), new( 0.5f, 0.5f,-0.5f), new( 0.5f, 0.5f, 0.5f) },
        { new(-0.5f, 0.5f, 0.5f), new( 0.5f, 0.5f, 0.5f), new( 0.5f, 0.5f,-0.5f), new(-0.5f, 0.5f,-0.5f) },
        { new(-0.5f,-0.5f,-0.5f), new( 0.5f,-0.5f,-0.5f), new( 0.5f,-0.5f, 0.5f), new(-0.5f,-0.5f, 0.5f) }
        };

        for (int i = 0; i < 4; i++)
            vertices.Add(p + faceVerts[face, i]);

        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 0);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);

        for (int i = 0; i < 4; i++)
            colors.Add(color);

        vertexIndex += 4;
    }
}
