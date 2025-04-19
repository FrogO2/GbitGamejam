using System;
using UnityEngine;

namespace RayFire
{
    /// <summary>
    /// Rayfire Shatter shell class.
    /// </summary>
    [Serializable]
    public class RFShell
    {
        public bool  enable;
        public bool  first;
        public bool  bridge;
        public bool  submesh;
        public float thickness;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        public RFShell()
        {
            enable = false;
            first = false;
            bridge    = true;
            submesh   = true;
            thickness = 0.05f;
        }
        
        public RFShell(RFShell src)
        {
            enable    = src.enable;
            first     = src.first;
            bridge    = src.bridge;
            submesh   = src.submesh;
            thickness = src.thickness;
        }
        
        public static void Copy(RFShell trg, RFShell src)
        {
            trg.enable    = src.enable;
            trg.first     = src.first;
            trg.bridge    = src.bridge;
            trg.submesh   = src.submesh;
            trg.thickness = src.thickness;
        }

        /// /////////////////////////////////////////////////////////
        /// Shell
        /// /////////////////////////////////////////////////////////
 
        // Editor support for shell methods for now
        #if (UNITY_EDITOR_WIN)  
        
        // Get shell mesh
        public static Mesh GetShellMesh(Mesh unityMesh, float thickVal, bool bridgeState)
        {
            // Create RF mesh based on original mesh
            Utils.Mesh rfMesh = new Utils.Mesh(unityMesh);
            
            // Get shell data
            rfMesh.GetShell(out int[] tris, out Vector3[] dirs, out int[] openEdges);

            // Set shell vertices position
            Vector3[] shiftVerts = unityMesh.vertices;
            for (int i = 0; i < shiftVerts.Length; i++)
                shiftVerts[i] += dirs[i] * thickVal;
            
            // Create shell mesh
            Mesh shellMesh = GetInnerMesh (unityMesh, tris, shiftVerts);
            shellMesh.name = unityMesh.name;
            
            // Create bridge mesh
            Mesh bridgeMesh = bridgeState == true && openEdges.Length > 0 ? GetBridgeMesh (unityMesh, openEdges, shiftVerts) : null;
            
            // Combine inner shell and bridge with forced one syb mesh
            Mesh modifiedMesh = CombineMesh(shellMesh, bridgeMesh, null, true);

            return modifiedMesh;
        }
        
        // Add shell to unity mesh based on info from RFMesh
        public static Mesh AddShell(Utils.Mesh utilsMesh, Mesh unityMesh, bool bridge, bool subMerge, float thickness)
        {
            // No thickness
            if (thickness <= 0)
                return unityMesh;
            
            // Get shell data
            utilsMesh.GetShell(out int[] tris, out Vector3[] dirs, out int[] openEdges);

            // Mesh has no open edges to add shell
            if (openEdges.Length == 0)
            {
                RayfireMan.Log (RFLog.shl_dbgn + RFLog.shl_noEdg);
                return unityMesh;
            }
            
            // Set shell vertices position
            Vector3[] shiftVerts = unityMesh.vertices;
            for (int i = 0; i < shiftVerts.Length; i++)
                shiftVerts[i] += dirs[i] * thickness;
            
            // Create shell mesh
            Mesh shellMesh = GetInnerMesh (unityMesh, tris, shiftVerts);
            
            // Create bridge mesh
            Mesh bridgeMesh = bridge == true && openEdges.Length > 0 ? GetBridgeMesh (unityMesh, openEdges, shiftVerts) : null;
            
            // Combine all meshes and return
            return CombineMesh(unityMesh, shellMesh, bridgeMesh, subMerge); 
        }
        
        #else
        
        // Dummy method for not supported platforms
        public static Mesh GetShellMesh(Mesh unityMesh, float thickVal, bool bridgeState) 
        {
            return null;
        }
        
        #endif
        
        // Create bridge for shell mesh
        static Mesh GetInnerMesh(Mesh unityMesh, int[] tris, Vector3[] shiftVerts)
        {
            // Create shell mesh
            Mesh shellMesh = new Mesh();
            shellMesh.vertices  = shiftVerts;
            shellMesh.triangles = tris;

            /*
            // Set submesh count
            shellMesh.subMeshCount = unityMesh.subMeshCount;
            for (int i = 0; i < unityMesh.subMeshCount; i++)
                shellMesh.SetSubMesh(i, unityMesh.GetSubMesh(i));         
            */
            
            // Set normals
            Vector3[] invNorms = unityMesh.normals;
            for(int i = 0; i < invNorms.Length; i++)
                invNorms[i] *= -1.0f;
            shellMesh.normals = invNorms;

            return shellMesh;
        }

        // Create bridge for shell mesh
        static Mesh GetBridgeMesh(Mesh unityMesh, int[] openEdges, Vector3[] shiftVerts)
        {
            Mesh      bridgeMesh   = new Mesh();
            int       numOpenEdges = openEdges.Length / 2;
            int[]     bridgeTris   = new int[numOpenEdges * 6];
            Vector3[] bridgeVerts  = new Vector3[numOpenEdges * 4];
            Vector3[] origVerts    = unityMesh.vertices;

            for (int i = 0; i < numOpenEdges; i++)
            {
                bridgeVerts[i * 4 + 0] = origVerts[openEdges[i * 2 + 0]];
                bridgeVerts[i * 4 + 1] = origVerts[openEdges[i * 2 + 1]];
                bridgeVerts[i * 4 + 2] = shiftVerts[openEdges[i * 2 + 0]];
                bridgeVerts[i * 4 + 3] = shiftVerts[openEdges[i * 2 + 1]];

                bridgeTris[i * 6 + 0] = i * 4 + 2;
                bridgeTris[i * 6 + 1] = i * 4 + 1;
                bridgeTris[i * 6 + 2] = i * 4 + 0;

                bridgeTris[i * 6 + 3] = i * 4 + 1;
                bridgeTris[i * 6 + 4] = i * 4 + 2;
                bridgeTris[i * 6 + 5] = i * 4 + 3;
            }
            bridgeMesh.vertices  = bridgeVerts;
            bridgeMesh.triangles = bridgeTris;
            return bridgeMesh;
        }
        
        // Combine three meshes
        static Mesh CombineMesh(Mesh outerMesh, Mesh innerMesh, Mesh bridgeMesh, bool mergeSubMeshes)
        {
            if (innerMesh == null)
            {
                outerMesh.name += "_shell";
                return outerMesh;
            }
            
            int num = 2;
            if (bridgeMesh != null)
                num = 3;
            
            // Combine
            CombineInstance[] combine = new CombineInstance[num];
            combine[0].mesh = outerMesh;
            combine[1].mesh = innerMesh;
            if (bridgeMesh != null)
                combine[2].mesh = bridgeMesh;

            Mesh mesh = new Mesh();
            mesh.name = outerMesh.name + "_shell";
            mesh.CombineMeshes (combine, mergeSubMeshes, false);
            mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
        
        // Combine two meshes
        public static Mesh CombineMesh(Mesh originalMesh, Mesh modifiedMesh, bool mergeSubMeshes)
        {
            // Combine
            CombineInstance[] combine = new CombineInstance[2];
            combine[0].mesh = originalMesh;
            combine[1].mesh = modifiedMesh;

            Mesh mesh = new Mesh();
            mesh.name = originalMesh.name + "_shell";
            mesh.CombineMeshes (combine, mergeSubMeshes, false);
            mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}