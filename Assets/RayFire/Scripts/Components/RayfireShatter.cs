using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_XBOXONE)
using RayFire.DotNet;
#endif

namespace RayFire
{
    [AddComponentMenu (RFLog.sht_path)]
    [HelpURL (RFLog.sht_link)]
    public class RayfireShatter : MonoBehaviour
    {
        public enum RFEngineType
        {
            V1     = 0,
            V2Beta = 1
        }
        
        public enum FragLastMode
        {
            New    = 0,
            ToLast = 1
        }

        // UI
        public RFEngineType      engine    = RFEngineType.V1;
        public FragType          type      = FragType.Voronoi;
        public FragmentMode      mode      = FragmentMode.Editor;
        public RFVoronoi         voronoi   = new RFVoronoi();
        public RFSplinters       splinters = new RFSplinters();
        public RFSplinters       slabs     = new RFSplinters();
        public RFRadial          radial    = new RFRadial();
        public RFHexagon         hexagon   = new RFHexagon();
        public RFCustom          custom    = new RFCustom();
        public RFMirrored        mirrored  = new RFMirrored();
        public RFSlice           slice     = new RFSlice();
        public RFBricks          bricks    = new RFBricks();
        public RFVoxels          voxels    = new RFVoxels();
        public RFTets            tets      = new RFTets();
        public RFSurface         material  = new RFSurface();
        public RFShatterCluster  clusters  = new RFShatterCluster();
        public RFShatterAdvanced advanced  = new RFShatterAdvanced();
        public RFShell           shell     = new RFShell();
        
        // Center
        public bool       showCenter;
        public Vector3    centerPosition;
        public Quaternion centerDirection;

        // Components
        public Transform           transForm;
        public MeshFilter          meshFilter;
        public MeshRenderer        meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRend;
        public List<MeshFilter>    meshFilters;

        // Vars
        [NonSerialized] public Mesh[]         meshes;
        [NonSerialized] public Vector3[]      pivots;
        [NonSerialized] public RFDictionary[] rfOrigSubMeshIds;
        public                 Material[]     materials;

        // Hidden
        public int     shatterMode = 1;
        public bool    colorPreview;
        public bool    scalePreview = true;
        public float   previewScale;
        public float   size;
        public float   rescaleFix = 1f;
        public Vector3 originalScale;
        public Bounds  bound;
        public bool    resetState;

        // Interactive
        public bool         interactive;
        public GameObject   intGo;
        public MeshFilter   intMf;
        public MeshRenderer intMr;

        // Static
        public string fragAddStr = "_sh_";
        public string shatterStr = "RayFire Shatter: ";
        
        // RFEngine props TODO move to advanced
        public List<Tuple<Mesh, Matrix4x4[]>> skinnedMeshesOrigScale = null;
        public RFEngine                       engineData;
        public List<RFShatterBatch>             batches = new List<RFShatterBatch>();
        
        public Transform  CenterBias { get { return advanced.centerSet == true && advanced.centerBias != null ? advanced.centerBias.transform : transform; }}
        public Vector3    CenterPos  { get { return advanced.centerSet == true && advanced.centerBias != null ? advanced.centerBias.transform.position : transform.position; }}
        public Quaternion CenterDir  { get { return advanced.centerSet == true && advanced.centerBias != null ? advanced.centerBias.transform.rotation : transform.rotation; }}

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Reset
        private void Reset()
        {
            InteractiveStop();
            ResetCenter();
        }

        // Set default vars before fragment
        void SetVariables()
        {
            size             = 0f;
            rescaleFix       = 1f;
            originalScale    = transForm.localScale;
            rfOrigSubMeshIds = null;
        }

        /// /////////////////////////////////////////////////////////
        /// Checks
        /// /////////////////////////////////////////////////////////

        // Basic proceed check
        bool MainCheck()
        {
            // Check if prefab
            if (gameObject.scene.rootCount == 0)
            {
                RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_pref, gameObject);
                return false;
            }

            // Single mesh mode
            if (advanced.children == false)
                if (SingleMeshCheck() == false)
                    return false;

            // Multiple mesh mode
            if (advanced.children == true)
            {
                // Has no children meshes
                if (meshFilters.Count == 1)
                    if (SingleMeshCheck() == false)
                        return false;

                // Remove no meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh == null)
                        {
                            RayfireMan.Log (RFLog.sht_dbgn + meshFilters[i].name + RFLog.sht_noMesh, meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }

                // Remove no readable meshes
                if (meshFilters.Count > 0)
                    for (int i = meshFilters.Count - 1; i >= 0; i--)
                        if (meshFilters[i].sharedMesh.isReadable == false)
                        {
                            RayfireMan.Log (RFLog.sht_dbgn + meshFilters[i].name + RFLog.sht_noRead, meshFilters[i].gameObject);
                            meshFilters.RemoveAt (i);
                        }

                // No meshes left
                if (meshFilters.Count == 0)
                    return false;
            }

            return true;
        }

        // Single mesh mode checks
        bool SingleMeshCheck()
        {
            // No mesh storage components
            if (meshFilter == null && skinnedMeshRend == null)
            {
                RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_noFlt, gameObject);
                return false;
            }

            // Has mesh filter
            if (meshFilter != null)
            {
                // No shared mesh
                if (meshFilter.sharedMesh == null)
                {
                    RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_noMesh, gameObject);
                    return false;
                }

                // Not readable mesh
                if (meshFilter.sharedMesh.isReadable == false)
                {
                    RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_noRead, gameObject);
                    return false;
                }
            }

            // Has skinned mesh
            if (skinnedMeshRend != null && skinnedMeshRend.sharedMesh == null)
            {
                RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_noFlt, gameObject);
                return false;
            }

            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Cache variables
        bool DefineComponents()
        {
            // Mesh storage 
            transForm       = GetComponent<Transform>();
            meshFilter      = GetComponent<MeshFilter>();
            meshRenderer    = GetComponent<MeshRenderer>();
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();

            // Multymesh fragmentation
            meshFilters = new List<MeshFilter>();
            if (advanced.children == true)
                meshFilters = GetComponentsInChildren<MeshFilter>().ToList();

            // Basic proceed check
            if (MainCheck() == false)
                return false;

            // Mesh renderer
            if (skinnedMeshRend == null)
            {
                if (meshRenderer == null)
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                bound = meshRenderer.bounds;
            }

            // Skinned mesh
            if (skinnedMeshRend != null)
                bound = skinnedMeshRend.bounds;

            return true;
        }

        // Get bounds
        public Bounds GetBound()
        {
            // Mesh renderer
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                    return meshRenderer.bounds;
            }
            else
                return meshRenderer.bounds;

            // Skinned mesh
            if (skinnedMeshRend == null)
            {
                skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRend != null)
                    return skinnedMeshRend.bounds;
            }

            return new Bounds();
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Fragment this object by shatter properties  List<GameObject>
        public void Fragment(FragLastMode fragmentMode = FragLastMode.New)
        {
            // TODO TEMP 
            if (engine == RFEngineType.V2Beta && type == FragType.Tets)
            {
                RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_noTet, gameObject);
                return;
            }

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            
            // Engine
            if (engine == RFEngineType.V1)
                FragmentV1 (fragmentMode);
            else if (engine == RFEngineType.V2Beta)
                FragmentV2();
            
            stopWatch.Stop();
            
            RayfireMan.Log(RFLog.sht_time + stopWatch.Elapsed.TotalMilliseconds.ToString("F2") + " ms");
        }

        // New Unity fragmentation engine
        void FragmentV2()
        {
            RFEngine.FragmentShatter (this);
        }

        // Original 3dsMax engine fragmentation
        void FragmentV1(FragLastMode fragmentMode)
        {
            // Prepare to cache fragments
            if (PreCache() == false)
                return;

            // Cache
            RFFragment.CacheMeshes (ref meshes, ref pivots, ref rfOrigSubMeshIds, this);

            // Stop
            if (meshes == null)
                return;

            // Create fragments
            CreateFragments();

            // Post create fragments operations
            PostFragments();
        } 
        
        // Prepare and cache fragments
        public bool PreCache()
        {
            // Cache variables
            if (DefineComponents() == false)
                return false;

            // Cache default vars
            SetVariables();

            // Check if object is too small
            ScaleCheck();

            return true;
        }

        // Create fragments by mesh and pivots array
        List<GameObject> CreateFragments(Transform root = null)
        {
            // No mesh were cached
            if (meshes == null)
                return null;

            // Clear array for new fragments
            GameObject[] fragArray = new GameObject[meshes.Length];

            // Create root object
            if (root == null)
            {
                GameObject rootGo = new GameObject (gameObject.name + "_root");
                rootGo.transform.position = transForm.position;
                rootGo.transform.rotation = transForm.rotation;
                rootGo.tag                = gameObject.tag;
                rootGo.layer              = gameObject.layer;
                root                      = rootGo.transform;
            }
            
            // Create shatter batch
            RFShatterBatch batch = new RFShatterBatch(transform, root);
            batch.SaveData (this);

            // Create instance for fragments
            GameObject fragInstance;
            if (advanced.copy == true)
            {
                fragInstance                      = Instantiate (gameObject);
                fragInstance.transform.rotation   = Quaternion.identity;
                fragInstance.transform.localScale = Vector3.one;

                // Destroy shatter
                DestroyImmediate (fragInstance.GetComponent<RayfireShatter>());
            }
            else
            {
                fragInstance = new GameObject();
                fragInstance.AddComponent<MeshFilter>();
                fragInstance.AddComponent<MeshRenderer>();
            }

            // Get original mats. in case of combined meshes it is already defined in CombineShatter()
            if (advanced.children == false)
                materials = skinnedMeshRend != null
                    ? skinnedMeshRend.sharedMaterials
                    : meshRenderer.sharedMaterials;

            // Vars 
            string baseName = gameObject.name + fragAddStr;

            // Create fragment objects
            MeshFilter   mf;
            GameObject   go;
            MeshCollider mc;
            MeshRenderer rn;
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Rescale mesh
                if (rescaleFix != 1f)
                    RFFragment.RescaleMesh (meshes[i], rescaleFix);

                // Instantiate. IMPORTANT do not parent when Instantiate
                go                      = Instantiate (fragInstance);
                go.transform.localScale = Vector3.one;

                // Set multymaterial
                rn = go.GetComponent<MeshRenderer>();
                RFSurface.SetMaterial (rfOrigSubMeshIds, materials, material, rn, i, meshes.Length);

                // Set fragment object name and tm
                go.name               = baseName + (i + 1);
                go.transform.position = root.transform.position + (pivots[i] / rescaleFix);
                go.transform.parent   = root.transform;
                go.tag                = gameObject.tag;
                go.layer              = gameObject.layer;

                // Set fragment mesh
                mf                 = go.GetComponent<MeshFilter>();
                mf.sharedMesh      = meshes[i];
                mf.sharedMesh.name = go.name;

                // Set mesh collider
                mc = go.GetComponent<MeshCollider>();
                if (mc != null)
                    mc.sharedMesh = meshes[i];

                // Add in array
                fragArray[i] = go;
                
                // Collect batch data
                batch.fragments.Add (go.transform);
            }

            // Root back to original parent
            root.transform.parent = transForm.parent;

            // Reset scale for mesh fragments. IMPORTANT: skinned mesh fragments root should not be rescaled 
            if (skinnedMeshRend == null)
                root.transform.localScale = Vector3.one;

            // Destroy instance
            DestroyImmediate (fragInstance);

            // Empty lists
            meshes           = null;
            pivots           = null;
            rfOrigSubMeshIds = null;
            
            // Collect batch
            batches.Add (batch);

            return fragArray.ToList();
        }

        // Post create fragments operations
        void PostFragments()
        {
            // Limitation fragment
            RFShatterAdvanced.Limitations (this);

            // Reset original object back if it was scaled
            transForm.localScale = originalScale;
        }

        // Fragment by limitations
        public void LimitationFragment(RFShatterBatch batch, int ind)
        {
            RayfireShatter shat = batch.fragments[ind].gameObject.AddComponent<RayfireShatter>();
            shat.voronoi.amount = 10;
            shat.engine         = engine;

            shat.Fragment();

            if (shat.batches[0].fragments.Count > 0)
            {
                // Reparent new frags
                foreach (var frag in batch.fragments)
                    frag.transform.parent = batch.fragments[ind].parent;
                
                // Add to source batch and remove original
                batch.fragments.AddRange (shat.batches[0].fragments);
                batch.fragments.RemoveAt (ind);
                
                // Destroy original and new frags parent
                DestroyImmediate (shat.batches[0].fragRoot.gameObject);
                DestroyImmediate (shat.gameObject);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Center
        /// /////////////////////////////////////////////////////////
        
        // Reset center helper
        public void ResetCenter()
        {
            centerPosition  = Vector3.zero;
            centerDirection = Quaternion.identity;

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
                centerPosition = transform.InverseTransformPoint (rend.bounds.center);
        }

        /// /////////////////////////////////////////////////////////
        /// Scale
        /// /////////////////////////////////////////////////////////

        // Check if object is too small
        void ScaleCheck()
        {
            // Geе size from renderers
            if (meshRenderer != null)
                size = meshRenderer.bounds.size.magnitude;
            if (skinnedMeshRend != null)
                size = skinnedMeshRend.bounds.size.magnitude;

            // Get rescaleFix if too small
            if (size != 0f && size < RFShatterAdvanced.minSize)
            {
                // Get rescaleFix factor
                rescaleFix = 1f / size;

                // Scale small object up to shatter
                Vector3 newScale = transForm.localScale * rescaleFix;
                transForm.localScale = newScale;

                // Warning
                RayfireMan.Log (RFLog.sht_dbgn + name + RFLog.sht_small, gameObject);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Copy
        /// /////////////////////////////////////////////////////////

        // Copy shatter component
        public static void CopyRootMeshShatter(RayfireRigid source, List<RayfireRigid> targets)
        {
            // No shatter
            if (source.mshDemol.sht == null)
                return;

            // Copy shatter
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].mshDemol.sht = targets[i].gameObject.AddComponent<RayfireShatter>();
                targets[i].mshDemol.sht.CopyFrom (source.mshDemol.sht);
            }
        }

        // Copy from
        void CopyFrom(RayfireShatter shatter)
        {
            type = shatter.type;

            voronoi   = new RFVoronoi (shatter.voronoi);
            splinters = new RFSplinters (shatter.splinters);
            slabs     = new RFSplinters (shatter.slabs);
            radial    = new RFRadial (shatter.radial);
            custom    = new RFCustom (shatter.custom);
            slice     = new RFSlice (shatter.slice);
            tets      = new RFTets (shatter.tets);

            mode = shatter.mode;
            RFSurface.Copy (material, shatter.material);
            RFShatterCluster.Copy (clusters, shatter.clusters);
            RFShatterAdvanced.Copy (advanced, shatter.advanced);
        }

        /// /////////////////////////////////////////////////////////
        /// Interactive
        /// /////////////////////////////////////////////////////////

        // Create Interactive object
        public void InteractiveCreate()
        {
            if (intGo == null)
            {
                intGo = new GameObject (name + "_Interactive");
                intMf = intGo.AddComponent<MeshFilter>();
            }
            
            if (intMf == null)
            {
                intMf = intGo.GetComponent<MeshFilter>();
                if (intMf == null)
                    intMf = intGo.AddComponent<MeshFilter>();
            }
            
            // Copy tm
            intGo.transform.position = transForm.position;
                
            intGo.tag   = gameObject.tag;
            intGo.layer = gameObject.layer;
            
            if (meshRenderer != null)
            {
                if (intMr == null)
                {
                    intMr                 = intGo.AddComponent<MeshRenderer>();
                    intMr.sharedMaterials = meshRenderer.sharedMaterials;
                }
            }
            else if (skinnedMeshRend != null)
            {
                // TODO skinned mesh renderer support
            }
        }

        // Fragment all meshes into own mesh
        public void InteractiveStart()
        {
            if (engine == RFEngineType.V1)
                RFFragment.InteractiveStart (this);
            else if (engine == RFEngineType.V2Beta)
                RFEngine.InteractiveStart (this);
        }

        // Property changed
        public void InteractiveChange()
        {
            if (engine == RFEngineType.V1)
                RFFragment.InteractiveChange (this);
            else if (engine == RFEngineType.V2Beta)
                RFEngine.InteractiveChange (this);
        }

        // Create interactively cached fragments
        public void InteractiveFragment()
        {
            // Create new fragments
            CreateFragments();

            // Post create fragments operations
            PostFragments();

            // Reset original mesh
            InteractiveStop();
        }

        // Revert original mesh
        public void InteractiveStop()
        {
            // Enable own Renderer
            OriginalRenderer(true);
            
            // Destroy interactive object
            if (intGo != null)
                DestroyImmediate (intGo);
            
            // Reset
            intGo       = null;
            intMf       = null;
            intMr       = null;
            interactive = false;
        }

        // Set original renderer state
        public void OriginalRenderer(bool state)
        {
            if (meshRenderer != null)
                meshRenderer.enabled = state;
            if (skinnedMeshRend != null)
                skinnedMeshRend.enabled = state;
        }
        
        public void InteractiveReset()
        {
            if (interactive == true)
            {
                InteractiveStop();
            }
        }

        // Final preview scale
        public float PreviewScale()
        {
            if (scalePreview == false)
                return 1f;
            return Mathf.Lerp (1f, 0.3f, previewScale);
        }
    }
}