using Extensions;
using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates terrain meshes based on noise maps generated with arbitrary strategies.
    /// </summary>
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>, IStrategyzer<IGenerator<float[,]>>
    {

        #region Fields and Properties

        private readonly NoiseGenerator noiseGenerator;

        /// <summary>
        /// The noise map generation strategy.
        /// </summary>
        public IGenerator<float[,]> Strategy
        {
            get => noiseGenerator.Strategy;
            set => noiseGenerator.Strategy = value;
        }

        /// <summary>
        /// The scale of heights when generating a terrain mesh. The larger the scale, the higher the "mountains".
        /// </summary>
        public float HeightScale { get; set; }

        /// <summary>
        /// Defines what different values in generated noise maps mean
        /// when calculating vertex heights during the mesh generation.
        /// </summary>
        public AnimationCurve HeightCurve
        {
            get => heightCurve.Copy();
            set => heightCurve = value.Copy();
        }
        private AnimationCurve heightCurve;

        #endregion

        /// <summary>
        /// Initializes the terrain generator by setting its
        /// noise map generation strategy.
        /// </summary>
        /// <param name="noiseStrategy">The strategy of generating noise maps.</param>
        public TerrainGenerator(IGenerator<float[,]> noiseStrategy)
        {
            HeightScale = 1;
            heightCurve = new AnimationCurve();
            noiseGenerator = new NoiseGenerator(noiseStrategy);
        }
        
        /// <summary>
        /// Generates a terrain as a tuple consisting of a mesh and its texture
        /// </summary>
        /// <returns>The terrain tuple.</returns>
        public (Mesh, Texture2D) Generate()
        {
            var noiseMap = noiseGenerator.Generate();
            // TODO Generate Whittaker texture
            // var texture = GenerateWhittakerTexture(noiseMap);
            return (GenerateTerrainMesh(noiseMap), CreateNoiseTexture(noiseMap));
        }

        #region Helper methods

                private Mesh GenerateTerrainMesh(float[,] noiseMap)
        {
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);

            // There are as many vertices as there are points in the noise map
            var numVertices = width * height;
            var vertices = new Vector3[numVertices];
            var textureCoordinates = new Vector2[numVertices];
            
            // A mesh is essentially a set of triangles forming an entity. For each vertex of the mesh,
            // except for the ones to the far right and at the very bottom, we add two triangles to create the shape. 
            // Since each triangle consists of three points, this results in six points per relevant vertex.
            // This explains the size of the "triangles" array.
            
            /*
             * Example: 4 * 3 = 12 vertices, meaning 6 * 3 * 2 = 36 triangle points (12 triangles)
             * 
             * 0 - 1 - 2 - 3
             * | \ | \ | \ |
             * 4 - 5 - 6 - 7
             * | \ | \ | \ |
             * 8 - 9 - 10-11
             */
            
            var triangles = new int[6 * (width - 1) * (height - 1)];

            // Calculate the mesh data based on the height scale, the height curve and the noise map
            var vertexIndex = 0;
            var trianglePointIndex = 0;
            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < height; z++)
                {
                    // Add the vertices (in Unity, y is vertical)
                    var y = HeightScale * heightCurve.Evaluate(noiseMap[x, z]);
                    vertices[vertexIndex] = new Vector3(x, y, z);
                    
                    // Calculate the texture coordinate
                    textureCoordinates[vertexIndex] = new Vector2(x / (float) width, z / (float) height);

                    // Two triangles per vertex. Their points are added in a clockwise order and are based on
                    // the current vertex
                    if (x < width - 1 && y < height - 1)
                    {
                        // First triangle
                        triangles[trianglePointIndex++] = vertexIndex;
                        triangles[trianglePointIndex++] = vertexIndex + width + 1;
                        triangles[trianglePointIndex++] = vertexIndex + width;
                        
                        // Second triangle
                        triangles[trianglePointIndex++] = vertexIndex + width + 1;
                        triangles[trianglePointIndex++] = vertexIndex;
                        triangles[trianglePointIndex++] = vertexIndex + 1;
                    }

                    vertexIndex++;
                }
            }

            // Return a mesh based on the calculated data
            var mesh = new Mesh()
            {
                vertices = vertices,
                uv = textureCoordinates,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            return mesh;
        }
        
        // TODO: Implement
        private Texture2D GenerateWhittakerTexture(float[,] noiseMap)
        {
            throw new System.NotImplementedException();
        }

        // TODO: Remove this method (it's temporary and used for testing)
        #region Temporary (Should be removed)
        
        private static Texture2D CreateNoiseTexture(float[,] noiseMap)
        {
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);
            
            // Interpolate between black and white based on the noise map's values
            var pixelColors = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    pixelColors[x * height + y] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                }
            }

            return CreateColoredTexture(pixelColors, width, height);
        }
        
        private static Texture2D CreateColoredTexture(Color[] pixelColors, int width, int height)
        {
            var texture = new Texture2D(width, height);
            texture.SetPixels(pixelColors);
            texture.Apply();
            return texture;
        }

        #endregion

        #endregion

    }
}