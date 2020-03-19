using Interfaces;
using UnityEngine;

namespace Terrain
{
    
    /// <summary>
    /// Can generate a gray scaled texture based on a noise map.
    /// </summary>
    internal class GrayScaleGenerator : IGenerator<Texture2D>
    {
        private readonly IInjector<float[,]> _noiseMapInjector;
        
        /// <summary>
        /// Initializes this generator with a injector from which a noise map can be fetched
        /// </summary>
        /// <param name="noiseMapInjector">
        /// The noise map injector used to generate a texture
        /// </param>
        internal GrayScaleGenerator(IInjector<float[,]> noiseMapInjector)
        {
            _noiseMapInjector = noiseMapInjector;
        }
        
        /// <summary>
        /// The noise map used to generate textures (values are in the range: [0, 1])
        /// </summary>
        internal float[,] NoiseMap => (float[,]) _noiseMapInjector.Get().Clone();
        
        
        
        /// <summary>
        /// Generates a gray scaled texture based on a noise map.
        /// </summary>
        /// <returns></returns>
        public Texture2D Generate()
        {
            var noiseMap = _noiseMapInjector.Get();
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);
            
            // Interpolate between black and white based on the noise map's values
            var pixelColors = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    pixelColors[x * height + y] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                }
            }

            return CreateColoredTexture(pixelColors, width, height);
        }

        private static Texture2D CreateColoredTexture(Color[] pixelColors, int width, int height)
        {
            var texture = new Texture2D(width, height);
            texture.SetPixels(pixelColors);
            texture.Apply();
            return texture;
        }
        
    }
}