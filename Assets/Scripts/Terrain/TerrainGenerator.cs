using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>, IStrategyzer<IGenerator<double[,]>>
    {

        private readonly NoiseGenerator noiseGenerator;

        public IGenerator<double[,]> Strategy
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
            // var texture = GenerateWhittakerTexture(noiseMap);
            var texture = TEMP_TextureFromHeightMap(noiseMap);
            throw new System.NotImplementedException();
        }

        private Texture2D GenerateWhittakerTexture(double[,] noiseMap)
        {
            throw new System.NotImplementedException();
        }
        
        // TODO: Remove this method (it's temporary and used for testing)
        private static Texture2D TEMP_TextureFromHeightMap(double[,] heightMap)
        {
            var width = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);

            // Map colors to the noise map values
            var colorMap = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, (float) heightMap[x, y]);
                }
            }

            return TEMP_TextureFromColorMap(colorMap, width, height);
        }
        
        // TODO: Remove this method (it's temporary and used for testing)
        private static Texture2D TEMP_TextureFromColorMap(Color[] colorMap, int width, int height)
        {
            var texture = new Texture2D(width, height);
            texture.SetPixels(colorMap);
            texture.Apply();
            return texture;
        }
        
    }
}