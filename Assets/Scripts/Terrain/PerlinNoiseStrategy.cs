using Interfaces;
using UnityEngine;
using System;

namespace Terrain
{
    public class PerlinNoiseStrategy : IGenerator<float[,]>
    {
        private int mapWidth;
        private int mapHeight;
        private float scale;

        public PerlinNoiseStrategy(int mapWidth, int mapHeight, float scale)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.scale = scale;

            // To fail safely
            if (scale <= 0)
            {
                scale = 0.0001f; // randomly chosen small value
            }
        }

        public float[,] Generate()
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sampleX = x / scale;
                    float sampleY = y / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseMap[x, y] = perlinValue;
                }
            }
            return noiseMap;
        }
    }   
}
