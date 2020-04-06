using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Terrain;
using Terrain.Noise;
using UnityEngine;

namespace Demo
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Demo2 : MonoBehaviour
    {
        private readonly Terrain.TerrainGenerator _terrainGenerator;
        private readonly Dictionary<NoiseStrategy, IGenerator<float[,]>> _noiseStrategies;
        private readonly Dictionary<string, IGenerator<Texture2D>> _textureStrategies;
        
        //internal string[] NoiseStrategies => _noiseStrategies.Keys.ToArray();
        internal string[] TextureStrategies => _textureStrategies.Keys.ToArray();
        
        [Header("Settings")]
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        [Header("Terrain Generation")]
        
        public AnimationCurve heightCurve;

        [Range(0, 10)]
        public float heightScale;

        //[HideInInspector] 
        public NoiseStrategy noiseStrategy;

        [HideInInspector]
        public int noiseStrategyIndex;
        
        [HideInInspector]
        public int textureStrategyIndex;

        public Demo2()
        {
            _terrainGenerator = new Terrain.TerrainGenerator();
            
            // todo Factory.createPerlinNoise
            _noiseStrategies = new Dictionary<NoiseStrategy, IGenerator<float[,]>>
            {
                [NoiseStrategy.PerlinNoise] = new PerlinNoiseStrategy()
            };

            var textureStrategyFactory = _terrainGenerator.TextureStrategyFactory;
            _textureStrategies = new Dictionary<string, IGenerator<Texture2D>>
            {
                ["Grayscale"] = textureStrategyFactory.CreateGrayScaleStrategy(),
                ["Whittaker"] = textureStrategyFactory.CreateWhittakerStrategy()
            };
        }
        private void Update()
        {
            _terrainGenerator.HeightCurve = heightCurve;
            _terrainGenerator.HeightScale = heightScale;
            _terrainGenerator.NoiseStrategy = _noiseStrategies.ElementAt(noiseStrategyIndex).Value;
            _terrainGenerator.TextureStrategy = _textureStrategies.ElementAt(textureStrategyIndex).Value;
            (meshFilter.sharedMesh, meshRenderer.sharedMaterial.mainTexture) = _terrainGenerator.Generate();
        }

        public enum NoiseStrategy
        {
            PerlinNoise,
        }
    }
}