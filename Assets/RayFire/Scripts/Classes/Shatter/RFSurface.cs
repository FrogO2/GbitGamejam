using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [System.Serializable]
    public class RFSurface
    {
        [FormerlySerializedAs ("innerMaterial")] public Material iMat;
        [FormerlySerializedAs ("outerMaterial")] public Material oMat;
        [FormerlySerializedAs ("mappingScale")]  public float    mScl;
        public                                          bool     uvE;
        public                                          Vector2  uvC;
        public                                          Vector2  uvR;
        public                                          bool     cE;
        public                                          Color    cC;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
         
        // Constructor
        public RFSurface()
        {
            InitValues();
        }

        void InitValues()
        {
            iMat = null;
            oMat = null;
            mScl = 0.5f;
            uvE  = false;
            uvC  = new Vector2 (0.25f, 0.25f);
            uvR  = new Vector2 (0.75f, 0.75f);
            cE   = false;
        }

        public RFSurface(RFSurface src)
        {
            iMat = src.iMat;
            oMat = src.oMat;
            mScl = src.mScl;
            uvE  = src.uvE;
            uvC  = src.uvC;
            uvR  = src.uvR;
            cE   = src.cE;
            cC   = src.cC;
        }
        
        // Pool Reset
        public void GlobalReset()
        {
            InitValues();
        }
        
        // Copy from
        public static void Copy(RFSurface trg, RFSurface src)
        {
            trg.iMat = src.iMat;
            trg.oMat = src.oMat;
            trg.mScl = src.mScl;
            trg.uvE  = src.uvE;
            trg.uvC  = src.uvC;
            trg.uvR  = src.uvR;
            trg.cE   = src.cE;
            trg.cC   = src.cC;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Set material to fragment by it's interior properties and parent material
        public static void SetMaterial(RFDictionary[] origSubMeshIdsRF, Material[] sharedMaterials, RFSurface interior, MeshRenderer targetRend, int i, int amount)
        {
            if (origSubMeshIdsRF != null && origSubMeshIdsRF.Length == amount)
            {
                Material[] mats = new Material[origSubMeshIdsRF[i].values.Length];
                for (int j = 0; j < origSubMeshIdsRF[i].values.Length; j++)
                {
                    int matId = origSubMeshIdsRF[i].values[j];
                    if (matId < sharedMaterials.Length)
                    {
                        if (interior.oMat == null)
                            mats[j] = sharedMaterials[matId];
                        else
                            mats[j] = interior.oMat;
                    }
                    else
                        mats[j] = interior.iMat;
                }

                targetRend.sharedMaterials = mats;
            }
        }
        
        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireRigid scr)
        {
            // No inner material
            if (scr.materials.iMat == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skr != null 
                ? scr.skr.sharedMaterials 
                : scr.mRnd.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.materials.iMat)
                    return i;
            
            return -1;
        }
        
        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireShatter scr)
        {
            // No inner material
            if (scr.material.iMat == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skinnedMeshRend != null 
                ? scr.skinnedMeshRend.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.material.iMat)
                    return i;
            
            return -1;
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        public Vector2 UvRegionMin { get {
                if (uvE == true)
                {
                    if (uvC.x < 0)
                        uvC.x = 0;
                    if (uvC.y < 0)
                        uvC.y = 0;
                    if (uvC.x > 1)
                        uvC.x = 1;
                    if (uvC.y > 1)
                        uvC.y = 1;


                    return uvC;
                }
                return Vector2.zero; 
        }}
        
        public Vector2 UvRegionMax { get {
                if (uvE == true)
                {
                    if (uvR.x < 0)
                        uvR.x = 0;
                    if (uvR.y < 0)
                        uvR.y = 0;
                    if (uvR.x > 1)
                        uvR.x = 1;
                    if (uvR.y > 1)
                        uvR.y = 1;
                    

                    
                    return uvR;
                }
                return Vector2.one; 
        }}
        
        public float MappingScale { get {
            if (uvE == true)
                return 1f;
            return mScl; 
        }}
        
        public Color Color { get {
            if (cE == true)
                return cC;
            return new Color (-1,-1,-1); 
        }}
        
    }
}

