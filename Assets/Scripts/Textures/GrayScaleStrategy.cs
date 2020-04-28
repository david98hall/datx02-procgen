using Interfaces;
using UnityEngine;

namespace Textures
{
    
    /// <summary>
    /// Can generate a gray scaled texture based on a noise map.
    /// </summary>
    internal class GrayScaleStrategy : Strategy<float[,], Texture2D>
    {
        /// <summary>
        /// Initializes this generator with a injector from which a noise map can be fetched
        /// </summary>
        /// <param name="noiseMapInjector">
        /// The noise map injector used to generate a texture
        /// </param>
        internal GrayScaleStrategy(IInjector<float[,]> noiseMapInjector) : base(noiseMapInjector)
        {
        }

        /// <summary>
        /// Generates a gray scaled texture based on a noise map.
        /// </summary>
        /// <returns></returns>
        public override Texture2D Generate()
        {
            var noiseMap = Injector.Get();
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);
            
            // Interpolate between black and white based on the noise map's values
            var pixelColors = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    // Cancel if requested
                    if (CancelToken.IsCancellationRequested) return null;
                    
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