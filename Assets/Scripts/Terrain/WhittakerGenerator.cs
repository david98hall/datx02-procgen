using System;
using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates 2D textures from a given height map with a simple version of a Whittaker diagram.
    /// A Whittaker diagram define a simple ecosystem depending on temperature and precipitation.
    /// This class also takes the height into consideration
    /// see <see cref = "Interfaces.IGenerator{T}"/>.
    /// </summary>
    internal class WhittakerGenerator : IGenerator<Texture2D>
    {
        #region Properties
        
        /// <summary>
        /// The height map of which the whittaker texture is generated from.
        /// </summary>
        private readonly float[,] _heightMap; // [0, 1]
        
        /// <returns>
        /// A copy of <see cref = "_heightMap"/>.
        /// </returns>
        internal float[,] HeightMap => (float[,]) _heightMap.Clone();
        
        /// <summary>
        /// The precipitation map of which the whittaker texture is generated from.
        /// </summary>
        private readonly float[,] _precipitationMap;
        
        /// <returns>
        /// A copy of <see cref = "_precipitationMap"/>.
        /// </returns>
        internal float[,] PrecipitationMap => (float[,]) _precipitationMap.Clone();

        /// <summary>
        /// The temperature map of which the whittaker texture is generated from.
        /// </summary>
        private readonly float[,] _temperatureMap; // [0, 1]
        
        /// <returns>
        /// A copy of <see cref = "_temperatureMap"/>.
        /// </returns>
        internal float[,] TemperatureMap => (float[,]) _temperatureMap.Clone();

        #endregion
        
        #region public and internal methods
        
        /// <summary>
        /// Constructs a WhittakerGenerator object from a given height map and generates
        /// precipitation and temperature maps of the same size.
        /// <param name = "heightMap"> The given height map.</param>
        /// <param name = "precipitationScale"> The scale used to generate the precipitation map</param>
        /// <param name = "temperatureScale"> The scale used to generate the temperature map</param>
        /// See <see cref = "GenerateMap(float)"/>.
        /// </summary>
        internal WhittakerGenerator(float[,] heightMap, float precipitationScale, float temperatureScale)
        {
            _heightMap = heightMap;
            _precipitationMap = GenerateMap(precipitationScale);
            _temperatureMap = GenerateMap(temperatureScale);
        }
        
        /// <summary>
        /// Generates the whittaker textures from the height, precipitation and temperature maps
        /// see <see cref = "Interfaces.IGenerator{T}.Generate()"/>.
        /// See <see cref = "ComputeColor(float, float, float)"/>.
        /// </summary>
        /// <returns>
        /// The generated Texture2D object
        /// </returns>
        public Texture2D Generate()
        {
            Texture2D texture = new Texture2D(_heightMap.GetLength(0), 
                _heightMap.GetLength(1));
            
            // for each pixel in texture, set color of texture
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    Color color = ComputeColor(_heightMap[x, y], 
                        _precipitationMap[x, y], _temperatureMap[x, y]);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply(false);
            return texture;
        }
        
        #endregion

        #region Private 
        
        /// <summary>
        /// Generates a 2x2 array (map) with the same size as <see cref = "_heightMap"/> in the range of [0, 1].
        /// The map is generated with <see cref = "Mathf.PerlinNoise(float, float)"/> with a given scale.
        /// <param name = "scale"> The given scale.</param>
        /// </summary>
        /// <returns>
        /// The generated map
        /// </returns>
        private float[,] GenerateMap(float scale)
        {
            var map = new float[_heightMap.GetLength(0), _heightMap.GetLength(1)];
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
        /// The computed color
        /// </returns>
        private Color ComputeColor(float height, float precipitation, float temperature)
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
