using System;
using Interfaces;
using UnityEngine;
using Utils;

namespace Terrain.Testing
{
    /// <summary>
    /// A visual testing class for displaying <see cref = "TextureGenerator"/>
    /// </summary>
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TextureDisplay : MonoBehaviour
    {
        #region Properties
        
        private TextureGenerator _textureGenerator = new TextureGenerator(null);
        
        public Strategy strategy;
        public enum Strategy
        {
            GrayScale,
            Whittaker
        }
        
        public int width = 2;
        public int depth = 2;
        public float heightScale;

        #region Whittaker properties

        public WhittakerMap whittakerMap;
        public enum WhittakerMap
        {
            Texture,
            Height,
            Precipitation,
            Temperature,
        }
        
        public float precipitationScale = 1;
        public float temperatureScale = 1;

        #endregion
        
        #endregion

        #region Public Methods

        public void Refresh()
        {
            _textureGenerator.NoiseMap = TerrainUtil.Slope(width, depth);
            GetComponent<MeshFilter>().sharedMesh = TerrainUtil.Mesh(_textureGenerator.Get(), heightScale);
            _textureGenerator.Strategy = GetStrategy();
            GetComponent<MeshRenderer>().material.mainTexture = _textureGenerator.Generate();
            transform.position = new Vector3((float) width / 2, width + depth, (float) depth / 2);
        }
        
        #endregion

        #region Private Methods

        private float[,] GenerateHeightMap()
        {
            var heightMap = new float[width, depth];
            for (var z = 0; z < heightMap.GetLength(1); z++)
            {
                for (var x = 0; x < heightMap.GetLength(0); x++)
                {
                    heightMap[x, z] = (float) x / (2 * width) + (float) z / (2 * depth);
                }
            }

            return heightMap;
        }

        private IGenerator<Texture2D> GetStrategy()
        {
            switch (strategy)
            {
                case Strategy.GrayScale:
                    return new GrayScaleGenerator(_textureGenerator);
                case Strategy.Whittaker:
                    return GetWhittakerStrategy();
                default:
                    throw new Exception("There is no such strategy!");
            }
        }

        private IGenerator<Texture2D> GetWhittakerStrategy()
        {
            var whittakerGenerator = new WhittakerGenerator(_textureGenerator)
            {
                PrecipitationScale = precipitationScale, 
                TemperatureScale = temperatureScale
            };

            var textureGenerator = new TextureGenerator(null);
            switch (whittakerMap)
            {
                case WhittakerMap.Texture:
                    return whittakerGenerator;
                case WhittakerMap.Height:
                    textureGenerator.NoiseMap = whittakerGenerator.HeightMap;
                    break;
                case WhittakerMap.Precipitation:
                    textureGenerator.NoiseMap = whittakerGenerator.PrecipitationMap;
                    break;
                case WhittakerMap.Temperature:
                    textureGenerator.NoiseMap = whittakerGenerator.TemperatureMap;
                    break;
                default:
                    throw new Exception("There is no such strategy!");
            }
            return new GrayScaleGenerator(textureGenerator);
        }
        
        #endregion
    }
}