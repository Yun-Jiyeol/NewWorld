using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BlockMeshGenerator : MonoBehaviour
{
    Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Build(BlockFace faces)
    {
        mesh.Clear();

        List<Vector3> verts = new();
        List<int> tris = new();

        void AddFace(Vector3[] v)
        {
            int start = verts.Count;
            verts.AddRange(v);

            tris.Add(start + 0);
            tris.Add(start + 2);
            tris.Add(start + 1);
            tris.Add(start + 0);
            tris.Add(start + 3);
            tris.Add(start + 2); 
        }

        if (faces.HasFlag(BlockFace.Up))
            AddFace(new[] {
                new Vector3(-0.5f,0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f),
                new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f)
            });

        if (faces.HasFlag(BlockFace.Down))
            AddFace(new[] {
                new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(-0.5f,-0.5f,0.5f),
                new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,-0.5f)
            });

        if (faces.HasFlag(BlockFace.Left))
            AddFace(new[] {
                new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(-0.5f,0.5f,0.5f), new Vector3(-0.5f,-0.5f,0.5f)
            });

        if (faces.HasFlag(BlockFace.Right))
            AddFace(new[] {
                new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,0.5f),
                new Vector3(0.5f,0.5f,0.5f), new Vector3(0.5f,0.5f,-0.5f)
            });

        if (faces.HasFlag(BlockFace.Front))
            AddFace(new[] {
                new Vector3(-0.5f,-0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f),
                new Vector3(0.5f,0.5f,0.5f), new Vector3(0.5f,-0.5f,0.5f)
            });

        if (faces.HasFlag(BlockFace.Back))
            AddFace(new[] {
                new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f)
            });

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
    }
}
