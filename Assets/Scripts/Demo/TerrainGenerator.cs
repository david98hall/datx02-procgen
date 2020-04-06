using System;
using System.Collections.Generic;
using Interfaces;
using Terrain.Textures;
using UnityEngine;

namespace Demo
{
    [Serializable]
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>
    {
        private readonly Terrain.TerrainGenerator _terrainGenerator = new Terrain.TerrainGenerator();
        
        private readonly Dictionary<NoiseStrategy, IGenerator<float[,]>> _noiseStrategies;
        private readonly Dictionary<TextureStrategy, IGenerator<Texture2D>> _textureStrategies;

        public enum NoiseStrategy
        {
            PerlinNoise
        }

        public enum TextureStrategy
        {
            GrayScale, Whittaker
        }
        
        public AnimationCurve heightCurve;
        
        [Range(0, 10)]
        public float heightScale;

        public NoiseStrategy noiseStrategy;
        public PerlinNoiseStrategy perlinNoiseStrategy;
        
        public TextureStrategy textureStrategy;
        public WhittakerStrategy whittakerStrategy;

        public TerrainGenerator()
        {
            _noiseStrategies = new Dictionary<NoiseStrategy, IGenerator<float[,]>>
            {
                [NoiseStrategy.PerlinNoise] = new Terrain.Noise.PerlinNoiseStrategy()
            };

            _textureStrategies = new Dictionary<TextureStrategy, IGenerator<Texture2D>>
            {
                [TextureStrategy.GrayScale] = new GrayScaleStrategy(_terrainGenerator),
                [TextureStrategy.Whittaker] = new Terrain.Textures.WhittakerStrategy(_terrainGenerator)
            };
        }
        
        public (Mesh, Texture2D) Generate()
        {
            _terrainGenerator.HeightCurve = heightCurve;
            _terrainGenerator.HeightScale = heightScale;
            _terrainGenerator.NoiseStrategy = _noiseStrategies[noiseStrategy];
            _terrainGenerator.TextureStrategy = _textureStrategies[textureStrategy];
            return _terrainGenerator.Generate();
        }
        
        [Serializable]
        public struct PerlinNoiseStrategy
        {
            [Range(1, 100)]
            public int width;
            
            [Range(1, 100)]
            public int depth;
            
            public int seed;
            
            [Range(1, 10)]
            public float scale;
            
            public int numOctaves;
            
            public float persistence;
            
            public float lacunarity;
            
            public Vector2 noiseOffset;
        }

        [Serializable]
        public struct WhittakerStrategy
        {
            [Range(0, 50)]
            public float percipitationScale;
            
            [Range(0, 50)]
            public float temperatureScale;
        }
    }
}