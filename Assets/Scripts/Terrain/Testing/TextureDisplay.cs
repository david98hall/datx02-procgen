﻿using System;
using Interfaces;
using UnityEngine;

namespace Terrain.Testing
{
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TextureDisplay : MonoBehaviour
    {
        #region Properties
        
        private TextureGenerator _generator = new TextureGenerator(null);
        public Strategy strategy;
        public enum Strategy
        {
            Whittaker
        }
        
        public int width = 2;
        public int depth = 2;
        public float heightScale;

        #region Whittaker properties
        public float temperatureScale = 1;
        public float precipitationScale = 1;

        #endregion
        
        #endregion

        #region Public Methods

        public void Refresh()
        {
            var heightMap = HeightMap();
            GetComponent<MeshFilter>().sharedMesh = Mesh(heightMap);
            _generator.Strategy = GetStrategy();
            GetComponent<MeshRenderer>().material.mainTexture = _generator.Generate();
            transform.position = new Vector3((float) -width / 2, -(width + depth), (float) -depth / 2);
        }
        
        #endregion

        #region Private Methods

        private float[,] HeightMap()
        {
            var noiseMap = new float[width, depth];
            for (var z = 0; z < noiseMap.GetLength(1); z++)
            {
                for (var x = 0; x < noiseMap.GetLength(0); x++)
                {
                    noiseMap[x, z] = (float) x / (2 * width) + (float) z / (2 * depth);
                }
            }
            return noiseMap;
        }

        private Mesh Mesh(float[,] noiseMap)
        {
            var vertices = new Vector3[width * depth];
            var textureCoordinates = new Vector2[width * depth];
            for (int z = 0, i = 0; z < depth; z++)
            {
                for (var x = 0; x < width; x++, i++)
                {
                    vertices[i] = new Vector3(x, heightScale * noiseMap[x, z], z);
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
                case Strategy.Whittaker:
                    return new WhittakerGenerator(null, temperatureScale, precipitationScale);
                default:
                    return new WhittakerGenerator(null, temperatureScale, precipitationScale);
            }
        }
        
        #endregion
    }
}