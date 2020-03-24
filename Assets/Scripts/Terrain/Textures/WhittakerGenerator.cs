using System;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Terrain.Textures
{
    /// <summary>
    /// Generates 2D textures from a given height map with a simple version of a Whittaker diagram.
    /// A Whittaker diagram define a simple ecosystem depending on temperature and precipitation.
    /// This class also takes the height into consideration.
    /// see <see cref = "Interfaces.IGenerator{T}"/>.
    /// </summary>
    internal class WhittakerGenerator : Strategy<float[,], Texture2D>
    {
        #region Properties and constructors

        /// <returns>
        /// A copy of the height map from <see cref = "Strategy{TI,TO}.Injector"/>.
        /// </returns>
        internal float[,] HeightMap => (float[,]) Injector.Get().Clone();

        /// <summary>
        /// The scale used for generating a precipitation map
        /// </summary>
        internal float PrecipitationScale { get; set; }
        
        /// <returns>
        /// Generates a precipitation map with <see cref="GenerateMap"/>.
        /// </returns>
        internal float[,] PrecipitationMap => (float[,]) GenerateMap(PrecipitationScale).Clone();
        
        /// <summary>
        /// The scale used for generating a temperature map.
        /// </summary>
        internal float TemperatureScale { get; set; }
        
        /// <returns>
        /// Generates a temperature map with <see cref="GenerateMap"/>.
        /// </returns>
        internal float[,] TemperatureMap => (float[,]) GenerateMap(TemperatureScale).Clone();

        /// <summary>
        /// Constructs a WhittakerGenerator object from a given height map injector.
        /// <param name = "heightMapInjector"> The given height map.</param>.
        /// </summary>
        internal WhittakerGenerator([NotNull] IInjector<float[,]> heightMapInjector) : base(heightMapInjector)
        {
        }
        
        #endregion
        
        #region public and internal methods

        /// <summary>
        /// Generates the whittaker textures from the height, precipitation and temperature maps.
        /// See <see cref = "Interfaces.IGenerator{T}.Generate()"/>.
        /// See <see cref = "ComputeColor(float, float, float)"/>.
        /// </summary>
        /// <returns>
        /// The generated Texture2D object.
        /// </returns>
        public override Texture2D Generate()
        {
            var heights = Injector.Get();
            var precipitation = GenerateMap(PrecipitationScale);
            var temperature = GenerateMap(TemperatureScale);
            
            var texture = new Texture2D(heights.GetLength(0), heights.GetLength(1));
            
            // for each pixel in texture, set color of texture
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    var color = ComputeColor(heights[x, y], precipitation[x, y], temperature[x, y]);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply(false);
            return texture;
        }
        
        #endregion

        #region Private methods
        
        /// <summary>
        /// Generates a 2x2 array (map) with the same size as the height map from
        /// <see cref = "Strategy{TI,TO}.Injector"/> in the range of [0, 1].
        /// The map is generated with <see cref = "Mathf.PerlinNoise(float, float)"/> with a given scale.
        /// <param name = "scale"> The given scale.</param>
        /// </summary>
        /// <returns>
        /// The generated map.
        /// </returns>
        private float[,] GenerateMap(float scale)
        {
            var heights = Injector.Get();
            var map = new float[heights.GetLength(0), heights.GetLength(1)];
            
            for (var x = 0; x < map.GetLength(0); x++)
            {
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = Math.Max(0, Math.Min(1, Mathf.PerlinNoise(x / scale, y / scale)));
                }
            }

            return map;
        }
        
        /// <summary>
        /// Computes a color from a simple Whittaker ecosystem using a given height,
        /// precipitation and temperature.
        /// 
        /// The colors of the ecosystem is defined as:
        /// Whittaker [Precipitation, Temperature] in [0, 0] to [1, 1]
        /// Gray   = [0, 0]
        /// White  = [1, 0]
        /// Yellow = [0, 1]
        /// Green  = [1, 1]
        ///
        /// The color is computed by interpolated between the colors using the precipitation and temperature.
        /// The precipitation and temperature are recalculated with the height as a bias.
        /// </summary>
        /// <returns>
        /// The computed color.
        /// </returns>
        private static Color ComputeColor(float height, float precipitation, float temperature)
        {
            // precipitation tends to remain high as height increases
            precipitation = (float) Math.Pow(precipitation, 1.0 - height);
            
            // temperature tend remain low as height increases
            temperature = (float) Math.Pow(temperature, 1 / (1.0 - height));
            
            // interpolate colors by precipitation
            var lowTemperature = Color.Lerp(Color.gray, Color.white, precipitation);
            var highTemperature = Color.Lerp(Color.yellow, Color.green, precipitation);

            // interpolate colors by temperature
            return Color.Lerp(lowTemperature, highTemperature, Math.Max(0, temperature));
        }
        
        #endregion
    }
}
