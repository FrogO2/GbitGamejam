using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)
// https://docs.unity3d.com/2021.3/Documentation/Manual/PlatformDependentCompilation.html

namespace RayFire
{
    public class RFEngine
    {
        public List<Mesh>                         unMeshes;
        public List<Utils.Mesh>                   utMeshes;
        public List<Renderer>                     renderers;
        public List<bool>                         skinStates;
        public Dictionary<GameObject, GameObject> oldNewGoMap;
        public GameObject                         rootGO;
        
        public Matrix4x4          normalMat;
        public Utils.SliceData    sliceData;
        public Utils.Mesh[][]     fragMeshes;
        public bool[][]           openEdgeFlags;
        public Vector3[][]        centroids;
        public Utils.MeshMaps[][] fragMaps;
        public int                innerSubId;
        public Matrix4x4          biasTN;
        public Vector3            biasPos;
        public Matrix4x4[]        centroidScaleMat;
        public Matrix4x4[]        flatExtraMat;
        public Vector3            aabbMin;
        public Vector3            aabbMax;
        public Vector3            aabbSize;

        // TODO array of meshfilters

        /// /////////////////////////////////
        /// Constructor
        /// /////////////////////////////////

        RFEngine(int size)
        {
            unMeshes    = new List<Mesh>(size);
            utMeshes    = new List<Utils.Mesh>(size);
            renderers   = new List<Renderer>(size);
            skinStates  = new List<bool> (size);
            oldNewGoMap = new Dictionary<GameObject, GameObject>(size);
        }
        
        // TODO add case with one object and renderer input
        static RFEngine SetEngineData (Transform tm)
        {
            // Get all renderers
            Renderer[] allRenderers = tm.GetComponentsInChildren<Renderer>();
            if (allRenderers == null)
                return null;
            
            // Create engine
            RFEngine engine = new RFEngine (allRenderers.Length);
            
            // Collect mesh data for renderers 
            for (int index = 0; index < allRenderers.Length; ++index)
                engine.AddRendererMesh (allRenderers[index]);

            return engine;
        }
        
        void AddRendererMesh(Renderer renderer)
        {
            // Get unity mesh
            Mesh unMesh    = null;
            bool skinState = false;
            if (renderer.GetType() == typeof(MeshRenderer))
                unMesh = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
            else
            {
                skinState = true;
                unMesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
            }

            // Skip mesh
            if (unMesh == null || unMesh.vertexCount <= 2)
            {
                Debug.Log (renderer.name + " has no mesh");
                return;
            }
            
            // Collect
            unMeshes.Add (unMesh);
            utMeshes.Add (new Utils.Mesh (unMesh));
            renderers.Add (renderer);
            skinStates.Add (skinState);
        }
        
        /// /////////////////////////////////
        /// Methods
        /// /////////////////////////////////

        Material[] GetMaterials(int i)
        {
            return renderers[i].sharedMaterials;
        }

        string GetGroupName(int i)
        {
            return renderers[i].name;
        }
        
        // Get bounds by renderers
        static Bounds GetRendererBounds(List<Renderer> list)
        {
            // Only one bound
            if (list.Count == 1)
                return list[0].bounds;
            
            // New bound
            Bounds bound = new Bounds();
            
            // Basic bounds min and max values
            float minX = list[0].bounds.min.x;
            float minY = list[0].bounds.min.y;
            float minZ = list[0].bounds.min.z;
            float maxX = list[0].bounds.max.x;
            float maxY = list[0].bounds.max.y;
            float maxZ = list[0].bounds.max.z;
            
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].bounds.min.x < minX) minX = list[i].bounds.min.x;
                if (list[i].bounds.min.y < minY) minY = list[i].bounds.min.y;
                if (list[i].bounds.min.z < minZ) minZ = list[i].bounds.min.z;
                if (list[i].bounds.max.x > maxX) maxX = list[i].bounds.max.x;
                if (list[i].bounds.max.y > maxY) maxY = list[i].bounds.max.y;
                if (list[i].bounds.max.z > maxZ) maxZ = list[i].bounds.max.z;
            }
            
            // Get center
            bound.center = new Vector3((maxX - minX) / 2f, (maxY - minY) / 2f, (maxZ - minZ) / 2f);

            // Get min and max vectors
            bound.min = new Vector3(minX, minY, minZ);
            bound.max = new Vector3(maxX, maxY, maxZ);

            return bound;
        }
        
        /// /////////////////////////////////
        /// Matrix ops
        /// /////////////////////////////////
        
        void GetBakedWorldTransformMeshes()
        {
            fragMeshes = new Utils.Mesh[unMeshes.Count][];
            for (int i = 0; i < unMeshes.Count; ++i)
            {
                fragMeshes[i]    = new Utils.Mesh[1];
                fragMeshes[i][0] = new Utils.Mesh (unMeshes[i]);
                if (skinStates[i] == false)
                    fragMeshes[i][0].Transform (renderers[i].transform.localToWorldMatrix);
                else
                    fragMeshes[i][0].TransformByBones (unMeshes[i].bindposes, ((SkinnedMeshRenderer)renderers[i]).bones, false);
            }
        }

        void UnBakeWorldTransform()
        {
            for (int i = 0; i < fragMeshes.Length; ++i)
            {
                for (int index2 = 0; index2 < fragMeshes[i].Length; ++index2)
                {
                    if (skinStates[i] == false)
                        fragMeshes[i][index2].Transform (renderers[i].transform.localToWorldMatrix.inverse);
                    else
                        fragMeshes[i][index2].TransformByBones (unMeshes[i].bindposes, ((SkinnedMeshRenderer)renderers[i]).bones, true);
                }
            }
        }

        ////////////////////////////////////
        // Common usage script
        ////////////////////////////////////
        
        /// <summary>
        /// RFEngine static method to fragment object with Rayfire Shatter component
        /// </summary>
        public static void FragmentShatter(RayfireShatter sh)
        {
            // Set engine data
            sh.engineData = PrepareFragmentation (sh.transform, false);
            
            // Set global bounds by all renderers TODO if Custom/Hex frag type
            sh.bound = GetRendererBounds (sh.engineData.renderers);
            
            // Setup fragmentation for shatter
            SetupFragmentation(sh);
            
            // Get Bias Pos
            sh.engineData.biasPos = sh.advanced.centerBias == null
                ? new Vector3 (0, 0, 0)
                : sh.engineData.normalMat.MultiplyPoint (sh.engineData.biasTN.GetPosition());
            
            // Chose Fragmentation Type and Set Parameters
            SetFragTypeShatter (sh, sh.engineData.sliceData, sh.engineData.normalMat, sh.engineData.biasTN, sh.engineData.biasPos);


            
            // Perform slicing ops
            ProcessFragmentation(sh);
            
            // Create Unity Mesh objects
            List<Transform> fragments = CreateFragmentObjects(sh); // TODO add skinnedMeshesOrigScale
            
            // Create shatter batch. Should be after SetCenterMatrices method
            RFShatterBatch batch = new RFShatterBatch(sh.transform, sh.engineData.rootGO.transform);
            batch.SaveData (sh);
            batch.fragments = fragments;
            sh.batches.Add (batch);
            
            // Sync animation
            sh.engineData.SyncAnimation(sh.transform);
            
            // Destroy shatter on instance hierarchy
            if (sh.engineData.rootGO != null)
                Object.DestroyImmediate(sh.engineData.rootGO.GetComponent<RayfireShatter>());   
            
            // Nullify
            sh.engineData = null;
        }
        
        /// <summary>
        /// RFEngine static method to fragment object with Rayfire Rigid component
        /// </summary>
        public static void FragmentRigid(RayfireRigid rg)
        {
            // Set engine data
            RFEngine engine = PrepareFragmentation (rg.transform, true);
            
            // Set global bounds by all renderers TODO use already calculated
            rg.limitations.bound = GetRendererBounds (engine.renderers);
            
            // Get separate property
            bool separate = rg.meshDemolition.prp.dec;
            
            // Get precap property
            bool precap = rg.meshDemolition.prp.cap;

            // Set slice type. Rigid do not use this feature
            SliceType sliceType = SliceType.ForcedCap;
            
            // Adjusted axis scale for Splinter and Slabs frag types
            Vector3 axisScale = Vector3.one;
            
            // Get aabb transform. Rigid do not use this feature
            Transform cutAABB = null;
            
            // Get AABB separate state
            bool aabbSeparate = false;
            
            // Get Bias TN. used for Splinters, Slabs, Bricks and Radial direction TODO check
            engine.biasTN = Matrix4x4.TRS (rg.limitations.contactVector3, Quaternion.Euler(rg.limitations.contactNormal), Vector3.one);
            
            // Setup fragmentation properties
            SetupFragmentation (engine, rg.transform, separate, precap, sliceType, axisScale, cutAABB, aabbSeparate);
            
            // Get Bias Pos
            engine.biasPos = rg.limitations.contactVector3;
            
            // Chose Fragmentation Type and Set Parameters
            SetFragTypeRigid(rg, engine.sliceData, engine.normalMat, engine.biasTN, engine.biasPos);
            
            // Set fragmentation type TODO Set by shatter
            FragType fragType = FragType.Voronoi; 
            
            // Separated nad fragment elements combine property TODO add in Rigid properties
            bool combine = true;
            
            // Elements fragmentation filter property TODO add in Rigid
            int elements = 1;
            
            // Set cluster properties. Disabled for Rigid
            int clusterCount = 0;
            int layers       = 0;
            
            // Face amount filter property
            int faceFlt = 0;
            
            // Get precap property
            bool postcap = false;
            
            // Inner surface smooth properties
            bool smooth = false;
            
            // Perform slicing ops
            ProcessFragmentation (engine, rg.materials, fragType, combine, elements, clusterCount, layers, faceFlt, postcap, smooth);

            // Get hierarchy type. Always Flat for Rigid
            FragHierarchyType hierarchy = FragHierarchyType.Flat;
            
            // Get inner material
            Material iMat = rg.materials.iMat;
            
            // Rigid always fragments with identity scale
            bool origScale = false;
            
            // Get inner filter state
            bool inner = false;
            
            // Get planar TODO use from UI
            bool planar = false;
            
            // Get shell properties
            RFShell shell = new RFShell();
            
            // Set center matrices for different hierarchy types
            SetCenterMatrices (engine, rg.transform, hierarchy, origScale);
            
            // Create Unity Mesh objects
            List<Transform> fragments = CreateFragmentObjects(engine, iMat, inner, planar, shell);
            
            // Sync animation
            engine.SyncAnimation(rg.transform);

            // Nullify
            engine = null;
        }

        /// /////////////////////////////////
        /// Common Steps
        /// /////////////////////////////////
        
        // STEP 1. Create engine. Prepare objects meshes for fragmentation.
        static RFEngine PrepareFragmentation(Transform tm, bool debug)
        {
            // Start countdown
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            // Set engine data
            RFEngine engine = SetEngineData (tm);
            if (engine == null)
                Debug.Log ("Rayfire : " + tm.name + " object has no renderers to fragment.");
            
            stopWatch.Stop();
            if (debug == true)
                Debug.Log("Rayfire : PrepareFragmentation " + stopWatch.Elapsed.TotalMilliseconds + " ms.");
            
            return engine;
        }
        
        // STEP 2. Setup meshes by their actual tm, set properties, fragment, compute maps
        static void SetupFragmentation(RFEngine engine, Transform tm, bool separate, bool precap, SliceType sliceType, Vector3 axisScale, Transform cutAABB, bool aabbSeparate)
        {
            // Bake World Transform & get RFMeshes Array
            engine.GetBakedWorldTransformMeshes();

            // Normalize meshes Verts to range 0.0f - 1.0f
            engine.normalMat = Utils.Mesh.GetNormMat (engine.fragMeshes, Matrix4x4.TRS (tm.position, tm.rotation * engine.biasTN.rotation, new Vector3 (1, 1, 1)).inverse);

            // Get min and max
            Utils.Mesh.Transform (engine.fragMeshes, engine.normalMat, out engine.aabbMin, out engine.aabbMax);

            // Separate not connected elements
            if (separate == true)
                engine.fragMeshes = Utils.Mesh.Separate (engine.fragMeshes, true);

            // PreCap holes on every element and set not capped open edges array
            engine.openEdgeFlags = new bool[engine.fragMeshes.Length][];
            Utils.Mesh.CheckOpenEdges (engine.fragMeshes, engine.openEdgeFlags, precap);

            // Surface for cut
            SetSliceType (sliceType, engine.fragMeshes, engine.openEdgeFlags);
            
            // Get cutAabb matrix
            Matrix4x4 cutAABBMat = cutAABB != null
                ? engine.normalMat * cutAABB.localToWorldMatrix
                : Matrix4x4.zero;

            // Prepare Slice Data Parameters
            Vector3 aabbCentroid = (engine.aabbMin + engine.aabbMax) * 0.5f;
            engine.aabbSize = engine.aabbMax - engine.aabbMin;

            // Create Slice Data  
            engine.sliceData = new Utils.SliceData();

            // Set fragmentation bounding box
            engine.sliceData.SetAABB (engine.fragMeshes, axisScale, aabbCentroid, engine.aabbSize, cutAABBMat, aabbSeparate);
        }
        
        // STEP 3. Perform slicing ops
        static void ProcessFragmentation(RFEngine engine, RFSurface material, FragType fragType, bool combine, int element, int clusterCount, int layers, int faceFlt, bool outCap, bool smooth) 
        {
            // Custom cell fragmentation state. 100% by default. Can be used for partial per frame fragmentation. 
            for (int i = 0; i < engine.sliceData.GetNumCells(); i++)
                engine.sliceData.Enable (i, true);

            // Fragment for all types except Decompose type
            if (fragType != FragType.Decompose)
                engine.fragMeshes = Utils.Mesh.Slice (engine.sliceData, combine, engine.openEdgeFlags, element * 0.01f, faceFlt, clusterCount, layers, 0);
            
            // OutCap holes on every fragment and set not capped open edges array
            if (outCap == true)
            {
                engine.openEdgeFlags = new bool[engine.fragMeshes.Length][];
                Utils.Mesh.CheckOpenEdges (engine.fragMeshes, engine.openEdgeFlags, true);
            }

            // Get inner sub id TODO add support for other renderers, not only first renderer materials
            engine.innerSubId = GetInnerSubId(material.iMat, engine.GetMaterials(0));
            
            // Build SubMeshes
            Utils.Mesh.BuildSubMeshes(engine.fragMeshes, engine.unMeshes, engine.innerSubId);

            // Undo Normalize
            Utils.Mesh.Transform(engine.fragMeshes, engine.normalMat.inverse);
            
            // Undo World Transform
            engine.UnBakeWorldTransform();
            
            // Restore Maps
            ComputeMaps(engine, material.MappingScale, material.UvRegionMin, material.UvRegionMax, material.Color, smooth);
           
            // Centerize
            engine.centroids = Utils.Mesh.Centerize(engine.fragMeshes);
        }
        
        // STEP 4. Create Unity Mesh objects
        static List<Transform> CreateFragmentObjects(RFEngine engine, Material iMat, bool inner, bool planar, RFShell shell)
        {
            // Fragments list
            List<Transform> fragments = new List<Transform>();
            
            // Transform by scale matrix
            if (engine.centroidScaleMat != null)
                Utils.Mesh.Transform(engine.fragMeshes, engine.centroidScaleMat);
            
            // Create fragments 
            for (int i = 0; i < engine.fragMeshes.Length; i++)
                if (engine.skinStates[i] == false)
                    CreateMeshFragments (engine, i, iMat, inner, planar, shell, ref fragments);
                else
                    CreateSkinFragments (engine, i, iMat, inner, planar, ref fragments);

            return fragments;
        }
        
        // Create
        static void CreateMeshFragments(RFEngine engine, int ind, Material iMat, bool inner, bool planar, RFShell shell, ref List<Transform> fragments)
        {
            // Create local fragments root
            Transform fragsGroupTm = engine.oldNewGoMap[engine.renderers[ind].gameObject].transform;
            fragsGroupTm.name += "_frags";
            
            // Create fragments
            for (int j = 0; j < engine.fragMeshes[ind].Length; j++)
            {
                
                // Skip inner fragments by inner filter
                if (inner == true && engine.fragMeshes[ind][j].GetFragLocation() == Utils.Mesh.FragmentLocation.Inner)
                    continue;

                // Create fragment mesh
                Mesh fragMesh = CreateMesh (engine.fragMeshes[ind][j], engine.fragMaps[ind][j]);

                // Skip planar fragments
                if (planar == true)
                    if (RFShatterAdvanced.IsCoplanar (fragMesh, RFShatterAdvanced.planarThreshold) == true)
                        continue;

                // Create fragment object
                GameObject fragGo = new GameObject (engine.GetGroupName (ind) + "_sh_" + j);

                // Set root as parent
                fragGo.transform.SetParent (fragsGroupTm, false);

                //S et mesh name
                fragMesh.name = fragGo.name;

                
                // Add shell
                if (shell.enable == true)
                    fragMesh = RFShell.AddShell (engine.fragMeshes[ind][j], fragMesh, shell.bridge, shell.submesh, shell.thickness);

                // Add meshfilter
                MeshFilter mf = fragGo.AddComponent<MeshFilter>();
                mf.sharedMesh = fragMesh;

                // Add renderer and materials
                MeshRenderer mr = fragGo.AddComponent<MeshRenderer>();
                mr.sharedMaterials = GetCorrectMaterials (engine.GetMaterials (ind), engine.fragMeshes[ind][j], iMat);

                // Set local position based on hierarchy type
                fragGo.transform.localPosition = engine.centroidScaleMat == null
                    ? engine.centroids[ind][j]
                    : engine.centroidScaleMat[ind].MultiplyPoint (engine.centroids[ind][j]);

                // Flat hierarchy 
                if (engine.flatExtraMat != null)
                {
                    fragGo.transform.localPosition =  engine.flatExtraMat[ind].rotation * fragGo.transform.localPosition;
                    fragGo.transform.localPosition += engine.flatExtraMat[ind].GetPosition();
                    fragGo.transform.localRotation =  engine.flatExtraMat[ind].rotation;
                }
                
                // Collect batch data
                fragments.Add (fragGo.transform);
            }
        }
        
        // Create
        static void CreateSkinFragments(RFEngine engine, int ind, Material iMat, bool inner, bool planar, ref List<Transform> fragments)
        {
            // Create local fragments root
            Transform fragsGroupTm = engine.oldNewGoMap[engine.renderers[ind].gameObject].transform;
            fragsGroupTm.name += "_frags";
            
            // Get bones transforms TODO move out of for, use if (engine.skinStates[i] == true)
            Transform[] bones = engine.GetBonesTNS(ind);
            
            // Create fragments
            for (int j = 0; j < engine.fragMeshes[ind].Length; j++)
            {
                
                // Skip inner fragments by inner filter
                if (inner == true)
                    if (engine.fragMeshes[ind][j].GetFragLocation() == Utils.Mesh.FragmentLocation.Inner)
                        continue;
                
                // Create fragment mesh
                Mesh fragMesh = CreateMesh (engine.fragMeshes[ind][j], engine.fragMaps[ind][j]);

                // Skip planar fragments
                if (planar == true)
                    if (RFShatterAdvanced.IsCoplanar (fragMesh, RFShatterAdvanced.planarThreshold) == true)
                        continue;
                
                // Create fragment object
                GameObject fragGo = new GameObject(engine.GetGroupName(ind) + "_sh_" + j);
                
                // Set root as parent
                fragGo.transform.SetParent(fragsGroupTm, false);
                
                //S et mesh name
                fragMesh.name = fragGo.name;
                
                
                // Set bone data
                fragMesh.boneWeights = engine.fragMeshes[ind][j].GetSkinData();
                
                Matrix4x4   centroidMat       = Matrix4x4.Translate(engine.centroids[ind][j]);
                Matrix4x4[] bindPoses         = new Matrix4x4[engine.unMeshes[ind].bindposes.Length];
                Matrix4x4[] bindPosesForScale = new Matrix4x4[engine.unMeshes[ind].bindposes.Length];
                for (int ii = 0; ii < bindPoses.Length; ii++)
                {
                    bindPoses[ii]         = engine.unMeshes[ind].bindposes[ii] * centroidMat;
                    bindPosesForScale[ii] = bindPoses[ii];
                }
                fragMesh.bindposes = bindPoses;
                
                // TODO sh.skinnedMeshesOrigScale.Add(Tuple.Create(fragMesh, bindPosesForScale));
                
                SkinnedMeshRenderer smr = fragGo.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMaterials = GetCorrectMaterials(engine.GetMaterials(ind), engine.fragMeshes[ind][j], iMat);
                smr.bones           = bones;
                smr.rootBone        = engine.GetRootBone(ind);
                smr.sharedMesh      = fragMesh;
                
                // TODO add absolute and relative size filters
                
                // Collect batch data
                fragments.Add (fragGo.transform);
            }
        }

        /// /////////////////////////////////
        /// Shatter only Steps
        /// /////////////////////////////////
        
        // STEP 2. Shatter only. Set properties
        static void SetupFragmentation(RayfireShatter sh)
        {
            // Get separate property. Should be enabled fro decompose
            bool separate = sh.advanced.separate;
            if (sh.type == FragType.Decompose)
                separate = true;

            // Get precap property
            bool precap = sh.advanced.inpCap;

            // Set slice type
            SliceType sliceType = sh.advanced.sliceType;
            
            // Adjusted axis scale for Splinter and Slabs frag types
            Vector3 axisScale = GetAxisScale(sh);

            // Get aabb transform
            Transform cutAABB = sh.advanced.aabbEnable == true && sh.advanced.aabbObject != null
                ? sh.advanced.aabbObject
                : null;

            // Get AABB separate state
            bool aabbSeparate = sh.advanced.aabbSeparate;
            
            // Get Bias TN
            sh.engineData.biasTN = sh.advanced.centerSet == true && sh.advanced.centerBias != null 
                ? sh.advanced.centerBias.localToWorldMatrix
                : Matrix4x4.identity;
            
            // Setup fragmentation properties
            SetupFragmentation (sh.engineData, sh.transform, separate, precap, sliceType, axisScale, cutAABB, aabbSeparate);
        }
        
        // STEP 3. Perform slicing ops. Shatter only
        static void ProcessFragmentation(RayfireShatter sh) 
        {
            // Set fragmentation type
            FragType fragType = sh.type; 
            
            // Separated nad fragment elements combine property
            bool combine = sh.advanced.combine;

            // Elements fragmentation filter property
            int elements = sh.advanced.element;
            
            // Set cluster properties
            int clusterCount = sh.clusters.count;
            int layers       = sh.clusters.layers;
            if (sh.clusters.enable == false)
                clusterCount = 0;
            
            // Face amount filter property
            int faceFlt = sh.advanced.faceFlt;

            // Get outCap property
            bool outCap = sh.advanced.sliceType == SliceType.ForcedCap;
            
            // Inner surface smooth properties
            bool smooth = sh.advanced.smooth;
            
            // Perform slicing ops
            ProcessFragmentation (sh.engineData, sh.material, fragType, combine, elements, clusterCount, layers, faceFlt, outCap, smooth);
        }
        
        // STEP 4. Create Unity Mesh objects
        static List<Transform> CreateFragmentObjects(RayfireShatter sh)
        {
            // Get hierarchy type
            FragHierarchyType hierarchy = sh.advanced.hierarchy;

            // Get original scale state
            bool origScale = sh.advanced.origScale;
            
            // Get inner filter state
            bool inner = sh.advanced.inner;
            
            // Get planar filter state 
            bool planar = sh.advanced.planar;
            
            // Reset list for skinned scale preview
            sh.skinnedMeshesOrigScale = new List<Tuple<Mesh, Matrix4x4[]>>();
            
            // Set center matrices for different hierarchy types
            SetCenterMatrices (sh.engineData, sh.transform, hierarchy, origScale);
            
            // Create Unity Mesh objects
            return CreateFragmentObjects(sh.engineData, sh.material.iMat, inner, planar, sh.shell); // TODO add skinnedMeshesOrigScale
        }
        
        /// /////////////////////////////////
        /// Shatter script
        /// /////////////////////////////////
        
        /*
        public static void FragmentOLD(RayfireShatter sh)
        {
            // Set engine data
            sh.engineData = PrepareFragmentation (sh.transform, true);
            
            // Set global bounds by all renderers TODO if Custom/Hex frag type
            sh.bound = GetRendererBounds (sh.engineData.renderers);
            
            // Bake World Transform & get RFMeshes Array
            sh.engineData.GetBakedWorldTransformMeshes();
            
            // Get Bias TN
            Matrix4x4 biasTN = sh.advanced.centerSet == true && sh.advanced.centerBias != null 
                ? sh.advanced.centerBias.localToWorldMatrix
                : Matrix4x4.identity;
            
            // Normalize meshes Verts to range 0.0f - 1.0f
            sh.engineData.normalMat = Utils.Mesh.GetNormMat(sh.engineData.fragMeshes, Matrix4x4.TRS(sh.transform.position, sh.transform.rotation * biasTN.rotation, new Vector3(1, 1, 1)).inverse);

            // Get min and max
            Utils.Mesh.Transform(sh.engineData.fragMeshes, sh.engineData.normalMat, out sh.engineData.aabbMin, out sh.engineData.aabbMax);
            
            // Separate not connected elements
            if (sh.advanced.separate == true)
                sh.engineData.fragMeshes = Utils.Mesh.Separate(sh.engineData.fragMeshes, true);
            
            // PreCap holes on every element and set not capped open edges array
            sh.engineData.openEdgeFlags = new bool[sh.engineData.fragMeshes.Length][];
            Utils.Mesh.CheckOpenEdges(sh.engineData.fragMeshes, sh.engineData.openEdgeFlags, sh.advanced.inpCap);

            // Surface for cut
            SetSliceType (sh.advanced.sliceType, sh.engineData.fragMeshes, sh.engineData.openEdgeFlags);
            
            // Adjusted axis scale for Splinter and Slabs frag types
            Vector3 axisScale = GetAxisScale(sh);

            // Get cutAabb matrix
            Matrix4x4 cutAABB = sh.advanced.aabbEnable == true && sh.advanced.aabbObject != null
                ? sh.engineData.normalMat * sh.advanced.aabbObject.localToWorldMatrix
                : Matrix4x4.zero;
            
            // Prepare Slice Data Parameters
            Vector3 aabbCentroid = (sh.engineData.aabbMin + sh.engineData.aabbMax) * 0.5f;
            Vector3 aabbSize     = sh.engineData.aabbMax - sh.engineData.aabbMin;
            
            // Create Slice Data  
            sh.engineData.sliceData = new Utils.SliceData();
            
            // Set fragmentation bounding box
            sh.engineData.sliceData.SetAABB(sh.engineData.fragMeshes, axisScale, aabbCentroid, aabbSize, cutAABB, sh.advanced.aabbSeparate);       
            
            // Get Bias Pos
            Vector3 biasPos = sh.advanced.centerBias == null 
                ? new Vector3(0, 0, 0) 
                : sh.engineData.normalMat.MultiplyPoint(biasTN.GetPosition());
            
            // Chose Fragmentation Type and Set Parameters
            SetFragTypeShatter (sh, sh.engineData.sliceData, sh.engineData.normalMat, biasTN, biasPos);
            
            // Custom cell fragmentation state. 100% by default. Can be used for partial per frame fragmentation. 
            for (int i = 0; i < sh.engineData.sliceData.GetNumCells(); i++)
                sh.engineData.sliceData.Enable (i, true);
            
            // Set cluster properties  TODO disable with runtime caching
            int clusterCount = sh.clusters.count;
            if (sh.clusters.enable == false)
                clusterCount = 0;

            // Fragment for all types except Decompose type
            sh.advanced.faceFlt = 0;
            if (sh.type != FragType.Decompose)
                sh.engineData.fragMeshes = Utils.Mesh.Slice (sh.engineData.sliceData, sh.advanced.combine, sh.engineData.openEdgeFlags, sh.advanced.element * 0.01f, sh.advanced.faceFlt, clusterCount, sh.clusters.layers);
            
            // Get inner sub id TODO add support for other renderers, not only first renderer materials
            sh.engineData.innerSubId = GetInnerSubId(sh.material.iMat, sh.engineData.GetMaterials(0));
            
            // Build SubMeshes
            Utils.Mesh.BuildSubMeshes(sh.engineData.fragMeshes, sh.engineData.unMeshes, sh.engineData.innerSubId);

            // Undo Normalize
            Utils.Mesh.Transform(sh.engineData.fragMeshes, sh.engineData.normalMat.inverse);
            
            // Undo World Transform
            sh.engineData.UnBakeWorldTransform();
            
            // Restore Maps
            ComputeMaps(sh.engineData, sh.material.MappingScale, sh.material.UvRegionMin, sh.material.UvRegionMax, sh.material.Color, sh.advanced.smooth);
           
            // Centerize
            sh.engineData.centroids = Utils.Mesh.Centerize(sh.engineData.fragMeshes);
            
            // Reset list for skinned scale preview
            sh.skinnedMeshesOrigScale = new List<Tuple<Mesh, Matrix4x4[]>>();
            
            // Create Unity Mesh objects
            List<Transform> fragments = CreateFragmentObjects(sh.transform, sh.engineData, sh.advanced.hierarchy, sh.material.iMat, sh.advanced.origScale, sh.advanced.inner, sh.shell);

            // Create shatter batch. Should be after SetCenterMatrices method
            RFShatterBatch batch = new RFShatterBatch(sh.transform, sh.engineData.rootGO.transform);
            batch.SaveData (sh);
            batch.fragments = fragments;
            sh.batches.Add (batch);
            
            // Skinned mesh preview ops
            if (sh.skinnedMeshesOrigScale != null && sh.skinnedMeshesOrigScale.Count == 0)
                sh.skinnedMeshesOrigScale = null;

            // Destroy shatter on instance hierarchy
            if (sh.engineData.rootGO != null)
                Object.DestroyImmediate(sh.engineData.rootGO.GetComponent<RayfireShatter>());            
            
            // Sync animation
            sh.engineData.SyncAnimation(sh.transform);
        }
        */

        // Chose Fragmentation Type and Set Parameters
        static void SetFragTypeShatter(RayfireShatter sh, Utils.SliceData sd, Matrix4x4 normalMat, Matrix4x4 biasTN, Vector3 biasPos)
        {
            switch (sh.type)
            {
                case FragType.Voronoi:
                {
                    sd.GenRandomPoints(sh.voronoi.Amount, sh.advanced.Seed);
                    sd.ApplyCenterBias(biasPos, sh.voronoi.centerBias);
                    sd.BuildCells();
                    break;
                }
                case FragType.Splinters:
                {
                    sd.GenRandomPoints(sh.splinters.Amount, sh.advanced.Seed);
                    sd.ApplyCenterBias(biasPos, sh.splinters.centerBias);
                    sd.BuildCells();
                    break;
                }
                case FragType.Slabs:
                {
                    sd.GenRandomPoints(sh.slabs.Amount, sh.advanced.Seed);
                    sd.ApplyCenterBias(biasPos, sh.slabs.centerBias);
                    sd.BuildCells();
                    break;
                }
                case FragType.Radial:
                {
                    Vector3 aabbAbsSize      = normalMat.inverse *  sh.engineData.aabbMax;
                    float   radiusExtraScale = Mathf.Max(aabbAbsSize.x, aabbAbsSize.y, aabbAbsSize.z);
                    sd.GenRadialPoints(
                        sh.advanced.Seed, 
                        sh.radial.rays, 
                        sh.radial.rings, 
                        sh.radial.radius / (radiusExtraScale * sh.radial.rings * 2f),
                        sh.radial.divergence * 2.0f, 
                        sh.radial.twist / 90.0f, 
                        (int)sh.radial.centerAxis, 
                        biasPos, 
                        sh.radial.focus * 0.01f, 
                        sh.radial.randomRings * 0.01f,
                        sh.radial.randomRays * 0.01f);
                    sd.BuildCells();
                    break;
                }
                case FragType.Hexagon:
                {
                    List<Vector3> customPnt        = RFHexagon.GetHexPointCLoudV2 (sh.hexagon, sh.CenterPos, sh.CenterDir, sh.bound);
                    List<Vector3> customPointsList = new List<Vector3>();
                    for(int i = 0; i < customPnt.Count; i++)
                        customPointsList.Add(normalMat.MultiplyPoint(biasTN * customPnt[i]));
                    sd.SetCustomPoints(customPointsList);
                    float centerBias = 0;
                    sd.ApplyCenterBias(biasPos, centerBias);
                    sd.BuildCells();
                    break;
                }
                case FragType.Custom:
                {
                    List<Vector3> customPnt        = RFCustom.GetCustomPointCLoud (sh.custom, sh.transform, sh.advanced.Seed, sh.bound);
                    List<Vector3> customPointsList = new List<Vector3>();
                    for(int i = 0; i < customPnt.Count; i++)
                        customPointsList.Add(normalMat.MultiplyPoint(biasTN * customPnt[i]));
                    sd.SetCustomPoints(customPointsList);
                    float centerBias = 0;
                    sd.ApplyCenterBias(biasPos, centerBias);
                    sd.BuildCells();
                    break;
                }
                case FragType.Slices:
                {
                    // Set slice data by transforms. Shatter usage. 
                    sd.AddPlanes(sh.slice.sliceList.ToArray(), new Vector3(0, 1, 0), normalMat);
                    
                    // Set slice data by vector arrays. TODO Use for Rigid
                    //Vector3[] pos = null;
                    //Vector3[] norm = null;
                    //sd.AddPlanes(pos, norm, normalMat);
                    
                    break;
                }
                case FragType.Bricks:
                {
                    sd.GenBricks(
                        sh.bricks.Size,
                        sh.bricks.Num, 
                        sh.bricks.SizeVariation, 
                        sh.bricks.SizeOffset, 
                        sh.bricks.SplitState,
                        sh.bricks.SplitPro);
                    break;
                }
                case FragType.Voxels:
                {
                    sd.GenBricks(
                        sh.voxels.Size,
                        sh.bricks.SplitState, // has 0 vector int
                        Vector3.zero, 
                        Vector3.zero, 
                        sh.voxels.SplitState,
                        Vector3.zero);
                    break;
                }
                case FragType.Tets:
                {
                    // TODO
                    Debug.Log ("Tetrahedron fragmentation is not supported yet by V2 Beta engine");
                    break; 
                }
            }
        }
        
        // Chose Fragmentation Type and Set Parameters
        static void SetFragTypeRigid(RayfireRigid rg, Utils.SliceData sd, Matrix4x4 normalMat, Matrix4x4 biasTN, Vector3 biasPos)
        {
            // Set Rigid Demolition frag properties by Shatter 
            if (rg.meshDemolition.use == true && rg.meshDemolition.sht != null)
            {
                SetFragTypeShatter (rg.meshDemolition.sht, sd, normalMat, biasTN, biasPos);
            }
            
            // Set default Rigid fragmentation properties
            else
            {
                // TODO fix amount and seed properties
                sd.GenRandomPoints (rg.meshDemolition.am, rg.meshDemolition.sd);
                sd.ApplyCenterBias (biasPos, rg.meshDemolition.bias);
                sd.BuildCells();
                
                // TODO set slice planes
            }
        }

        // Surface for cut
        static void SetSliceType(SliceType slice, Utils.Mesh[][] fragMeshes, bool[][] openEdgeFlags)
        {
            if (slice != SliceType.Hybrid)
                for (int i = 0; i < fragMeshes.Length; i++)
                    for (int j = 0; j < fragMeshes[i].Length; j++)
                        openEdgeFlags[i][j] = slice == SliceType.ForcedCut;
        }

        // Adjusted axis scale for Splinter and Slabs frag types
        static Vector3 GetAxisScale(RayfireShatter sh)
        {
            Vector3 axisScale = Vector3.one;
            if (sh.type == FragType.Splinters)
            {
                float stretch = Mathf.Min (1.0f, Mathf.Max (0.005f, Mathf.Pow (1.0f - sh.splinters.strength, 1.5f)));
                if (sh.splinters.axis == AxisType.XRed)
                    axisScale.x = stretch;
                else if (sh.splinters.axis == AxisType.YGreen)
                    axisScale.y = stretch;
                else if (sh.splinters.axis == AxisType.ZBlue)
                    axisScale.z = stretch;
            }
            else if (sh.type == FragType.Slabs)
            {
                float stretch = Mathf.Min (1.0f, Mathf.Max (0.005f, Mathf.Pow (1.0f - sh.slabs.strength, 1.5f)));
                if (sh.slabs.axis == AxisType.XRed)
                    axisScale.y = axisScale.z = stretch;
                else if (sh.slabs.axis == AxisType.YGreen)
                    axisScale.x = axisScale.z = stretch;
                else if (sh.slabs.axis == AxisType.ZBlue)
                    axisScale.x = axisScale.y = stretch;
            }
            return axisScale;
        }
        
        // Create fragment mesh
        static Mesh CreateMesh(Utils.Mesh utilsMesh, Utils.MeshMaps map)
        {
            Mesh fragMesh = new Mesh();
            fragMesh.indexFormat = utilsMesh.GetNumTri() * 3 < ushort.MaxValue 
                ? UnityEngine.Rendering.IndexFormat.UInt16 
                : UnityEngine.Rendering.IndexFormat.UInt32;
            fragMesh.subMeshCount = utilsMesh.GetNumSubMeshes();
            fragMesh.vertices     = utilsMesh.GetVerts();
                    
            // Set triangles
            for (int subMesh = 0; subMesh < utilsMesh.GetNumSubMeshes(); subMesh++)
                fragMesh.SetTriangles(utilsMesh.GetSubTris(subMesh), subMesh);
                    
            // Set UV data
            map.GetMaps(fragMesh);
            return fragMesh;
        }

        // Get inner sub id in case of inner material usage
        static int GetInnerSubId(Material innerMaterial, Material[] materials)
        {
            // Inner surface should have custom inner material
            if (innerMaterial != null)
            {
                // Object already has Inner material applied to one of the submesh
                if (materials.Contains (innerMaterial) == true)
                    for (int i = 0; i < materials.Length; i++)
                        if (innerMaterial == materials[i])
                            return i;
                
                // Object has no inner material applied
                return -1;
            }
            
            // Apply first material to inner surface
            return 0;
        }

        // Set materials to fragments
        static Material[] GetCorrectMaterials(Material[] materials, Utils.Mesh rfMesh, Material innerMaterial)
        {
            List<Material> correctMaterials = new List<Material>();
            for (int ii = 0; ii < rfMesh.GetNumSubMeshes(); ii++)
            {
                int origSubID = rfMesh.GetSubMeshID(ii);
                if (origSubID >= 0) // if == -1 then its a inner faces new submesh
                {
                    correctMaterials.Add(materials[origSubID]);
                }
                if (origSubID == -1) // if == -1 then its a inner faces new submesh
                {
                    correctMaterials.Add(innerMaterial);
                }
            }
            return correctMaterials.ToArray();
        }

        /// /////////////////////////////////
        /// Shatter Interactive ops
        /// /////////////////////////////////
        
        // Fragment all meshes into own mesh
        public static void InteractiveStart(RayfireShatter scr)
        {
            // Set engine data
            scr.engineData = PrepareFragmentation (scr.transform, true);
            
            // Set global bounds by all renderers TODO if Custom/Hex frag type
            scr.bound = GetRendererBounds (scr.engineData.renderers);
            
            // Setup fragmentation for shatter
            SetupFragmentation(scr);
            
            // Get Bias Pos
            scr.engineData.biasPos = scr.advanced.centerBias == null
                ? new Vector3 (0, 0, 0)
                : scr.engineData.normalMat.MultiplyPoint (scr.engineData.biasTN.GetPosition());
            
            // Chose Fragmentation Type and Set Parameters
            SetFragTypeShatter (scr, scr.engineData.sliceData, scr.engineData.normalMat, scr.engineData.biasTN, scr.engineData.biasPos);

            // Perform slicing ops
            ProcessFragmentation(scr);

            /*
            // TODO Convert engine meshes to meshes and pivots array
            List<Mesh>    meshList  = new List<Mesh>();
            List<Vector3> pivotList = new List<Vector3>();
            for (int i = 0; i < scr.engineData.fragMeshes.Length; i++)
            {
                for (int j = 0; j < scr.engineData.fragMeshes[i].Length; j++)
                {
                    meshList.Add (CreateMesh (scr.engineData.fragMeshes[i][j], scr.engineData.fragMaps[i][j]));
                    pivotList.Add (Vector3.zero);
                }
            }
            scr.meshes = meshList.ToArray();
            scr.pivots = pivotList.ToArray();
            List<Dictionary<int, int>> ids = new List<Dictionary<int, int>>();
            ids.Add (new Dictionary<int, int>());
            Debug.Log (scr.meshes.Length);
            */
            
            
            // Create interactive object if there is no any
            scr.InteractiveCreate();
            
            // Stop
            if (scr.meshes == null)
            {
                scr.OriginalRenderer(true);
                return;
            }
            
          
            
            // Weld into one mesh and set to interactive object
            // scr.intMf.sharedMesh = RFShatter.WeldMeshes (ref scr.meshes, ref scr.pivots, ref ids, scr.PreviewScale());
            
            // Disable own Renderer TODO affect all rendrers
            scr.OriginalRenderer(false);
        }
        
        // Property changed
        public static void InteractiveChange(RayfireShatter scr)
        {
           
        }
        
        /// /////////////////////////////////////////////////////////
        /// Hierarchy
        /// /////////////////////////////////////////////////////////
        
        // Set center matrices for different hierarchy types
        static void SetCenterMatrices(RFEngine engine, Transform tm, FragHierarchyType hierarchy, bool origScale)
        {
            // Check if has skinned meshes and use Instance hierarchy if has
            if (engine.HasSkin == true)
            {
                origScale = true;
                hierarchy = FragHierarchyType.Instance;
            }

            // Just one object, no need to copy
            if (hierarchy == FragHierarchyType.Copy)
                if (engine.renderers.Count == 1)
                    hierarchy = FragHierarchyType.Flat;
            
            // Set centers based on hierarchy type
            switch (hierarchy)
            {
                case FragHierarchyType.Instance:
                {
                    engine.centroidScaleMat = engine.InstHierarchy(tm, origScale);
                    engine.flatExtraMat     = null;
                    break;
                }
                case FragHierarchyType.Copy:
                {
                    engine.centroidScaleMat = engine.CopyHierarchy(tm, origScale);
                    engine.flatExtraMat     = null;
                    break;
                }
                case FragHierarchyType.Flat:
                {
                    engine.centroidScaleMat = engine.FlatHierarchy(tm, origScale);
                    engine.flatExtraMat      = new Matrix4x4[engine.renderers.Count];
                    for (int i = 0; i < engine.flatExtraMat.Length; i++)
                        engine.flatExtraMat[i] = engine.rootGO.transform.localToWorldMatrix.inverse * engine.renderers[i].transform.localToWorldMatrix;
                    break;
                }
            }
        }
        
        // instantiate root structure. Only option for skinned mesh fragmentation
        Matrix4x4[] InstHierarchy(Transform tm, bool origScale)
        {
            rootGO = Object.Instantiate(tm.gameObject);
            Transform[] origObjs = tm.GetComponentsInChildren<Transform>();
            Transform[] newObjs  = rootGO.GetComponentsInChildren<Transform>();

            oldNewGoMap.Clear();

            // Destroy Renderer and Meshfilters to use as roots
            for (int i = 0; i < origObjs.Length; i++)
            {
                Renderer rnd = newObjs[i].gameObject.GetComponent<Renderer>();
                if (rnd != null)
                    Object.DestroyImmediate(rnd);

                MeshFilter mf = newObjs[i].gameObject.GetComponent<MeshFilter>();
                if (mf != null)
                    Object.DestroyImmediate(mf);

                oldNewGoMap.Add(origObjs[i].gameObject, newObjs[i].gameObject);
                
                // Set identity scale
                if (origScale == false)
                {
                    newObjs[i].localScale = new Vector3(1, 1, 1);
                    newObjs[i].position = origObjs[i].position;
                    newObjs[i].rotation = origObjs[i].rotation;
                }
            }

            // Fix name
            rootGO.name = rootGO.name.Remove (rootGO.name.Length - 7, 7);

            // Skip if roots keep original scale
            if (origScale == true)
                return null;
            
            Matrix4x4[] globalScales = new Matrix4x4[renderers.Count];
            for(int i = 0; i < globalScales.Length; i++)
                globalScales[i] = Matrix4x4.Scale(renderers[i].gameObject.transform.lossyScale);
                                     
            return globalScales;
        }
          
        // Create full copy of root structure
        Matrix4x4[] CopyHierarchy(Transform tm, bool origScale)
        {
            oldNewGoMap.Clear();

            // Create the same root structure
            for (int i = 0; i < renderers.Count; i++)
                GetChain(tm, renderers[i].transform, ref oldNewGoMap, ref rootGO);

            // Skip if roots keep original scale
            if (origScale == true)
                return null;

            // Set frag roots scale to [1 1 1] 
            Transform[] oldTms = tm.GetComponentsInChildren<Transform>();
            for (int i = 0; i < oldTms.Length; i++)
            {
                if(oldNewGoMap.ContainsKey(oldTms[i].gameObject))
                {
                    Transform newTm = oldNewGoMap[oldTms[i].gameObject].transform;
                    newTm.localScale = new Vector3(1, 1, 1);
                    newTm.position   = oldTms[i].position;
                    newTm.rotation   = oldTms[i].rotation;
                }              
            }

            // Save scale matrices because of roots identity scale
            Matrix4x4[] globalScales = new Matrix4x4[renderers.Count];
            for (int i = 0; i < globalScales.Length; i++)
                globalScales[i] = Matrix4x4.Scale(renderers[i].gameObject.transform.lossyScale);
            
            return globalScales;
        }
        
        // Create root structure copy
        static void GetChain(Transform mainTm, Transform rendTm, ref Dictionary<GameObject, GameObject> oldNewGo, ref GameObject rootGO)
        {
            GameObject gameObject1 = null;
            GameObject gameObject2;
            
            // Repeat for all fragmented meshes transform
            while (true)
            {
                // Check if mesh tm in dictionary. 
                if (oldNewGo.ContainsKey(rendTm.gameObject) == true)
                {
                    // Set corresponding pair in original structure. 
                    gameObject2 = oldNewGo[rendTm.gameObject];
                }
                
                // Mesh tm is not yet in dictionary
                else
                {
                    // Create corresponding object for structure copy
                    gameObject2 = new GameObject(rendTm.name);
                    
                    // Set position, rotation, scale
                    gameObject2.transform.localPosition = rendTm.localPosition;
                    gameObject2.transform.localRotation = rendTm.localRotation;
                    gameObject2.transform.localScale    = rendTm.localScale;
                    
                    // Collect to dictionary
                    oldNewGo.Add(rendTm.gameObject, gameObject2);
                }
                
                // Set defined object as child for corresponding object in structure copy
                if (gameObject1 != null)
                    gameObject1.transform.SetParent(gameObject2.transform, false);
                
                // If mesh tm is not main root tm set rendTm to parent for next cycle
                if (rendTm != mainTm)
                {
                    gameObject1 = gameObject2;
                    rendTm      = rendTm.parent;
                }
                
                // Break if mesh time finally main root tm.
                else
                    break;
            }
            rootGO = gameObject2;
        }
        
        // Create flat hierarchy only with one main root
        Matrix4x4[] FlatHierarchy(Transform tm, bool origScale)
        {
            // Create root
            rootGO                    = new GameObject();
            rootGO.transform.position = tm.position;
            rootGO.transform.rotation = tm.rotation;
            rootGO.transform.localScale = origScale == false 
                ? new Vector3(1, 1, 1)
                : tm.lossyScale;
            rootGO.name = tm.name;

            // Collect oldNew map
            oldNewGoMap.Clear();
            for (int i = 0; i < renderers.Count; i++)
                oldNewGoMap.Add(renderers[i].gameObject, rootGO);
            
            Matrix4x4[] globalScales = new Matrix4x4[renderers.Count];
            if (origScale == false)
                for (int i = 0; i < globalScales.Length; i++)
                    globalScales[i] = Matrix4x4.Scale(renderers[i].gameObject.transform.lossyScale);
            else
                for (int i = 0; i < globalScales.Length; i++)
                    globalScales[i] = Matrix4x4.Scale((tm.localToWorldMatrix.inverse * renderers[i].gameObject.transform.localToWorldMatrix).lossyScale);
            return globalScales;
        }
        
        /// /////////////////////////////////
        /// Maps ops
        /// /////////////////////////////////

        // Compute texture coordinates, normals and vertex color
        static void ComputeMaps(RFEngine engine, float uvScale, Vector2 uvAreaBegin, Vector2 uvAreaEnd, Color innerColor, bool smoothInner) 
        {
            Utils.Mesh[]     rfMeshes = engine.utMeshes.ToArray();
            Utils.MeshMaps[] origMaps = GetMaps(engine);
            float            num      = Math.Max(engine.normalMat.lossyScale.x, Math.Max(engine.normalMat.lossyScale.y, engine.normalMat.lossyScale.z));
            uvScale = (num == 0.0 
                ? 0.0f 
                : 1f / num) * uvScale;
            engine.fragMaps = new Utils.MeshMaps[engine.fragMeshes.Length][];
            for (int index1 = 0; index1 < engine.fragMeshes.Length; ++index1)
            {
                engine.fragMaps[index1] = new Utils.MeshMaps[engine.fragMeshes[index1].Length];
                for (int index2 = 0; index2 < engine.fragMeshes[index1].Length; ++index2)
                {
                    engine.fragMaps[index1][index2] = new Utils.MeshMaps();
                    engine.fragMaps[index1][index2].BuildBary(rfMeshes[index1], engine.fragMeshes[index1][index2]);

                    // Normals
                    if (engine.skinStates[index1] == false)
                        engine.fragMaps[index1][index2].ComputeNormals(origMaps[index1], rfMeshes[index1], engine.fragMeshes[index1][index2], null, null, smoothInner);
                    else
                        engine.fragMaps[index1][index2].ComputeNormals(origMaps[index1], rfMeshes[index1], engine.fragMeshes[index1][index2], engine.unMeshes[index1].bindposes, ((SkinnedMeshRenderer)engine.renderers[index1]).bones, smoothInner);
                    
                    // Texture coordinates
                    engine.fragMaps[index1][index2].ComputeTexCoords(origMaps[index1], rfMeshes[index1], engine.fragMeshes[index1][index2], uvScale, uvAreaBegin, uvAreaEnd);
                    
                    // Vertex color
                    engine.fragMaps[index1][index2].ComputeVertexColors(origMaps[index1], rfMeshes[index1], engine.fragMeshes[index1][index2], innerColor);
                }
            }
        }
        
        // Get unity mesh maps
        static Utils.MeshMaps[] GetMaps(RFEngine engine)
        {
            Utils.MeshMaps[] maps = new Utils.MeshMaps[engine.unMeshes.Count];
            for (int index = 0; index < engine.unMeshes.Count; ++index)
            {
                maps[index] = new Utils.MeshMaps();
                maps[index].SetNormals(engine.unMeshes[index].normals);
                maps[index].SetTexCoords (engine.unMeshes[index]);
                maps[index].SetVertexColors(engine.unMeshes[index].colors);
            }
            return maps;
        }
        
        /// /////////////////////////////////
        /// Skinned ops
        /// /////////////////////////////////
        
        // Get bones transforms
        Transform[] GetBonesTNS(int i)
        {
            if (skinStates[i] == false)
                return null;
            
            Transform[] bones    = ((SkinnedMeshRenderer)renderers[i]).bones;
            Transform[] bonesTns = new Transform[bones.Length];
            for (int index = 0; index < bones.Length; ++index)
                bonesTns[index] = oldNewGoMap[bones[index].gameObject].transform;
            return bonesTns;
        }
        
        // Get bones root
        Transform GetRootBone(int i)
        {
            if (skinStates[i] == false)
                return null;
            return oldNewGoMap[((SkinnedMeshRenderer)renderers[i]).rootBone.gameObject].transform;
        }

        // Copy Animation abd Animator components
        void SyncAnimation(Transform tm)
        {
            Animation[] animationArrayOld = tm.GetComponentsInChildren<Animation>();
            for (int i = 0; i < animationArrayOld.Length; ++i)
            {
                if (oldNewGoMap.ContainsKey (animationArrayOld[i].gameObject) == true)
                {
                    Animation animationOld = animationArrayOld[i];
                    Object.DestroyImmediate (oldNewGoMap[animationOld.gameObject].GetComponent<Animation>());
                    Animation animationNew = oldNewGoMap[animationArrayOld[i].gameObject].AddComponent<Animation>();
                    animationNew.clip = animationOld.clip;
                    List<float> floatList = new List<float>();
                    foreach (AnimationState animationState in animationOld)
                    {
                        floatList.Add (animationState.time);
                        animationNew.AddClip (animationState.clip, animationState.name);
                    }
                    int ind = 0;
                    foreach (AnimationState animationState in animationNew)
                    {
                        animationState.time = floatList[ind];
                        ++ind;
                    }
                    animationNew.playAutomatically = animationOld.playAutomatically;
                    animationNew.animatePhysics    = animationOld.animatePhysics;
                    animationNew.cullingType       = animationOld.cullingType;
                    animationNew.Play();
                }
            }
            Animator[] animatorArrayOld = tm.GetComponentsInChildren<Animator>();
            for (int i = 0; i < animatorArrayOld.Length; ++i)
            {
                if (oldNewGoMap.ContainsKey (animatorArrayOld[i].gameObject) == true)
                {
                    Animator animatorOld  = animatorArrayOld[i];
                    Animator animatorNew = oldNewGoMap[animatorArrayOld[i].gameObject].GetComponent<Animator>();
                    for (int t = 0; t < animatorOld.layerCount; ++t)
                    {
                        AnimatorStateInfo animatorStateInfo = animatorOld.GetCurrentAnimatorStateInfo (t);
                        animatorNew.Play (animatorStateInfo.fullPathHash, t, animatorStateInfo.normalizedTime);
                    }
                }
            }
        }

        // Check if has skinned meshes
        bool HasSkin { get {
                if (skinStates != null)
                    for (int i = 0; i < skinStates.Count; i++)
                        if (skinStates[i] == true)
                            return true;
                return false; }}
    }
}

// Static dummy class for not supported platforms
#else
namespace RayFire
{
    public class RFEngine
    {
        public List<Renderer>                     renderers;

        public static void FragmentShatter(RayfireShatter sh) {}
        public static void FragmentRigid(RayfireRigid rg) {}
        public static void InteractiveStart(RayfireShatter scr) {}
        public static void InteractiveChange(RayfireShatter scr) {}
    }
}

#endif