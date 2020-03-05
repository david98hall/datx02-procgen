using System;
using Interfaces;
using UnityEngine;

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
            _textureGenerator.NoiseMap = GenerateHeightMap();
            GetComponent<MeshFilter>().sharedMesh = GenerateMesh();
            _textureGenerator.Strategy = GetStrategy();
            GetComponent<MeshRenderer>().material.mainTexture = _textureGenerator.Generate();
            transform.position = new Vector3((float) width / 2, (width + depth), (float) depth / 2);
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

        private Mesh GenerateMesh()
        {
            var heights = _textureGenerator.Get();
            var vertices = new Vector3[width * depth];
            var textureCoordinates = new Vector2[width * depth];
            for (int z = 0, i = 0; z < depth; z++)
            {
                for (var x = 0; x < width; x++, i++)
                {
                    vertices[i] = new Vector3(x, heightScale * heights[x, z], z);
                    textureCoordinates[i] = new Vector2(x / (float) width, z / (float) depth);
                }
            }

            var triangles = new int[6 * (width - 1) * (depth - 1)];
            for (int z = 0, offset = 0, vertex = 0; z < depth - 1; z++, vertex++)
            {
                for (var x = 0; x < width - 1; x++, vertex++)
                {
                    triangles[offset++] = vertex;
                    triangles[offset++] = vertex + 1;
                    triangles[offset++] = vertex + width;
                    triangles[offset++] = vertex + width + 1;
                    triangles[offset++] = vertex + width;
                    triangles[offset++] = vertex + 1;
                }
            }

            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = textureCoordinates
            };
            
            mesh.RecalculateNormals();
            return mesh;
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
                    return null;
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
                    throw new Exception("There is no such noise map strategy!");
            }
            return new GrayScaleGenerator(textureGenerator);
        }
        
        #endregion
    }
}