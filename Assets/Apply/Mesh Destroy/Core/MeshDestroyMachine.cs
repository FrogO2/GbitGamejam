using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDestroy
{
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                    
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of {typeof(T)} found. This should never happen.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = $"{typeof(T)} (Singleton)";

                            DontDestroyOnLoad(singleton);

                            Debug.Log($"[Singleton] An instance of {typeof(T)} was created.");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }
    }
    
    public class MeshDestroyMachine : Singleton<MeshDestroyMachine>
    {
        private bool edgeSet = false;
        private Vector3 edgeVertex = Vector3.zero;
        private Vector2 edgeUV = Vector2.zero;
        private Plane edgePlane = new Plane();

        /// <summary>
        /// 爆炸的切割
        /// </summary>
        public List<MeshPart> DestroyMesh(MeshDestroyAble destroyAble)
        {
            Mesh originMesh = destroyAble.OrigionMesh;
            if (originMesh == null) 
                return null; 
            originMesh.RecalculateBounds();
            MeshPart mainPart = new MeshPart(originMesh);
            List<MeshPart> parts = new List<MeshPart>();
            List<MeshPart> subParts = new List<MeshPart>();

            parts.Add(mainPart);

            for (int c = 0; c < destroyAble.cutCascades; c++)
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    Bounds bounds = parts[i].bounds;
                    Plane plane = new Plane(Random.onUnitSphere, bounds.center);
                    subParts.Add(GenerateMesh(parts[i], plane, true));
                    subParts.Add(GenerateMesh(parts[i], plane, false));
                }
                parts = new List<MeshPart>(subParts);
                subParts.Clear();
            }

            for (int i = 0; i < parts.Count; i++)
            {
                CreatePart(destroyAble, parts[i]);
            }

            Destroy(destroyAble.gameObject);

            return parts;
        }

        /// <summary>
        /// 刀剑的切割
        /// </summary>
        public List<MeshPart> SliceMesh(Plane plane, MeshDestroyAble destroyAble)
        {
            Mesh originalMesh = destroyAble.OrigionMesh;
            if (originalMesh == null) { return null; }
            originalMesh.RecalculateBounds();
            MeshPart mainPart = new MeshPart(originalMesh);

            List<MeshPart> parts = new List<MeshPart>();
            parts.Add(GenerateMesh(mainPart, plane, true));
            parts.Add(GenerateMesh(mainPart, plane, false));

            for (int i = 0; i < parts.Count; i++)
            {
                CreatePart(destroyAble, parts[i]);
            }

            Destroy(destroyAble.gameObject);

            return parts;
        }

        /// <summary>
        /// 用平面切网格
        /// </summary>
        private MeshPart GenerateMesh(MeshPart original, Plane plane, bool left)
        {
            MeshPart partMesh = new MeshPart() { };
            Ray ray1 = new Ray();
            Ray ray2 = new Ray();

            List<int> triangles = original.Triangles;
            edgeSet = false;

            for (var j = 0; j < triangles.Count; j = j + 3)
            {
                //判断是否被切割
                bool sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                bool sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                bool sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                int sideCount = (sideA ? 1 : 0) + (sideB ? 1 : 0) + (sideC ? 1 : 0);
                if (sideCount == 0) // 切割平面不朝向的一侧
                {
                    continue;
                }
                if (sideCount == 3) // 切割平面不经过三角形
                {
                    partMesh.AddTriangle(
                        original.Vertices[triangles[j]],
                        original.Vertices[triangles[j + 1]],
                        original.Vertices[triangles[j + 2]],
                        original.Normals[triangles[j]],
                        original.Normals[triangles[j + 1]],
                        original.Normals[triangles[j + 2]],
                        original.UVs[triangles[j]],
                        original.UVs[triangles[j + 1]],
                        original.UVs[triangles[j + 2]]);
                    continue;
                }

                //对于剖面
                //判断三角形中单独的点的是哪一个
                int singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                //求切割点的位置1
                ray1.origin = original.Vertices[triangles[j + singleIndex]];
                Vector3 dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out var enter1);
                float lerp1 = enter1 / dir1.magnitude;

                //求切割点的位置2
                ray2.origin = original.Vertices[triangles[j + singleIndex]];
                Vector3 dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out var enter2);
                float lerp2 = enter2 / dir2.magnitude;

                //将两个切割点加入子网格
                AddEdge(partMesh, left ? plane.normal * -1f : plane.normal,
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                //对于切割边缘
                //一个顶点的一侧
                if (sideCount == 1)
                {
                    partMesh.AddTriangle(
                        original.Vertices[triangles[j + singleIndex]],
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        original.Normals[triangles[j + singleIndex]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        original.UVs[triangles[j + singleIndex]],
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    continue;
                }
                //两个顶点的一侧
                if (sideCount == 2)
                {
                    partMesh.AddTriangle(
                        // vertice
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        // normal
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        // uv
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UVs[triangles[j + ((singleIndex + 1) % 3)]],
                        original.UVs[triangles[j + ((singleIndex + 2) % 3)]]);

                    partMesh.AddTriangle(
                        // vertice
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        ray2.origin + ray2.direction.normalized * enter2,
                        // normal
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        // uv
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UVs[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector2.Lerp(original.UVs[triangles[j + singleIndex]], original.UVs[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    continue;
                }
            }

            return partMesh;
        }

        /// <summary>
        /// 添加边
        /// </summary>
        private void AddEdge(MeshPart meshPart, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
        {
            if (!edgeSet)
            {
                edgeSet = true;
                edgeVertex = vertex1;
                edgeUV = uv1;
            }
            else
            {
                edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

                meshPart.AddTriangle(
                    edgeVertex, 
                    edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2, 
                    edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                    normal, normal, normal,
                    edgeUV, uv1, uv2);
            }
        }

        /// <summary>
        /// 创建分割的Mesh
        /// </summary>
        /// <param name="destroyAble"></param>
        /// <param name="meshPart"></param>
        private void CreatePart(MeshDestroyAble destroyAble, MeshPart meshPart)
        { 
            GameObject go = new GameObject(destroyAble.name);
            go.transform.position = destroyAble.transform.position;
            go.transform.rotation = destroyAble.transform.rotation;
            go.transform.localScale = destroyAble.transform.localScale;

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.materials = destroyAble.meshRenderer.materials;
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.mesh = meshPart.CreatePartMesh();
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.convex = true;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.AddForceAtPosition(meshPart.bounds.center * destroyAble.explodeForce, go.transform.position);
            if (destroyAble.inherit)
            {
                go.AddComponent<MeshDestroyAble>().Inherit(destroyAble);
            }
        }

    }
}