using Interfaces;
using UnityEngine;

namespace Terrain
{
    internal class PerlinNoiseStrategy : IGenerator<float[,]>
    {
        private int width;
        private int height;
        private int seed;
        private float scale;

        private int octaves;
        private float persistence;
        private float lacunarity;
        private Vector2 noiseOffset;

        public PerlinNoiseStrategy(int width, int height, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 noiseOffset)
        {
            this.width = width;
            this.height = height;
            this.seed = seed;

            this.scale = scale;
            // To fail safely
            if (scale <= 0)
            {
                scale = 0.0001f; // randomly chosen small value
            }

            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.noiseOffset = noiseOffset;
        }

        private Vector2[] randomOffset() {
            // prng = pseudorandom number generator
            System.Random prng = new System.Random(seed);

            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                // Mathf.PerlinNoise will give same value over and over if the offset is too high
                // therefore we limit the random number generator to the range [-75000,75000]
                // this range was found through testing
                float offsetX = prng.Next(-75000, 75000) + noiseOffset.x;
                float offsetY = prng.Next(-75000, 75000) + noiseOffset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            return octaveOffsets;
        }

        public float[,] Generate()
        {
            float[,] noiseMap = new float[width, height];

            // We want to be able to generate different types of noise so we 
            // offset the input to Mathf.PerlinNoise using octaveOffsets
            Vector2[] octaveOffsets = randomOffset();

            float maxNosieHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++) 
                    {
                        float sampleX = x / scale * frequency + octaveOffsets[i].x;
                        float sampleY = y / scale * frequency + octaveOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                        // For more interesting noise we might want some octaves to decrease the noise height
                        // therefore we change the range of the perlin value to [-1, 1]
                        perlinValue = perlinValue * 2 - 1; 
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence; // higher octaves have less contribution to noise
                        frequency *= lacunarity; // higher octaves have higher frequency
                    }

                    // Find the extreme points of the noise map to be used later in normalization
                    if (noiseHeight > maxNosieHeight)
                    {
                        maxNosieHeight = noiseHeight;
                    } else if (noiseHeight < minNoiseHeight) {
                        minNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Normalize the noise map to the range [0, 1] because the rest of the program expect these values
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // InverseLerp returns a percentage value beteween minNoiseHeight and maxNoiseHeight
                    // to achieve the desired range
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNosieHeight, noiseMap[x, y]);
                }
            }
            return noiseMap;
        }
    }   
}
