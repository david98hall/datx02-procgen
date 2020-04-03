using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave 
{
    public float seed;
    public float frequency;
    public float amplitude;
}
public class NoiseMapGenerator : MonoBehaviour
{
    ///<summary>
    ///Generate noise value with Mathf.PerlinNoise. 
    /// function recives two parameters and generates a value between 0 and 1
    ///</summary>
    public float [,] GenerateNoiseMap(int mapDepth,int mapWidth, float scale, float offsetX, float offsetZ, Wave[] waves)
    {
        float[,] nosieMap = new float[mapDepth,mapWidth];
        
        for(int zIndex = 0; zIndex <mapDepth; zIndex++)
        {
            for(int xIndex = 0; xIndex<mapWidth; xIndex++)
            {
                float sampleX = (xIndex + offsetX) /scale;
                float sampleZ = (zIndex + offsetZ) /scale;

                float noise =0f;
                float normalization = 0f;

                foreach(Wave wave in waves)
                {
                    // generate noise value using PerlinNoise for a given Wave
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
                    normalization += wave.amplitude;

                }
                // normalize the noise value so that it is within 0 and 1
                noise /= normalization;
                nosieMap [zIndex,xIndex]= noise;
            }
        }
        return nosieMap;
    }

    ///<summary>
    ////Calculating the noise proportional to the distance to the center of the Level.
    ///</summary>
    public float[,] GenerateUniformNoiseMap(int mapDepth, int mapWidth, float centerVertexZ, float maxDistanceZ, float offsetZ)
    {  
        float [,] nosieMap = new float [mapDepth,mapWidth];

        for(int zIndex =0 ; zIndex < mapDepth; zIndex++)
        {
            float sampleZ = zIndex + offsetZ;
            float noise = Mathf.Abs(sampleZ - centerVertexZ) / maxDistanceZ;

            for(int xIndex = 0 ; xIndex < mapWidth; xIndex++)
            {
                nosieMap[mapDepth - zIndex -1 , xIndex] = noise;
            }

        }
        return nosieMap;
    }
}
