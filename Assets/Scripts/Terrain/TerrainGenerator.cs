using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>, IStrategyzer<IGenerator<float[,]>>
    {

        private readonly NoiseGenerator noiseGenerator;

        public IGenerator<float[,]> Strategy
        {
            get => noiseGenerator.Strategy;
            set => noiseGenerator.Strategy = value;
        }

        public TerrainGenerator()
        {
            noiseGenerator = new NoiseGenerator();
        }
        
        public (Mesh, Texture2D) Generate()
        {
            var noiseMap = noiseGenerator.Generate();
            // TODO Generate terrain map
            // TODO Generate texture
            throw new System.NotImplementedException();
        }

        private Texture2D GenerateWhittakerTexture(double[,] noiseMap)
        {
            throw new System.NotImplementedException();
        }
        
    }
}