using Interfaces;
using UnityEngine;

namespace Terrain.Noise
{
    internal class PerlinNoiseStrategy : IGenerator<float[,]>
    {
        internal int Width { get; set; }
        internal int Depth { get; set; }
        internal int Seed { get; set; }
        internal float Scale { get; set; }

        internal int NumOctaves { get; set; }
        internal float Persistence { get; set; }
        internal float Lacunarity { get; set; }
        internal Vector2 NoiseOffset { get; set; }

        private Vector2[] RandomOffset() {
            var random = new System.Random(Seed); // Pseudo random

            var octaveOffsets = new Vector2[NumOctaves];

            for (var i = 0; i < NumOctaves; i++)
            {
                // Mathf.PerlinNoise will give same value over and over if the offset is too high
                // therefore we limit the random number generator to the range [-75000,75000]
                // this range was found through testing
                var offsetX = random.Next(-75000, 75000) + NoiseOffset.x;
                var offsetY = random.Next(-75000, 75000) + NoiseOffset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            return octaveOffsets;
        }

        public float[,] Generate()
        {
            var noiseMap = new float[Width, Depth];

            // We want to be able to generate different types of noise so we 
            // offset the input to Mathf.PerlinNoise using octaveOffsets
            var octaveOffsets = RandomOffset();

            var maxNoiseHeight = float.MinValue;
            var minNoiseHeight = float.MaxValue;

            for (var y = 0; y < Depth; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (var i = 0; i < NumOctaves; i++) 
                    {
                        var sampleX = x / Scale * frequency + octaveOffsets[i].x;
                        var sampleY = y / Scale * frequency + octaveOffsets[i].y;

                        var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                        // For more interesting noise we might want some octaves to decrease the noise height
                        // therefore we change the range of the perlin value to [-1, 1]
                        perlinValue = perlinValue * 2 - 1; 
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= Persistence; // higher octaves have less contribution to noise
                        frequency *= Lacunarity; // higher octaves have higher frequency
                    }

                    // Find the extreme points of the noise map to be used later in normalization
                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    } else if (noiseHeight < minNoiseHeight) {
                        minNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Normalize the noise map to the range [0, 1] because the rest of the program expect these values
            for (var y = 0; y < Depth; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    // InverseLerp returns a percentage value beteween minNoiseHeight and maxNoiseHeight
                    // to achieve the desired range
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }
            return noiseMap;
        }

        //public float[,] Get() => Generate();
    }   
}
