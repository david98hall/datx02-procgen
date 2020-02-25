using UnityEngine;

namespace Terrain.Testing
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Whittaker : MonoBehaviour
    {
        public int width = 2;
        public int depth = 2;
        void Start()
        {
            var noiseMap = NoiseMap();
            var mesh = Mesh(noiseMap);
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().material.mainTexture = new Terrain.Whittaker(noiseMap).Generate();
        }
        
        private float[,] NoiseMap()
        {
            var noiseMap = new float[width, depth];

            for (var z = 0; z < noiseMap.GetLength(1); z++)
            {
                for (var x = 0; x < noiseMap.GetLength(0); x++)
                {
                    noiseMap[x, z] = 0;
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
                    vertices[i] = new Vector3(x, noiseMap[x, z], z);
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

            return new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = textureCoordinates
            };
        }
    }
}


