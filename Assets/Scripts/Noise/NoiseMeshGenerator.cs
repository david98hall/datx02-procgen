using System;
using Extensions;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Noise
{
    /// <summary>
    /// Generates meshes based on noise maps.
    /// </summary>
    internal class NoiseMeshGenerator : IGenerator<Mesh>
    {

        /// <summary>
        /// The scale of heights when generating a terrain mesh. The larger the scale, the higher the "mountains".
        /// </summary>
        internal float HeightScale { get; set; }

        /// <summary>
        /// Defines what different values in generated noise maps mean
        /// when calculating vertex heights during the mesh generation.
        /// </summary>
        internal AnimationCurve HeightCurve
        {
            get => _heightCurve.Copy();
            set => _heightCurve = value.Copy();
        }
        private AnimationCurve _heightCurve;
        
        private readonly IInjector<float[,]> _noiseMapInjector;
        
        /// <summary>
        /// Initializes this generator with a noise map injector.
        /// </summary>
        /// <param name="noiseMapInjector">The noise map injector.</param>
        internal NoiseMeshGenerator([NotNull] IInjector<float[,]> noiseMapInjector)
        {
            _noiseMapInjector = noiseMapInjector;
            HeightScale = 1;
            _heightCurve = new AnimationCurve();
        }
        
        public Mesh Generate()
        {
            if (_noiseMapInjector.Get() == null)
            {
                throw new NullReferenceException("The noise map is not set!");
            }

            var noiseMap = _noiseMapInjector.Get();
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);

            if (!(width > 0 && height > 0))
            {
                throw new IndexOutOfRangeException("Invalid width or height");
            }

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
            for (var z = 0; z < height; z++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Add the vertices (in Unity, y is vertical)
                    var y = HeightScale * _heightCurve.Evaluate(noiseMap[x, z]);
                    vertices[vertexIndex] = new Vector3(x, y, z);
                    
                    // Calculate the texture coordinate
                    textureCoordinates[vertexIndex] = new Vector2(x / (float) width, z / (float) height);

                    // Two triangles per relevant vertex.
                    // Their points are added in a counterclockwise order and are based on the current vertex
                    if (x < width - 1 && z < height - 1)
                    {
                        // First triangle
                        triangles[trianglePointIndex++] = vertexIndex + width;
                        triangles[trianglePointIndex++] = vertexIndex + width + 1;
                        triangles[trianglePointIndex++] = vertexIndex;
                        
                        // Second triangle
                        triangles[trianglePointIndex++] = vertexIndex + 1;
                        triangles[trianglePointIndex++] = vertexIndex;
                        triangles[trianglePointIndex++] = vertexIndex + width + 1;
                    }

                    vertexIndex++;
                }
            }

            // Return a mesh based on the calculated data
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = textureCoordinates
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}