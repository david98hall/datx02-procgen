﻿using BIAS.Utils.Interfaces;
using UnityEngine;

namespace BIAS.PCG.Noise
{
    /// <summary>
    /// Creates different types of noise map generation strategies.
    /// </summary>
    public class Factory
    {
        /// <summary>
        /// Creates a generator for noise maps using the Perlin noise strategy.
        /// </summary>
        /// <param name="width">The width of the noise maps to generate.</param>
        /// <param name="depth">The depth of the noise maps to generate.</param>
        /// <param name="seed">The seed of the noise maps to generate.</param>
        /// <param name="scale">The scale of the noise maps to generate.</param>
        /// <param name="numOctaves">
        /// The number of octaves ("noise functions") that will be used to generate noise maps.
        /// </param>
        /// <param name="persistence">The persistence of the noise maps  to generate.</param>
        /// <param name="lacunarity">The lacunarity of the noise maps (distance between patterns) to generate.</param>
        /// <param name="noiseOffset">The offset of the noise maps to generate.</param>
        /// <returns>A generator that generates noise maps.</returns>
        public IGenerator<float[,]> CreatePerlinNoiseStrategy(
            int width,
            int depth,
            int seed,
            float scale,
            int numOctaves,
            float persistence,
            float lacunarity,
            Vector2 noiseOffset)
        {
            return new PerlinNoiseStrategy
            {
                Width = width,
                Depth = depth,
                Seed = seed,
                Scale = scale,
                NumOctaves = numOctaves,
                Persistence = persistence,
                Lacunarity = lacunarity,
                NoiseOffset = noiseOffset
            };
        }

        public IGenerator<Mesh> CreateMeshGenerator(
            IInjector<float[,]> noiseMapInjector, 
            AnimationCurve heightCurve, 
            float heightScale)
        {
            return new NoiseMeshGenerator(noiseMapInjector){HeightCurve = heightCurve, HeightScale = heightScale};
        }
    }
}