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
    public class TerrainGenerator2 : IGenerator<(Mesh, Texture2D)>
    {
        private readonly Terrain.TerrainGenerator _terrainGenerator = new Terrain.TerrainGenerator();
        private readonly Dictionary<string, IGenerator<float[,]>> _noiseStrategies 
            = new Dictionary<string, IGenerator<float[,]>>();

        internal string[] NoiseStrategies => _noiseStrategies.Keys.ToArray();

        
        public AnimationCurve heightCurve;

        [Range(0, 10)]
        public float heightScale;

        [HideInInspector]
        public int noiseStrategyIndex;

        public TerrainGenerator2()
        {
            _noiseStrategies["Perlin Noise"] = new PerlinNoiseStrategy();
        }
        public (Mesh, Texture2D) Generate()
        {
            _terrainGenerator.HeightCurve = heightCurve;
            _terrainGenerator.HeightScale = heightScale;
            return _terrainGenerator.Generate();
        }
    }
}