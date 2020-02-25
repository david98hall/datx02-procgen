using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Creates different types of noise map generation strategies.
    /// </summary>
    public static class NoiseStrategyFactory
    {
        /// <summary>
        /// Instantiates a perlin noise strategy for generating noise maps.
        /// </summary>
        /// <param name="width">The width of the noise map which can be generated.</param>
        /// <param name="height">The height of the noise map which can be generated.</param>
        /// <param name="seed">The seed used for pseudo-randomness when generating a noise map.</param>
        /// <param name="scale">The scale of the noise map.</param>
        /// <param name="octaves">The number of octaves (noise functions added together) used when generating.</param>
        /// <param name="persistence">TODO</param>
        /// <param name="lacunarity">TODO</param>
        /// <param name="offset">The offset when generating a noise map.</param>
        /// <returns></returns>
        public static IGenerator<float[,]> GetPerlinNoiseStrategy(
            int width, int height,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity,
            Vector2 offset)
        {
            return new PerlinNoiseStrategy(width, height, seed, scale, octaves, persistence, lacunarity, offset);
        }
    }
}