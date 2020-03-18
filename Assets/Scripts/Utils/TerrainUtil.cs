using System;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Utils
{
    public class TerrainUtil
    {
        
        private TerrainUtil() {}

        public static Mesh Mesh(float[,] heightMap, float scale)
        {
            var width = heightMap.GetLength(0);
            var depth = heightMap.GetLength(1);
            var vertices = new Vector3[width * depth];
            var textureCoordinates = new Vector2[width * depth];
            var triangles = new int[6 * (width - 1) * (depth - 1)];
            
            for (int z = 0, i = 0; z < depth; z++)
            {
                for (var x = 0; x < width; x++, i++)
                {
                    vertices[i] = new Vector3(x, scale * heightMap[x, z], z);
                    textureCoordinates[i] = new Vector2(x / (float) width, z / (float) depth);
                }
            }

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

        public static float[,] Flat(int width, int depth) => HeightMap(width, depth, (x, z) => 0);

        public static float[,] Slope(int width, int depth)
        {
            return HeightMap(width, depth, (x, z) => 
                (float) x / (2 * (width - 1)) + (float) z / (2 * (depth - 1)));
        }

        public static float[,] HeightMap(int width, int depth, Func<int, int, float> height)
        {
            var heightMap = new float[width, depth];
            for (var z = 0; z < width; z++)
            {
                for (var x = 0; x < depth; x++)
                {

                    heightMap[x, z] = height(x, z);
                }
            }

            return heightMap;
        }
    }
}