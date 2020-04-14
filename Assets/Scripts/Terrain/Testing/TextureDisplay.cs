using System;
using Interfaces;
using Terrain.Textures;
using UnityEngine;
using Utils;

namespace Terrain.Testing
{
    /// <summary>
    /// A visual testing class for displaying generation of textures
    /// </summary>
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TextureDisplay : MonoBehaviour, IInjector<float[,]>
    {
        #region Properties
        
        private float[,] _noiseMap;

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
            _noiseMap = TerrainUtil.Pyramid(width, depth);
            GetComponent<MeshFilter>().sharedMesh = TerrainUtil.Mesh(_noiseMap, heightScale);
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = GetStrategy().Generate();
            transform.position = new Vector3((float) width / 2, width + depth, (float) depth / 2);
        }
        
        public float[,] Get()
        {
            return _noiseMap;
        }
        
        #endregion

        #region Private Methods

        private IGenerator<Texture2D> GetStrategy()
        {
            switch (strategy)
            {
                case Strategy.GrayScale:
                    return new GrayScaleStrategy(this);
                case Strategy.Whittaker:
                    return GetWhittakerStrategy();
                default:
                    throw new Exception("There is no such strategy!");
            }
        }

        private IGenerator<Texture2D> GetWhittakerStrategy()
        {
            var whittakerGenerator = new WhittakerStrategy(this)
            {
                PrecipitationScale = precipitationScale, 
                TemperatureScale = temperatureScale
            };

            switch (whittakerMap)
            {
                case WhittakerMap.Texture:
                    return whittakerGenerator;
                case WhittakerMap.Precipitation:
                    _noiseMap = whittakerGenerator.PrecipitationMap;
                    break;
                case WhittakerMap.Temperature:
                    _noiseMap = whittakerGenerator.TemperatureMap;
                    break;
                default:
                    throw new Exception("There is no such strategy!");
            }
            return new GrayScaleStrategy(this);
        }
        
        #endregion
    }
}