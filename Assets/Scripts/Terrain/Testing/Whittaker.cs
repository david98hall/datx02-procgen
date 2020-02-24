using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain.Testing
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Whittaker : MonoBehaviour
    {
        public int width = 1;
        public int depth = 1;
        void Start()
        {
            //old();
            
            var noiseMap = NoiseMap();
            var mesh = Mesh(noiseMap);
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshRenderer>().material.mainTexture = new Terrain.Whittaker(noiseMap).Generate();
        }
        
        private float[,] NoiseMap()
        {
            var noiseMap = new float[width, depth];

            for (var x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (var z = 0; z < noiseMap.GetLength(1); z++)
                {
                    noiseMap[x, z] = x + z;
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

            var triangles = new int[6 * (width - 1) + (depth - 1)];
            for (int z = 0, offset = 0, vertex = 0; z < depth - 1; z++, vertex++)
            {
                for (var x = 0; x < width - 1; x++, vertex++)
                {
                    triangles[offset++] = vertex;
                    triangles[offset++] = vertex + 1;
                    triangles[offset++] = vertex + width + 1;
                    triangles[offset++] = vertex + width + 2;
                    triangles[offset++] = vertex + width + 1;
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

        private void old()
        {
            var length = depth;
            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        
            Vector3[] vertices = new Vector3[(length + 1) * (width + 1)];
            for (int z = 0, i = 0; z <= length; z++)
            {
                for (int x = 0; x <= width; x++, i++)
                {
                    // todo compute y
                    vertices[i] = new Vector3(x, 0, z);
                }
            }
        
            int[] triangles = new int[6 * length * width];
            for (int z = 0, offset = 0, vertex = 0; z < length; z++, vertex++)
            {
                for (int x = 0; x < width; x++, vertex++)
                {
                    triangles[offset++] = vertex;
                    triangles[offset++] = vertex + 1;
                    triangles[offset++] = vertex + width + 1;
                    triangles[offset++] = vertex + width + 2;
                    triangles[offset++] = vertex + width + 1;
                    triangles[offset++] = vertex + 1;
                    Debug.Log(vertex);
                }
            }
        
            Color[] testColors = {Color.red, Color.yellow, Color.blue};
            Color[] colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = testColors[i % testColors.Length];
            }
        
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
        }
    }
}


