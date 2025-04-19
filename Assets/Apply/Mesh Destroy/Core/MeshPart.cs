using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDestroy
{
    public class MeshPart
    {
        public List<Vector3> Vertices { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<int> Triangles { get; private set; }
        public List<Vector2> UVs { get; private set; }

        public Bounds bounds;
        public bool canBuild;

        public MeshPart()
        {
            Init();
        }

        public MeshPart(Mesh mesh)
        {
            Init();
            mesh.GetVertices(Vertices);
            mesh.GetNormals(Normals);
            mesh.GetTriangles(Triangles, 0);
            mesh.GetUVs(0, UVs);
            bounds = mesh.bounds;
        }

        public void AddTriangle(
            Vector3 vert1, Vector3 vert2, Vector3 vert3,
            Vector3 normal1, Vector3 normal2, Vector3 normal3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            Triangles.Add(Vertices.Count);
            Vertices.Add(vert1);
            Triangles.Add(Vertices.Count);
            Vertices.Add(vert2);
            Triangles.Add(Vertices.Count);
            Vertices.Add(vert3);
            Normals.Add(normal1);
            Normals.Add(normal2);
            Normals.Add(normal3);
            UVs.Add(uv1);
            UVs.Add(uv2);
            UVs.Add(uv3);

            bounds.min = Vector3.Min(bounds.min, vert1);
            bounds.min = Vector3.Min(bounds.min, vert2);
            bounds.min = Vector3.Min(bounds.min, vert3);
            bounds.max = Vector3.Min(bounds.max, vert1);
            bounds.max = Vector3.Min(bounds.max, vert2);
            bounds.max = Vector3.Min(bounds.max, vert3);
        }

        public Mesh CreatePartMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "MeshPart";
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetUVs(0, UVs);
            mesh.SetTriangles(Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            bounds = mesh.bounds;

            return mesh;
        }

        private void Init() 
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Triangles = new List<int>();
            UVs = new List<Vector2>();
            bounds = new Bounds();
        }
    }
}