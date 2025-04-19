using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    /// <summary>
    /// Rayfire Shatter batch class.
    /// </summary>
    [Serializable]
    public class RFShatterBatch
    {
        public string          name;
        public bool            preview;
        public bool            exported;
        public Transform       sourceTm;
        public Transform       fragRoot;
        public List<Transform> fragments;

        // Preview Target scale/color
        // Fragment to target location

        public List<int>   dataInt;
        public List<float> dataFlt;
        public List<float> dataFrg;

        public RFSurface         material;
        public RFShatterCluster  cluster;
        public RFShatterAdvanced advanced;
        public RFShell           shell;
        
        // Constructor
        public RFShatterBatch(Transform SourceTm, Transform FragRoot)
        {
            sourceTm  = SourceTm;
            fragRoot  = FragRoot;
            fragments = new List<Transform>();
        }

        public void SaveData(RayfireShatter scr)
        {
            material = new RFSurface(scr.material);
            cluster  = new RFShatterCluster (scr.clusters);
            advanced = new RFShatterAdvanced (scr.advanced);
            shell    = new RFShell (scr.shell);
            
            dataInt = new List<int>{
                (int)scr.engine,
                (int)scr.type,
                (int)scr.mode
            };
            
            dataFlt = new List<float> {

            };
            
            dataFrg = new List<float>();
            if (scr.type == FragType.Voronoi)
            {
                dataFrg.Add (scr.voronoi.amount);
                dataFrg.Add (scr.voronoi.centerBias);
            }
            else if (scr.type == FragType.Splinters)
            {
                dataFrg.Add ((float)scr.splinters.axis);
                dataFrg.Add (scr.splinters.amount);
                dataFrg.Add (scr.splinters.strength);
                dataFrg.Add (scr.splinters.centerBias);
            }
            else if (scr.type == FragType.Slabs)
            {
                dataFrg.Add ((float)scr.slabs.axis);
                dataFrg.Add (scr.slabs.amount);
                dataFrg.Add (scr.slabs.strength);
                dataFrg.Add (scr.slabs.centerBias);
            }
            else if (scr.type == FragType.Radial)
            {
                dataFrg.Add ((float)scr.radial.centerAxis);
                dataFrg.Add (scr.radial.radius);
                dataFrg.Add (scr.radial.divergence);
                dataFrg.Add (BoolToFloat(scr.radial.restrictToPlane));
                dataFrg.Add (scr.radial.rings);
                dataFrg.Add (scr.radial.focus);
                dataFrg.Add (scr.radial.focusStr);
                dataFrg.Add (scr.radial.randomRings);
                dataFrg.Add (scr.radial.rays);
                dataFrg.Add (scr.radial.randomRays);
                dataFrg.Add (scr.radial.twist);
            }
            else if (scr.type == FragType.Hexagon)
            {
                
            }
            else if (scr.type == FragType.Custom)
            {
                
            }
            else if (scr.type == FragType.Slices)
            {
                
            }
            else if (scr.type == FragType.Bricks)
            {
                dataFrg.Add ((float)scr.bricks.amountType);
                dataFrg.Add (scr.bricks.mult);
                dataFrg.Add (scr.bricks.amount_X);
                dataFrg.Add (scr.bricks.amount_Y);
                dataFrg.Add (scr.bricks.amount_Z);
                dataFrg.Add (BoolToFloat(scr.bricks.size_Lock));
                dataFrg.Add (scr.bricks.size_X);
                dataFrg.Add (scr.bricks.size_Y);
                dataFrg.Add (scr.bricks.size_Z);
                dataFrg.Add (scr.bricks.sizeVar_X);
                dataFrg.Add (scr.bricks.sizeVar_Y);
                dataFrg.Add (scr.bricks.sizeVar_Z);
                dataFrg.Add (scr.bricks.offset_X);
                dataFrg.Add (scr.bricks.offset_Y);
                dataFrg.Add (scr.bricks.offset_Z);
                dataFrg.Add (BoolToFloat(scr.bricks.split_X));
                dataFrg.Add (BoolToFloat(scr.bricks.split_Y));
                dataFrg.Add (BoolToFloat(scr.bricks.split_Z));
                dataFrg.Add (scr.bricks.split_probability);
                dataFrg.Add (scr.bricks.split_offset);
                dataFrg.Add (scr.bricks.split_rotation);
            }
            else if (scr.type == FragType.Voxels)
            {
                dataFrg.Add (scr.voxels.size);
            }

            // TODO fragments
        }

        public void LoadData(RayfireShatter scr)
        {
            RFSurface.Copy (scr.material, material);
            RFShatterCluster.Copy (scr.clusters, cluster);
            RFShatterAdvanced.Copy (scr.advanced, advanced);
            RFShell.Copy (scr.shell, shell);
            
            // dataInt
            scr.engine = (RayfireShatter.RFEngineType)dataInt[0];
            scr.type   = (FragType)dataInt[1];
            scr.mode   = (FragmentMode)dataInt[2];
            
            // dataFrg
            if (scr.type == FragType.Voronoi)
            {
                scr.voronoi.amount     = (int)dataFrg[0];
                scr.voronoi.centerBias = dataFrg[1];
            } 
            else if (scr.type == FragType.Splinters)
            {
                scr.splinters.axis       = (AxisType)dataFrg[0];
                scr.splinters.amount     = (int)dataFrg[1];
                scr.splinters.strength   = dataFrg[2];
                scr.splinters.centerBias = dataFrg[3];
            }
            else if (scr.type == FragType.Slabs)
            {
                scr.slabs.axis       = (AxisType)dataFrg[0];
                scr.slabs.amount     = (int)dataFrg[1];
                scr.slabs.strength   = dataFrg[2];
                scr.slabs.centerBias = dataFrg[3];
            }
            else if (scr.type == FragType.Radial)
            {
                scr.radial.centerAxis      = (AxisType)dataFrg[0];
                scr.radial.radius          = dataFrg[1];
                scr.radial.divergence      = dataFrg[2];
                scr.radial.restrictToPlane = FloatToBool(dataFrg[3]);
                scr.radial.rings           = (int)dataFrg[4];
                scr.radial.focus           = (int)dataFrg[5];
                scr.radial.focusStr        = (int)dataFrg[6];
                scr.radial.randomRings     = (int)dataFrg[7];
                scr.radial.rays            = (int)dataFrg[8];
                scr.radial.randomRays      = (int)dataFrg[9];
                scr.radial.twist           = (int)dataFrg[10];
            }
            else if (scr.type == FragType.Bricks)
            {
                scr.bricks.amountType        = (RFBricks.RFBrickType)dataFrg[0];
                scr.bricks.mult              = dataFrg[1];
                scr.bricks.amount_X          = (int)dataFrg[2];
                scr.bricks.amount_Y          = (int)dataFrg[3];
                scr.bricks.amount_Z          = (int)dataFrg[4];
                scr.bricks.size_Lock         = FloatToBool(dataFrg[5]);
                scr.bricks.size_X            = dataFrg[6];
                scr.bricks.size_Y            = dataFrg[7];
                scr.bricks.size_Z            = dataFrg[8];
                scr.bricks.sizeVar_X         = (int)dataFrg[9];
                scr.bricks.sizeVar_Y         = (int)dataFrg[10];
                scr.bricks.sizeVar_Z         = (int)dataFrg[11];
                scr.bricks.offset_X          = dataFrg[12];
                scr.bricks.offset_Y          = dataFrg[13];
                scr.bricks.offset_Z          = dataFrg[14];
                scr.bricks.split_X           = FloatToBool(dataFrg[15]);
                scr.bricks.split_Y           = FloatToBool(dataFrg[16]);
                scr.bricks.split_Z           = FloatToBool(dataFrg[17]);
                scr.bricks.split_probability = (int)dataFrg[18];
                scr.bricks.split_offset      = dataFrg[19];
                scr.bricks.split_rotation    = (int)dataFrg[20];
            }
            else if (scr.type == FragType.Voxels)
            {
                scr.voxels.size = dataFrg[0];
            }
        }

        float BoolToFloat(bool state)
        {
            if (state == true)
                return 1f;
            return 0f;
        }
        
        bool FloatToBool(float val)
        {
            if (val == 1f)
                return true;
            return false;
        }
        
        bool IntToBool(int val)
        {
            if (val == 1)
                return true;
            return false;
        }

        int BoolToInt(bool state)
        {
            if (state == true)
                return 1;
            return 0;
        }
        
        public bool HasFragments
        {
            get
            {
                if (fragments == null || fragments.Count == 0)
                    return false;
                return true;
            }
        }
    }
}

