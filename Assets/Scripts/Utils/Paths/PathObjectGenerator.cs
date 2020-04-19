using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Utils.Paths
{
    /// <summary>
    /// Generates path game objects based on path vertices and a terrain.
    /// </summary>
    public class PathObjectGenerator
    {
        /// <summary>
        /// The width of the generated path objects.
        /// </summary>
        public float PathWidth { get; set; } = 0.3f;

        /// <summary>
        /// Determines how curved a path is between two vertices.
        /// This value has to be in the range [0, 0.5]. 
        /// </summary>
        public float CurveFactor
        {
            get => _curveFactor;
            set
            {
                if (value < 0.0f || value > 0.5f)
                    throw new ArgumentOutOfRangeException(
                        nameof(CurveFactor), "The curve factor has to be in the range [0, 0.5]");
                _curveFactor = value;
            }
        }
        private float _curveFactor = 0.1f;
        
        /// <summary>
        /// How many times smoothing vertices should be added to a path.
        /// Determines how smooth the path will be between two vertices.
        /// </summary>
        public int SmoothingIterations 
        { 
            get => _smoothingIterations;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(SmoothingIterations), "The number of smoothing iterations has to be positive!");
                _smoothingIterations = value;
            } 
        }
        private int _smoothingIterations = 3;
        
        /// <summary>
        /// The material of the paths this generator creates.
        /// </summary>
        public Material PathMaterial { get; set; }
        
        /// <summary>
        /// The y-coordinate offset between the terrain and the paths.
        /// </summary>
        public float TerrainOffsetY { get; set; } = 0.075f;

        /// <summary>
        /// Initializes this generator by setting the path material.
        /// </summary>
        /// <param name="pathMaterial">The material to use when creating paths objects.</param>
        public PathObjectGenerator(Material pathMaterial = null)
        {
            PathMaterial = pathMaterial;
        }
        
        /// <summary>
        /// Generates a path network GameObject.
        /// </summary>
        /// <param name="paths">All the paths in the network.</param>
        /// <param name="terrainMeshFilter">The mesh filter of the terrain mesh.</param>
        /// <param name="terrainCollider">The mesh collider of the terrain mesh.</param>
        /// <param name="pathNetworkName">The name of the path network to generate.</param>
        /// <param name="basePathName">
        /// The base name of each path. The path index will be added at the end of each path.
        /// </param>
        /// <returns>The path network as a GameObject.</returns>
        public GameObject GeneratePathNetwork(
            IEnumerable<IEnumerable<Vector3>> paths,
            MeshFilter terrainMeshFilter,
            Collider terrainCollider,
            string pathNetworkName = "Path Network",
            string basePathName = "Path")
        {
            return GeneratePathNetwork(
                paths.GetEnumerator(), 
                terrainMeshFilter, terrainCollider, 
                pathNetworkName,
                basePathName);
        }

        /// <summary>
        /// Generates a path network GameObject.
        /// </summary>
        /// <param name="paths">All the paths in the network.</param>
        /// <param name="terrainMeshFilter">The mesh filter of the terrain mesh.</param>
        /// <param name="terrainCollider">The mesh collider of the terrain mesh.</param>
        /// <param name="pathNetworkName">The name of the path network to generate.</param>
        /// <param name="basePathName">
        /// The base name of each path. The path index will be added at the end of each path.
        /// </param>
        /// <returns>The path network as a GameObject.</returns>
        public GameObject GeneratePathNetwork(
            IEnumerator<IEnumerable<Vector3>> paths, 
            MeshFilter terrainMeshFilter, 
            Collider terrainCollider,
            string pathNetworkName = "Path Network",
            string basePathName = "Path")
        {
            var pathNetworkObj = new GameObject(pathNetworkName);
            
            // Create a path object for each set of vertices
            var count = 1;
            while (paths.MoveNext())
            {
                var pathName = basePathName + " " + count;
                var pathObj = GeneratePath(paths.Current, terrainMeshFilter, terrainCollider, pathName);
                pathObj.transform.SetParent(pathNetworkObj.transform);                
                count++;
            }

            return pathNetworkObj;
        }

        /// <summary>
        /// Generates a path GameObject.
        /// </summary>
        /// <param name="pathVertices">The vertices along the path.</param>
        /// <param name="terrainMeshFilter">The mesh filter of the terrain mesh.</param>
        /// <param name="terrainCollider">The mesh collider of the terrain mesh.</param>
        /// <param name="name">The name of the path.</param>
        /// <returns>A path as a GameObject.</returns>
        /// <exception cref="Exception">
        /// If there are less than 2 vertices or if the SmoothingFactor is not an element of [0, 0.5].
        /// </exception>
        public GameObject GeneratePath(
            IEnumerable<Vector3> pathVertices, 
            MeshFilter terrainMeshFilter, 
            Collider terrainCollider, 
            string name = "Path")
        {
            var pathVertexList = new List<Vector3>(pathVertices);
            
            // Validate parameters
            if (pathVertexList.Count < 2)
                throw new Exception("At least two vertices are required to make a path!");

            // If the curve factor is greater than zero, add new vertices between the
            // existing vertices in order to make the path smoother looking
            if (CurveFactor > 0.0f) {
                for (var smoothingPass = 0; smoothingPass < SmoothingIterations; smoothingPass++) {
                    AddSmoothingVertices(pathVertexList);
                }
            }
            
            AdaptVerticesToTerrain(pathVertexList, terrainMeshFilter, terrainCollider);

            // Return the path game object
            var mesh = CreatePathMesh(pathVertexList, terrainCollider, name);
            return CreateGameObject(mesh, name);
        }

        // Creates a mesh along the given path vertices with an appearance based on certain properties
        private Mesh CreatePathMesh(IList<Vector3> pathVertices, Collider terrainCollider, string name)
        {
            var mesh = new Mesh {name = name + " Mesh"};
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            
            for (var i = 0; i < pathVertices.Count - 1; i++)
            {
                var currentVertex = pathVertices[i];
                
                Vector3 nextVertex;
                Vector3 nextNextVertex;
                if (i == pathVertices.Count - 2) 
                {
                    // Second to last vertex, we need to make up a "next next vertex"
                    nextVertex = pathVertices[i + 1];
                    
                    // Assuming the 'next next' imaginary segment has the same direction as the real last one
                    nextNextVertex = nextVertex + (nextVertex - currentVertex);
                } 
                else 
                {
                    nextVertex = pathVertices[i + 1];
                    nextNextVertex = pathVertices[i + 2];
                }

                // Calculate the actual normals
                var ray = new Ray(currentVertex + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit1, 100.0f);
                var terrainNormal1 = hit1.normal;

                ray = new Ray(nextVertex + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit2, 100.0f);
                var terrainNormal2 = hit2.normal;

                // Calculate the normal to the segment, so we can displace 'left' and 'right' of
                // the vertex by half the path width and create our first vertices there
                var perpendicularDirection = Vector3.Cross(terrainNormal1, nextVertex - currentVertex).normalized;
                var vertex1 = currentVertex + perpendicularDirection * (PathWidth * 0.5f);
                var vertex2 = currentVertex - perpendicularDirection * (PathWidth * 0.5f);

                // Calculate the tangent to the corner between the current segment and the next
                var tangent = ((nextNextVertex - nextVertex).normalized + (nextVertex - currentVertex).normalized).normalized;
                var cornerNormal = Vector3.Cross(terrainNormal2, tangent).normalized;
                
                // Project the normal line to the corner to obtain the correct length
                var cornerWidth = PathWidth * 0.5f / Vector3.Dot(cornerNormal, perpendicularDirection);
                var cornerVertex1 = nextVertex + cornerWidth * cornerNormal;
                var cornerVertex2 = nextVertex - cornerWidth * cornerNormal;

                // The first vertex has no previous vertices set by past iterations
                if (i == 0) 
                {
                    vertices.Add(vertex1);
                    vertices.Add(vertex2);
                }
                vertices.Add(cornerVertex1);
                vertices.Add(cornerVertex2);

                // Add first triangle
                var doubleI = i * 2;
                triangles.Add(doubleI);
                triangles.Add(doubleI + 1);
                triangles.Add(doubleI + 2);

                // Add second triangle
                triangles.Add(doubleI + 3);
                triangles.Add(doubleI + 2);
                triangles.Add(doubleI + 1);
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, GenerateUVs(vertices));
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }
        
        // Adds new vertices to make the path between the given vertices smoother
        private void AddSmoothingVertices(IList<Vector3> vertices)
        {
            for (var i = 0; i < vertices.Count - 2; i++) 
            {
                var currentVertex = vertices[i];
                var nextVertex = vertices[i + 1];
                var nextNextVertex = vertices[i + 2];

                var distance1 = Vector3.Distance(currentVertex, nextVertex);
                var distance2 = Vector3.Distance(nextVertex, nextNextVertex);

                var dir1 = (nextVertex - currentVertex).normalized;
                var dir2 = (nextNextVertex - nextVertex).normalized;

                vertices.RemoveAt(i + 1);
                vertices.Insert(i + 1, currentVertex + dir1 * (distance1 * (1.0f - CurveFactor)));
                vertices.Insert(i + 2, nextVertex + dir2 * (distance2 * CurveFactor));
                i++;
            }
        }

        // Sets the y-value of each given vertex to the y-value of the terrain height at same xz-position
        private void AdaptVerticesToTerrain(
            IList<Vector3> vertices, MeshFilter terrainMeshFilter, Collider terrainCollider)
        {
            const float rayLength = 100;
            const float halfRayLength = rayLength / 2;
            var up = Vector3.up * halfRayLength;
            var heightMap = terrainMeshFilter.sharedMesh.HeightMap();
            for (var i = 0; i < vertices.Count; i++)
            {
                // Get the vertex at an approximately correct y-level height
                var closeY = heightMap[(int) (0.5f + vertices[i].x), (int) (0.5f + vertices[i].z)];
                var vertex = new Vector3(vertices[i].x, closeY, vertices[i].z);

                // Use ray casting to find the y-position of the xz-vertex on to the terrain mesh
                var terrainY = float.NaN;
                if (terrainCollider.Raycast(new Ray(vertex + up, Vector3.down), out var hit1, rayLength))
                {
                    terrainY = hit1.point.y;
                } 
                else if (terrainCollider.Raycast(new Ray(vertex - up, Vector3.up), out var hit2, rayLength))
                {
                    terrainY = hit2.point.y;
                }

                // If a y-value was found, update the vertex
                if (!float.IsNaN(terrainY))
                {
                    var y = terrainMeshFilter.transform.position.y
                            + terrainY
                            + TerrainOffsetY;
                    vertices[i] = new Vector3(vertex.x, y, vertex.z);   
                }
            }
        }

        // Create a game object based on a mesh and give it a name
        private GameObject CreateGameObject(Mesh mesh, string name) {
            var obj = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            obj.GetComponent<MeshFilter>().mesh = mesh;

            if (PathMaterial == null) return obj;
                
            // If there is a material, set it
            var renderer = obj.GetComponent<MeshRenderer>();
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++) 
            {
                materials[i] = PathMaterial;
            }
            renderer.sharedMaterials = materials;

            return obj;
        }

        // Calculate the base texture coordinates of a mesh based on its vertices
        private static List<Vector2> GenerateUVs(IEnumerable<Vector3> vertices) {
            var uvs = new List<Vector2>();

            var vertexList = new List<Vector3>(vertices);
            
            for (var i = 0; i < vertexList.Count; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        uvs.Add(new Vector2(0, 0));
                        break;
                    case 1:
                        uvs.Add(new Vector2(0, 1));
                        break;
                    case 2:
                        uvs.Add(new Vector2(1, 0));
                        break;
                    default:
                        uvs.Add(new Vector2(1, 1));
                        break;
                }
            }
            
            return uvs;
        }
    }
}