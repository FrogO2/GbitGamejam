using System.Collections.Generic;
using UnityEngine;

// TODO bridge mapping
// TODO First property
// TODO renderer copy

namespace RayFire
{
    [AddComponentMenu (RFLog.shl_path)]
    public class RayfireShell : MonoBehaviour
    {
        public bool     awakeCreate;
        public bool     bridge    = true;
        public float    thickness = 0.1f;
        public Material material;
        public bool     awakeBake;
        public bool     subMerge;


        public bool disableByShatter;
        
        // support for mesh children
        // int         shatterFragmentType;

        [SerializeField] public bool       created;
        [SerializeField] public bool       backed;

        // Components
        public GameObject   shellObj;
        public MeshFilter   shellMeshFilter;
        public MeshRenderer shellMeshRenderer;
        public MeshFilter   meshFilter;
        public MeshRenderer meshRenderer;
 
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake 
        void Awake()
        {
            if (awakeCreate == true)
            {
                // Destroy existing
                if (shellObj != null)
                    Destroy (shellObj);
                
                // Create new
                if (created == false)
                    ShellObject();

                // Bake on awake
                if (awakeBake == true)
                    BakeShell();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Toggle preview
        public void PreviewOff()
        {
            // Destroy existing shell
            if (shellObj != null)
            {
                if (Application.isPlaying == true)
                    Destroy (shellObj);
                else
                    DestroyImmediate (shellObj);
            }
            
            created           = false;
            shellObj          = null; 
            shellMeshFilter   = null;
            shellMeshRenderer = null;
        }
        
        // Toggle preview
        public void PreviewOn()
        {
            // Create shell object
            ShellObject();
            
            // Set state
            created = true;
        }
        
        // Create shell object
        void ShellObject()
        {
            // Set components
            meshFilter   = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            
            // No mesh and renderer
            if (meshFilter == null)
                return;
            
            // No mesh and renderer
            if (meshFilter.sharedMesh == null)
                return;

            // Set mesh
            Mesh mesh = RFShell.GetShellMesh(meshFilter.sharedMesh, thickness, bridge);;

            // No shell mesh
            if (mesh == null)
                return;
            
            // TODO Bake on wake without creating shell object
            
            // Create shell object
            shellObj                      = new GameObject(name + "_shell");
            shellObj.transform.parent     = transform;
            shellObj.transform.position   = transform.position;
            shellObj.transform.rotation   = transform.rotation;
            shellObj.transform.localScale = Vector3.one;

            // Add meshfilter and mesh
            shellMeshFilter            = shellObj.AddComponent<MeshFilter>();
            shellMeshFilter.sharedMesh = mesh;
            
            // No renderer
            if (meshRenderer == null)
                return;
            
            // Set renderer
            shellMeshRenderer = shellObj.AddComponent<MeshRenderer>();
            if (material == null)
                shellMeshRenderer.sharedMaterials = meshRenderer.sharedMaterials;
            else
                shellMeshRenderer.sharedMaterial = material;
        }
        
        // Change thickness already created shell
        public void EditShell()
        {
            // Checks
            if (shellMeshFilter == null)
                return;

            // Get shell mesh
            shellMeshFilter.sharedMesh = RFShell.GetShellMesh(meshFilter.sharedMesh, thickness, bridge);
        }

        // Bake Shell
        public void BakeShell()
        {
            // Null checks
            if (meshFilter == null)
                return;
            if (shellMeshFilter == null)
                return;
            if (meshRenderer == null)
                return;
            if (shellMeshRenderer == null)
                return;
            
            // Combine meshes
            Mesh combinedMesh = RFShell.CombineMesh (meshFilter.sharedMesh, shellMeshFilter.sharedMesh, !subMerge);
            meshFilter.sharedMesh = combinedMesh;
            
            // Add materials
            if (subMerge == true)
            {
                List<Material> materials = new List<Material> (meshRenderer.sharedMaterials);
                materials.AddRange (shellMeshRenderer.sharedMaterials);
                meshRenderer.sharedMaterials = materials.ToArray();
            }
            
            // Destroy shell objects
            PreviewOff();

            // Baked state
            backed            = true;
            shellMeshFilter   = null;
            shellMeshRenderer = null;
        }
    }
}