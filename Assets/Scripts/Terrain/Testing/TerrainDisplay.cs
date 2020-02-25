using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Terrain;
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

        private readonly TerrainGenerator terrainGenerator;
        
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public bool autoUpdate;
        
        public TerrainDisplay()
        {
            noiseMapStrategy = NoiseMapStrategy.PerlinNoise;
            terrainGenerator = new TerrainGenerator(null);
        }
        
        public void GenerateTerrainMesh()
        {
            terrainGenerator.HeightScale = heightScale;
            terrainGenerator.HeightCurve = heightCurve;
            terrainGenerator.NoiseMapStrategy = GetNoiseStrategy();
            var (mesh, texture) = terrainGenerator.Generate();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial.mainTexture = texture;
        }

        private IGenerator<float[,]> GetNoiseStrategy()
        {
            switch (noiseMapStrategy)
            {
                case NoiseMapStrategy.PerlinNoise:
                    return NoiseStrategyFactory.GetPerlinNoiseStrategy(
                        width, height, seed, noiseScale, octaves, persistence, lacunarity, offset);
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
