using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFNoise
    {
        public enum NoiseCoordType
        {
            Local  = 0, // uses local position
            Global = 1  // uses global position
        }
        
        public enum NoiseDimType
        {
            _2D = 1,
            _3D = 2,
        }

        public bool           enable;
        public NoiseCoordType coords;
        public PlaneType      axes;
        public NoiseDimType   dimension;
        public float          scale;
        public int            octaves;
        public float          persistence; // Strength of every next octave
        public float          lacunarity;  // Frequency of every next octave
        public bool           normalize;
        public float          minCap;
        public float          maxCap;
        public bool           invert;
        public bool           remove;
        public float          minThreshold;
        public float          maxThreshold;
        
        float   v1;
        float   v2;
        float   v3;
        float   amplitude;
        float   frequency;
        float   noiseHeight;
        float   sampleX;
        float   sampleY;
        float   perlinValue;
        Vector3 pos;
        
        static float v1_offset = 100;
        static float v2_offset =  90;
        static float v3_offset =  80;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        public RFNoise()
        {
            coords       = NoiseCoordType.Local;
            axes         = PlaneType.XZ;
            dimension    = NoiseDimType._2D;
            scale        = 10f;
            octaves      = 3;
            persistence  = 1f; // Strength of every next octave
            lacunarity   = 2f; 
            normalize    = true;
            minCap       = 0;
            maxCap       = 1f;
            invert       = false;
            remove       = false;
            minThreshold = 0.45f;
            maxThreshold = 0.55f;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        public float CalculateNoise(float x, float y)
        {
            amplitude   = 1;
            frequency   = 1;
            noiseHeight = 0;
            for (int i = 0; i < octaves; i++)
            {
                sampleX     = x / scale * frequency;
                sampleY     = y / scale * frequency;
                perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;
                amplitude   *= persistence;
                frequency   *= lacunarity;
            }
            return noiseHeight;
        }
        
        public float CalculateNoise(Vector3 locPos,Vector3 glbPos, PlaneType ax, NoiseCoordType cr)
        {
            // Get axes pos
            pos = cr == NoiseCoordType.Local ? locPos : glbPos;
            if (ax == PlaneType.XZ)
            {
                v1 = pos.x;
                v2 = pos.z;
                v3 = pos.y;
            }
            else if (ax == PlaneType.XY)
            {
                v1 = pos.x;
                v2 = pos.y;
                v3 = pos.z;
            }
            else
            {
                v1 = pos.y;
                v2 = pos.z;
                v3 = pos.x;
            }
            
            // Fix mirroring TODO by size
            v1 += v1_offset;
            v2 += v2_offset;
            v3 += v3_offset;
            // Get min and max bbox difference by original global pos
            
            // Dimension
            if (dimension == NoiseDimType._2D)
                v3 = 0;
            
            // Calc noise
            amplitude   = 1;
            frequency   = 1;
            noiseHeight = 0;
            for (int i = 0; i < octaves; i++)
            {
                sampleX     = (v1 + v3) / scale * frequency;
                sampleY     = (v2 + v3) / scale * frequency;
                perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;
                amplitude   *= persistence;
                frequency   *= lacunarity;
            }
            return noiseHeight;
        }
        
        public void Normalize(List<float> nseList, float nseMin, float nseMax)
        {
            if (normalize == true)
                for (int i = 0; i < nseList.Count; i++)
                    nseList[i] = Mathf.InverseLerp (nseMin, nseMax, nseList[i]);
        }

        public void Invert(List<float> nseList, float nseMin, float nseMax)
        {
            if (invert == true)
            {
                if (normalize == true)
                    for (int i = 0; i < nseList.Count; i++)
                        nseList[i] = 1 - nseList[i];
                else
                    for (int i = 0; i < nseList.Count; i++)
                        nseList[i] = nseMax - nseList[i] + nseMin;
            }
        }

        public void CapNoise(List<float> nseList, float nseMin, float nseMax)
        {
            float min    = minCap;
            float max    = maxCap;
            if (normalize == false)
            {
                min = (nseMax - nseMin) * minCap + nseMin;
                max = (nseMax - nseMin) * maxCap + nseMin;
            }
            if (minCap > 0)
                for (int i = 0; i < nseList.Count; i++)
                    if (nseList[i] < min)
                        nseList[i] = min;
            if (maxCap < 1f)
                for (int i = 0; i < nseList.Count; i++)
                    if (nseList[i] > max)
                        nseList[i] = max;
        }

        public void ResetProperties()
        {
            coords       = NoiseCoordType.Local;
            axes         = PlaneType.XZ;
            scale        = 10f;
            octaves      = 4;
            persistence  = 1f; // Strength of every next octave
            lacunarity   = 2f; 
            normalize    = true;
            invert       = false;
            remove       = false;
            minThreshold = 0.45f;
            maxThreshold = 0.55f;
        }
    }
}