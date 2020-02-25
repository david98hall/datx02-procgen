using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class WhittakerGenerator : IGenerator<Texture2D>
    {
        #region Properties
        
        private readonly float[,] _heightMap; // [0, 1]
        public float[,] HeightMap => (float[,]) _heightMap.Clone();
        
        private readonly float[,] _temperatureMap; // [0, 1]
        public float[,] TemperatureMap => (float[,]) _temperatureMap.Clone();

        private readonly float[,] _precipitationMap;
        public float[,] PrecipitationMap => (float[,]) _precipitationMap.Clone();
        
        #endregion
        
        #region public methods
        
        public WhittakerGenerator(float[,] heightMap, float temperatureScale, float precipitationScale)
        {
            _heightMap = heightMap;
            _temperatureMap = GenerateMap(temperatureScale);
            _precipitationMap = GenerateMap(precipitationScale);
        }
        
        public Texture2D Generate()
        {
            Texture2D texture = new Texture2D(_heightMap.GetLength(0), 
                _heightMap.GetLength(1));

            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    Color color = GetColor(_heightMap[x, y], _temperatureMap[x, y], _precipitationMap[x, y]);
                    texture.SetPixel(x, y, color);
                }
            }
            
            texture.Apply(false);
            return texture;
        }
        
        #endregion

        #region Private 
        
        private float[,] GenerateMap(float scale)
        {
            var map = new float[_heightMap.GetLength(0), _heightMap.GetLength(1)];
            for (var x = 0; x < map.GetLength(0); x++)
            {
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = Mathf.PerlinNoise(x / scale, y / scale);
                }
            }

            return map;
        }
        private Color GetColor(float height, float temperature, float precipitation)
        {
            // todo add bias for height
            Color first = Color.Lerp(Color.gray, Color.yellow, temperature);
            Color second = Color.Lerp(Color.white, Color.green, temperature);
            return Color.Lerp(first, second, precipitation);
        }
        
        #endregion
    }
}
