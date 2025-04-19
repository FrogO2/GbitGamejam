using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
	/// <summary>
    /// Advanced Properties class for Rayfire Shatter component.
    /// </summary>
	[Serializable]
	public class RFShatterAdvanced
	{
		// Common
		public int   seed;
		public bool  dec;
		public bool  coll;
		public bool  copy;
		public bool  inpCap;
        public bool  outCap;
        public bool  dbl;
        public int   element;
        public bool  children;
        public bool  smooth;
        public bool  weld;
        public bool  inner;
        public bool  planar;
        public int   relSze;
        public float absSze;
        
        // V2
        public FragHierarchyType hierarchy;
        public bool              separate;
        public bool              combine;
        public SliceType         sliceType;
        public bool              origScale;
        public int               faceFlt;
        public bool              centerSet;
        public Transform         centerBias;
        public bool              aabbEnable;
        public bool              aabbSeparate;
        public Transform         aabbObject;
        
        // Limitations
        public bool  szeLim;
        public float szeAm;
        public bool  vrtLim;
        public int   vrtAm;
        public bool  triLim;
        public int   triAm;
        
        // Static
        public static float     planarThreshold = 0.01f;
        public static Vector3[] vertices;
        public static Plane     plane;
        public static float     minSize = 0.01f;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

		public RFShatterAdvanced()
		{
			seed     = 1;
			copy     = false;
			smooth   = false;
			children = false;
			coll     = false;
			dec      = true;
			inpCap   = true;
			outCap   = false;
			dbl      = true; 
			element  = 5;
			inner    = false;
			planar   = false;
			relSze   = 4;
			absSze   = 0.1f;
			weld     = false;
			szeLim   = false;
			szeAm    = 5f;
			vrtLim   = false;
			vrtAm    = 300;
			triLim   = false;
			triAm    = 300;
			
			// V2
			hierarchy    = FragHierarchyType.Copy;
			separate     = true;
	        combine      = true;
	        sliceType    = SliceType.Hybrid;
	        origScale    = false;
	        faceFlt      = 0;
	        centerSet    = false;
	        centerBias   = null;
	        aabbEnable   = false;
	        aabbSeparate = false;
	        aabbObject   = null;
		}
        
        public RFShatterAdvanced (RFShatterAdvanced src)
        {
	        seed     = src.seed;
	        copy     = src.copy;
	        smooth   = src.smooth;
	        children = src.children;
	        coll     = src.coll;
	        dec      = src.dec;
	        inpCap   = src.inpCap;
	        outCap   = src.outCap;
	        dbl      = src.dbl;
	        element  = src.element;
	        inner    = src.inner;
	        planar   = src.planar;
	        relSze   = src.relSze;
	        absSze   = src.absSze;
	        weld     = src.weld;
	        szeLim   = src.szeLim;
	        szeAm    = src.szeAm;
	        vrtLim   = src.vrtLim;
	        vrtAm    = src.vrtAm;
	        triLim   = src.triLim;
	        triAm    = src.triAm;
	        
	        // V2
	        hierarchy    = src.hierarchy;
	        separate     = src.separate;
	        combine      = src.combine;
	        sliceType    = src.sliceType;
	        origScale    = src.origScale;
	        faceFlt      = src.faceFlt;
	        centerSet    = src.centerSet;
	        centerBias   = src.centerBias;
	        aabbEnable   = src.aabbEnable;
	        aabbSeparate = src.aabbSeparate;
	        aabbObject   = src.aabbObject;
        }
        
        public static void Copy(RFShatterAdvanced trg, RFShatterAdvanced src)
        {
	        trg.seed     = src.seed;
	        trg.copy     = src.copy;
	        trg.smooth   = src.smooth;
	        trg.children = src.children;
	        trg.coll     = src.coll;
	        trg.dec      = src.dec;
	        trg.inpCap   = src.inpCap;
	        trg.outCap   = src.outCap;
	        trg.dbl      = src.dbl;
	        trg.element  = src.element;
	        trg.inner    = src.inner;
	        trg.planar   = src.planar;
	        trg.relSze   = src.relSze;
	        trg.absSze   = src.absSze;
	        trg.weld     = src.weld;
	        trg.szeLim   = src.szeLim;
	        trg.szeAm    = src.szeAm;
	        trg.vrtLim   = src.vrtLim;
	        trg.vrtAm    = src.vrtAm;
	        trg.triLim   = src.triLim;
	        trg.triAm    = src.triAm;
	        
	        // V2
	        trg.hierarchy    = src.hierarchy;
	        trg.separate     = src.separate;
	        trg.combine      = src.combine;
	        trg.sliceType    = src.sliceType;
	        trg.origScale    = src.origScale;
	        trg.faceFlt      = src.faceFlt;
	        trg.centerSet    = src.centerSet;
	        trg.centerBias   = src.centerBias;
	        trg.aabbEnable   = src.aabbEnable;
	        trg.aabbSeparate = src.aabbSeparate;
	        trg.aabbObject   = src.aabbObject;
        }

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if mesh is coplanar. All verts on a plane
        public static bool IsCoplanar(Mesh mesh, float threshold)
        {
	        // Coplanar 3 verts
            if (mesh.vertexCount <= 3)
                return true;

            // Get first plane vertex index
            int index1 = 0;
            int index2 = 0;
            int index3 = 0;

            // Set array of vertices
            vertices = mesh.vertices;
            
            // Get second plane vertex index
            int ind = 1;
            for (int i = ind; i < vertices.Length; i++)
            {
	            if (Vector3.Distance (vertices[index1], vertices[i]) > threshold)
	            {
		            index2 = i;
		            ind = i;
		            break;
	            }
            }
            
            // No second vert
            if (index2 == 0)
                return true;

            // Second vert is the last ver
            if (ind == vertices.Length - 1)
                return true;
            
            // Get third vert
            ind++;
            float   distance;
            Vector3 vector2;
            Vector3 vector1 = (vertices[index2] - vertices[index1]).normalized;
            for (int i = ind; i < vertices.Length; i++)
            {
                if (Vector3.Distance (vertices[index1], vertices[i]) > threshold)
                {
                    vector2  = (vertices[i] - vertices[index1]).normalized;
                    distance = Vector3.Cross (vector1, vector2).magnitude;
                    if (distance > threshold)
                    {
	                    index3 = i;
                        break;
                    }
                }
            }
            
            // No third vert
            if (index3 == 0)
                return true;
            
            // Create plane and check other verts for coplanar
            plane = new Plane(vertices[index1], vertices[index2], vertices[index3]);
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i != index1 && i != index2 && i != index3)
                {
	                distance = plane.GetDistanceToPoint (vertices[i]);
                    if (Math.Abs (distance) > threshold)
                        return false;
                }
            }
            
            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////

        // Filter out planar meshes
        public static void RemovePlanar(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] origSubMeshIdsRf, RayfireShatter scrShatter)
        {
	        if (scrShatter.advanced.planar == true)
	        {
		        List<Mesh>         newMeshes = new List<Mesh>();
		        List<Vector3>      newPivots = new List<Vector3>();
		        List<RFDictionary> newIds    = new List<RFDictionary>();
		        for (int i = 0; i < meshes.Length; i++)
		        {
			        if (IsCoplanar (meshes[i], planarThreshold) == false)
			        {
				        newMeshes.Add (meshes[i]);
				        newPivots.Add (pivots[i]);
				        newIds.Add (origSubMeshIdsRf[i]);
			        }
		        }
		        if (newMeshes.Count > 0)
		        {
			        meshes           = newMeshes.ToArray();
			        pivots           = newPivots.ToArray();
			        origSubMeshIdsRf = newIds.ToArray();
		        }
	        }
        }
        
        // Filter out meshes by size
        public static void RemoveBySize(ref Mesh[] meshes, ref Vector3[] pivots, ref RFDictionary[] origSubMeshIdsRf, RayfireShatter scr)
        {
	        if (scr.advanced.absSze > 0 || scr.advanced.relSze > 0)
	        {
		        List<Mesh>         newMeshes = new List<Mesh>();
		        List<Vector3>      newPivots = new List<Vector3>();
		        List<RFDictionary> newIds    = new List<RFDictionary>();

		        // Size
		        float size = scr.advanced.relSze / 100f;
		        if (scr.meshRenderer != null)
			        size *= scr.meshRenderer.bounds.size.magnitude;
		        if (scr.skinnedMeshRend != null)
			        size *= scr.skinnedMeshRend.bounds.size.magnitude;

		        // Filter
		        for (int i = 0; i < meshes.Length; i++)
		        {
			        if (scr.advanced.absSze > 0)
				        if (meshes[i].bounds.size.magnitude > scr.advanced.absSze)
				        {
					        newMeshes.Add (meshes[i]);
					        newPivots.Add (pivots[i]);
					        newIds.Add (origSubMeshIdsRf[i]);
					        continue;
				        }
			        if (scr.advanced.relSze > 0)
				        if (meshes[i].bounds.size.magnitude > size)
				        {
					        newMeshes.Add (meshes[i]);
					        newPivots.Add (pivots[i]);
					        newIds.Add (origSubMeshIdsRf[i]);
				        }
		        }

		        if (newMeshes.Count > 0)
		        {
			        meshes           = newMeshes.ToArray();
			        pivots           = newPivots.ToArray();
			        origSubMeshIdsRf = newIds.ToArray();
		        }
	        }
        }
        
		/// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////
		
		// Limitations ops
		public static void Limitations(RayfireShatter sh)
		{
			SizeLimitation(sh);
			SizeLimitation(sh);
			SizeLimitation(sh);
			VertexLimitation(sh);
			VertexLimitation(sh);
			VertexLimitation(sh);
			TriangleLimitation(sh);
			TriangleLimitation(sh);
			TriangleLimitation(sh); 
		}
		
        // Size limitation
        static void SizeLimitation(RayfireShatter sh)
		{
			if (sh.advanced.szeLim != true)
				return;

			// Last batch
			RFShatterBatch batch = sh.batches[sh.batches.Count - 1];
			for (int i = batch.fragments.Count - 1; i >= 0; i--)
			{
				MeshRenderer mr = batch.fragments[i].GetComponent<MeshRenderer>();
				if (mr.bounds.size.magnitude > sh.advanced.szeAm)
					sh.LimitationFragment (batch, i);
			}
		}
        
        // Vertex limitation
        static void VertexLimitation(RayfireShatter sh)
        {
	        if (sh.advanced.vrtLim != true)
		        return;
	        
	        // Last batch
	        RFShatterBatch batch = sh.batches[sh.batches.Count - 1];
	        for (int i = batch.fragments.Count - 1; i >= 0; i--)
	        {
		        MeshFilter mf = batch.fragments[i].GetComponent<MeshFilter>();
		        if (mf.sharedMesh.vertexCount > sh.advanced.vrtAm)
			        sh.LimitationFragment (batch, i);
	        }
        }
        
        // Triangle limitation
        static void TriangleLimitation(RayfireShatter sh)
        {
	        if (sh.advanced.triLim != true)
		        return;
	        
	        // Last batch
	        RFShatterBatch batch = sh.batches[sh.batches.Count - 1];
	        for (int i = batch.fragments.Count - 1; i >= 0; i--)
	        {
		        MeshFilter mf = batch.fragments[i].GetComponent<MeshFilter>();
		        if (mf.sharedMesh.triangles.Length / 3 > sh.advanced.triAm)
			        sh.LimitationFragment (batch, i);
	        }
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////

		// Get seed
        public int Seed { get {
		        if (seed == 0)
			       return Random.Range (0, 100);
		        return seed;
        }}
        
	}
}