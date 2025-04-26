using System;
using UnityEngine;

[Obsolete("There will be bugs when the object is a child of certain other objects. If you face such bugs, copy the material and change tiling instead of using this script.")]
[ExecuteInEditMode]
public class ReCalcPlaneTexture : MonoBehaviour
{
    private Vector3 _currentScale;

    private void Start()
    {
        Calculate();
    }

    private void Update()
    {
        Calculate();
    }

    public void Calculate()
    {
        if (_currentScale == transform.localScale) return;
        if (CheckForDefaultSize()) return;

        _currentScale = transform.localScale;
        var mesh = GetMesh();
        mesh.uv = SetupUvMap(mesh.vertices);
        mesh.name = "Plane Instance";

        var mat = GetComponent<Renderer>().sharedMaterial;
        if (mat != null && mat.mainTexture != null && mat.mainTexture.wrapMode != TextureWrapMode.Repeat)
        {
            mat.mainTexture.wrapMode = TextureWrapMode.Repeat;
        }
    }

    private Mesh GetMesh()
    {
        Mesh mesh;

#if UNITY_EDITOR
        var meshFilter = GetComponent<MeshFilter>();
        var meshCopy = Instantiate(meshFilter.sharedMesh);
        mesh = meshFilter.mesh = meshCopy;
#else
        mesh = GetComponent<MeshFilter>().mesh;
#endif

        return mesh;
    }

    private Vector2[] SetupUvMap(Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];
        float scaleX = _currentScale.x;
        float scaleZ = _currentScale.z;

        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x * scaleX, vertices[i].z * scaleZ);
        }

        return uvs;
    }

    private bool CheckForDefaultSize()
    {
        if (_currentScale != Vector3.one) return false;

        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        DestroyImmediate(GetComponent<MeshFilter>());
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().sharedMesh = plane.GetComponent<MeshFilter>().sharedMesh;

        DestroyImmediate(plane);

        return true;
    }
}