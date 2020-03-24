using System;
using Interfaces;
using Terrain.Textures;
using UnityEngine;

namespace Terrain.Testing
{
    internal class TerrainDisplay : MonoBehaviour
    {
        
        public enum NoiseMapStrategy
        {
            PerlinNoise
        }

        public NoiseMapStrategy noiseMapStrategy;
        
        public int width;
        public int height;
        public float noiseScale;
        public int seed;

        // More octaves aren't necessary and too many octaves result in a performance decrease
        [Range(1,10)]  
        public int octaves;

        // The amplitude of higher octaves should decrease
        [Range(0, 1)]
        public float persistence;

        // A lacunarity above 10 makes the terrain too spikey
        [Range(1, 10)]
        public float lacunarity;
        public Vector2 offset;

        public float heightScale;
        public AnimationCurve heightCurve;

        private readonly TerrainGenerator _terrainGenerator;
        
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public enum TextureStrategy
        {
            Whittaker, GrayScale
        }

        public TextureStrategy textureStrategy;
        public float precipitationScale;
        public float temperatureScale;

        public bool autoUpdate;

        public TerrainDisplay()
        {
            noiseMapStrategy = NoiseMapStrategy.PerlinNoise;
            _terrainGenerator = new TerrainGenerator();
        }
        
        public void GenerateTerrainMesh()
        {
            _terrainGenerator.HeightScale = heightScale;
            _terrainGenerator.HeightCurve = heightCurve;
            _terrainGenerator.NoiseStrategy = GetNoiseStrategy();
            _terrainGenerator.TextureStrategy = GetTextureStrategy();
            (meshFilter.sharedMesh, meshRenderer.sharedMaterial.mainTexture) = _terrainGenerator.Generate();
        }

        private IGenerator<float[,]> GetNoiseStrategy()
        {
            switch (noiseMapStrategy)
            {
                case NoiseMapStrategy.PerlinNoise:
                    return new Factory().CreatePerlinNoiseStrategy(
                        width, height, seed, noiseScale, octaves, persistence, lacunarity, offset);
                default:
                    throw new Exception("There is no such noise map strategy!");
            }
        }
        
        private IGenerator<Texture2D> GetTextureStrategy()
        {
            switch (textureStrategy)
            {
                case TextureStrategy.Whittaker:
                    var whittakerGenerator = 
                        (WhittakerGenerator) _terrainGenerator.TextureStrategyFactory.CreateWhittakerStrategy();
                    whittakerGenerator.PrecipitationScale = precipitationScale;
                    whittakerGenerator.TemperatureScale = temperatureScale;
                    return whittakerGenerator;
                case TextureStrategy.GrayScale:
                    return _terrainGenerator.TextureStrategyFactory.CreateGrayScaleStrategy();
                default:
                    throw new Exception("There is no such noise map strategy!");
            }
        }
          
        private void OnValidate()
        {
            if (width < 1) 
                width = 1;
            if (height < 1) 
                height = 1;
        }
        
    }
}
