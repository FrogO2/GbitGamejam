using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    /// <summary>
    /// Rayfire dictionary class.
    /// </summary>
    [Serializable]
    public class RFDictionary
    {
        public int[] keys;
        public int[] values;

        // Constructor
        public RFDictionary(Dictionary<int, int> dictionary)
        {
            keys = dictionary.Keys.ToArray();
            values =  dictionary.Values.ToArray();
        }

        // Get RayFire dictionary array by original sub mesh ids list
        public static RFDictionary[] GetRFDictionary (List<Dictionary<int, int>> origSubMeshIds)
        {
            RFDictionary[] origSubMeshIdsRf = new RFDictionary[origSubMeshIds.Count];
            for (int i = 0; i < origSubMeshIds.Count; i++)
                origSubMeshIdsRf[i] = new RFDictionary(origSubMeshIds[i]);
            return origSubMeshIdsRf;
        }
        
        // Get dictionary by RFDictionary
        public static Dictionary<int, int> GetDictionary(RFDictionary rfDict)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < rfDict.keys.Length; i++)
                dict.Add (rfDict.keys[i], rfDict.values[i]);
            return dict;
        }
        
        // Get dictionary by RFDictionary
        public static List<Dictionary<int, int>> GetDictionary (RFDictionary[] rfDictionary)
        {
            List<Dictionary<int, int>> dict = new List<Dictionary<int, int>>();
            for (int i = 0; i < rfDictionary.Length; i++)
                dict.Add (GetDictionary (rfDictionary[i]));
            return dict;
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Fragments Clustering
    /// /////////////////////////////////////////////////////////
    
    /// <summary>
    /// Rayfire Shatter pos fragmentation cluster class.
    /// </summary>
    [Serializable]
    public class RFShatterCluster
    {
        public bool  enable;
        public int   count;
        public int   seed;
        public float relax;
        public int   amount;
        public int   layers;
        public float scale;
        public int   min;
        public int   max;

        public RFShatterCluster()
        {
            enable = false;
            count  = 10;
            seed   = 1;
            relax  = 0.5f;
            layers = 0;
            amount = 0;
            scale  = 1f;
            min    = 1;
            max    = 3;
        }
        
        public RFShatterCluster (RFShatterCluster src)
        {
            enable = src.enable;
            count  = src.count;
            seed   = src.seed;
            relax  = src.relax;
            layers = src.layers;
            amount = src.amount;
            scale  = src.scale;
            min    = src.min;
            max    = src.max;
        }
        
        public static void Copy (RFShatterCluster trg, RFShatterCluster src)
        {
            trg.enable = src.enable;
            trg.count  = src.count;
            trg.seed   = src.seed;
            trg.relax  = src.relax;
            trg.layers = src.layers;
            trg.amount = src.amount;
            trg.scale  = src.scale;
            trg.min    = src.min;
            trg.max    = src.max;
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Shatter
    /// /////////////////////////////////////////////////////////

    /// <summary>
    /// Rayfire Shatter voronoi fragmentation class.
    /// </summary>
    [Serializable]
    public class RFVoronoi
    {
        public int amount;
        public float centerBias;
        
        public RFVoronoi()
        {
            amount = 30;
            centerBias = 0f;
        }
        
        public RFVoronoi(RFVoronoi src)
        {
            amount     = src.amount;
            centerBias = src.centerBias;
        }
        
        // Amount
        public int Amount
        {
            get
            {
                if (amount < 2)
                    return 2;
                if (amount > 20000)
                    return 2;
                return amount;
            }
        }
    }

    /// <summary>
    /// Rayfire Shatter splinters fragmentation class.
    /// </summary>
    [Serializable]
    public class RFSplinters
    {
        public AxisType axis;
        public int      amount;
        public float    strength;
        public float    centerBias;
        
        public RFSplinters()
        {
            axis       = AxisType.YGreen;
            amount     = 30;
            strength   = 0.7f;
            centerBias = 0f;
        }
        
        public RFSplinters(RFSplinters src)
        {
            axis       = src.axis; 
            amount     = src.amount;
            strength   = src.strength;
            centerBias = src.centerBias;
        }
        
        // Amount
        public int Amount { get {
            if (amount < 2)
                return 2;
            if (amount > 20000)
                return 2;
            return amount;
        }}
    }

    /// <summary>
    /// Rayfire Shatter radial fragmentation class.
    /// </summary>
    [Serializable]
    public class RFRadial
    {
        public AxisType centerAxis;
        public float    radius;
        public float    divergence;
        public bool     restrictToPlane;
        public int      rings;
        public int      focus;
        public int      focusStr;
        public int      randomRings;
        public int      rays;
        public int      randomRays;
        public int      twist;
        
        public RFRadial()
        {
            centerAxis  = AxisType.YGreen;
            radius          = 1f;
            divergence      = 1f;
            restrictToPlane = true;
            rings           = 10;
            focus           = 0;
            focusStr        = 50;
            randomRings     = 50;
            rays            = 10;
            randomRays      = 0;
            twist           = 0;
        }
        
        public RFRadial(RFRadial src)
        {
            centerAxis      = src.centerAxis;
            radius          = src.radius;
            divergence      = src.divergence;
            restrictToPlane = src.restrictToPlane;
            rings           = src.rings;
            focus           = src.focus;
            focusStr        = src.focusStr;
            randomRings     = src.randomRings;
            rays            = src.rays;
            randomRays      = src.randomRays;
            twist           = src.twist;
        }
    }
    
    /// <summary>
    /// Rayfire Shatter slice class.
    /// </summary>
    [Serializable]
    public class RFSlice
    {
        public PlaneType       plane;
        public List<Transform> sliceList;
        
        public RFSlice()
        {
            plane = PlaneType.XZ;
        }
        
        public RFSlice(RFSlice src)
        {
            plane     = src.plane;
            sliceList = src.sliceList;
        }
        
        // Get axis
        public Vector3 Axis (Transform tm)
        {
            if (plane == PlaneType.YZ)
                return tm.right;
            if (plane == PlaneType.XZ)
                return tm.up;
            return tm.forward;
        }
        
        // Set slicing for V2 engine
        public static void SetSliceData(RFSlice slices, Transform tm, ref Vector3[] pos, ref Vector3[] norm)
        {
            // Vars 
            List<Transform> list = new List<Transform>();
            
            // Collect slice transforms
            for (int i = 0; i < slices.sliceList.Count; i++)
                if (slices.sliceList[i] != null)
                    list.Add (slices.sliceList[i]);

            // No objects. Use default center
            if (list.Count == 0)
            {
                pos = new[] {tm.position};
                if (slices.plane == PlaneType.XY) norm       = new[] {Vector3.up};
                else  if (slices.plane == PlaneType.YZ) norm = new[] {Vector3.right};
                else norm                                    = new[] {Vector3.forward};
            }

            // Get slice data
            else
            {
                pos  = list.Select (t => t.position).ToArray();
                norm = list.Select (slices.Axis).ToArray();
            }
        }
    }
    
    /// <summary>
    /// Rayfire Shatter bricks fragmentation class.
    /// </summary>
    [Serializable]
    public class RFBricks
    {
        public enum RFBrickType
        {
            ByAmount = 0,
            BySize = 1
        }

        public RFBrickType amountType;
        public float       mult;
        public int         amount_X;
        public int         amount_Y;
        public int         amount_Z;
        public bool        size_Lock;
        public float       size_X;
        public float       size_Y;
        public float       size_Z;
        public int         sizeVar_X;
        public int         sizeVar_Y;
        public int         sizeVar_Z;
        public float       offset_X;
        public float       offset_Y;
        public float       offset_Z;
        public bool        split_X;
        public bool        split_Y;
        public bool        split_Z;
        public int         split_probability;
        public float       split_offset;
        public int         split_rotation;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        public RFBricks()
        {
            amountType     = RFBrickType.ByAmount;
            mult           = 1f;
            amount_X       = 3;
            amount_Y       = 6;
            amount_Z       = 0;
            size_X         = 0.4f;
            size_Y         = 0.2f;
            size_Z         = 2f;
            offset_X       = 0.5f;
            offset_Y       = 0.5f;
            offset_Z       = 0;
            split_offset   = 0.5f;
            split_rotation = 30;
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        public Vector3 Size { get {
            if (amountType == RFBrickType.BySize)
            {
                if (size_X < 0.001f) size_X = 0.001f;
                if (size_Y < 0.001f) size_Y = 0.001f;
                if (size_Z < 0.001f) size_Z = 0.001f;
                if (mult < 0.1f)     mult   = 0.1f;
                return new Vector3 (size_X, size_Y, size_Z) * mult;
            }
            return Vector3.zero;
        }}
        
        public Vector3Int Num { get {
            if (amountType == RFBrickType.ByAmount)
            {
                float X = amount_X * mult;
                if (X <= 0) X = 1;
                float Y = amount_Y * mult;
                if (Y <= 0) Y = 1;
                float Z = amount_Z * mult;
                if (Z <= 0) Z = 1;
                return new Vector3Int ((int)X, (int)Y, (int)Z);
            }
            return new Vector3Int (0,0,0);
        }}
        
        public Vector3    SizeVariation { get { return new Vector3(sizeVar_X, sizeVar_Y, offset_Z) * 0.01f; } }
        public Vector3    SizeOffset    { get { return new Vector3(offset_X, offset_Y, offset_Z); } }
        public Vector3Int SplitState    { get { return new Vector3Int(BoolToInt(split_X), BoolToInt(split_Y), BoolToInt(split_Z)); } }
        public Vector3    SplitPro      { get { return new Vector3(split_probability * 0.01f, (float)split_rotation, split_offset); } }
        
        int BoolToInt(bool state) { return state == true ? 1 : 0; }
    }

    /// <summary>
    /// Rayfire Shatter voxels fragmentation class.
    /// </summary>
    [Serializable]
    public class RFVoxels
    {
        public float size;

        public RFVoxels()
        {
            size = 1f;
        }
        
        public Vector3 Size { get
        {
            if (size < 0.001f)
                size = 0.001f;
            return new Vector3 (size, size, size);
        }}
        
        public Vector3Int SplitState { get { return new Vector3Int(0, 0, 0); } }
    }
    
    /// <summary>
    /// Rayfire Shatter tets fragmentation class.
    /// </summary>
    [Serializable]
    public class RFTets
    {
        public enum TetType
        {
            Uniform = 0,
            Curved  = 1
        }
        
        public TetType lattice;
        public int     density;
        public int     noise;
        
        public RFTets()
        {
            lattice = TetType.Uniform;
            density = 7;
            noise   = 100;
        }
        
        public RFTets(RFTets src)
        {
            lattice = src.lattice;
            density = src.density;
            noise   = src.noise;
        }
    }
}

