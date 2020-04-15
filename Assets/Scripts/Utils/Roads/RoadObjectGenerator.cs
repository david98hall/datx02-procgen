using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Utils.Roads
{
    /// <summary>
    /// Generates road game objects based on road vertices and a terrain.
    /// </summary>
    public class RoadObjectGenerator
    {
        /// <summary>
        /// The width of the generated road objects.
        /// </summary>
        public float RoadWidth { get; set; } = 0.3f;

        /// <summary>
        /// Determines how curved a road is between two points.
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
        /// How many times smoothing points should be added to a road.
        /// Determines how smooth the road will be between to points.
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
        /// The material of the roads this generator creates.
        /// </summary>
        public Material RoadMaterial { get; set; }
        
        /// <summary>
        /// The y-coordinate offset between the terrain and the roads.
        /// </summary>
        public float TerrainOffsetY { get; set; } = 0.075f;

        /// <summary>
        /// Initializes this generator by setting the road material.
        /// </summary>
        /// <param name="roadMaterial">The material to use when creating roads objects.</param>
        public RoadObjectGenerator(Material roadMaterial = null)
        {
            RoadMaterial = roadMaterial;
        }

        /// <summary>
        /// Generates a road network GameObject.
        /// </summary>
        /// <param name="roads">All the roads in the network.</param>
        /// <param name="terrainMeshFilter">The mesh filter of the terrain mesh.</param>
        /// <param name="terrainCollider">The mesh collider of the terrain mesh.</param>
        /// <param name="roadNetworkName">The name of the road network to generate.</param>
        /// <returns>The road network as a GameObject.</returns>
        public GameObject GenerateRoadNetwork(
            IEnumerable<IEnumerable<Vector3>> roads, 
            MeshFilter terrainMeshFilter, 
            Collider terrainCollider,
            string roadNetworkName = "Road Network")
        {
            var roadNetworkObj = new GameObject(roadNetworkName);
            
            var count = 1;
            foreach (var road in roads)
            {
                var roadObj = GenerateRoad(road, terrainMeshFilter, terrainCollider, "Road " + count);
                roadObj.transform.SetParent(roadNetworkObj.transform);                
                count++;
            }

            return roadNetworkObj;
        }

        /// <summary>
        /// Generates a road GameObject.
        /// </summary>
        /// <param name="roadVertices">The vertices along the road.</param>
        /// <param name="terrainMeshFilter">The mesh filter of the terrain mesh.</param>
        /// <param name="terrainCollider">The mesh collider of the terrain mesh.</param>
        /// <param name="name">The name of the road.</param>
        /// <returns>A road as a GameObject.</returns>
        /// <exception cref="Exception">
        /// If there are less than 2 vertices or if the SmoothingFactor is not an element of [0, 0.5].
        /// </exception>
        public GameObject GenerateRoad(
            IEnumerable<Vector3> roadVertices, 
            MeshFilter terrainMeshFilter, 
            Collider terrainCollider, 
            string name = "Road")
        {
            var roadVertexList = new List<Vector3>(roadVertices);
            
            // Validate parameters
            if (roadVertexList.Count < 2)
                throw new Exception("At least two points are required to make a road.");

            if (CurveFactor > 0.0f) {
                for (var smoothingPass = 0; smoothingPass < SmoothingIterations; smoothingPass++) {
                    AddSmoothingPoints(roadVertexList);
                }
            }

            // Replace the y-coordinate of every point with the height of the terrain (+ an offset)
            AdaptPointsToTerrainHeight(roadVertexList, terrainMeshFilter, terrainCollider);

            var mesh = new Mesh {name = name + " Mesh"};
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            
            for (var i = 0; i < roadVertexList.Count - 1; i++)
            {
                var currentPoint = roadVertexList[i];
                
                Vector3 nextPoint;
                Vector3 nextNextPoint;
                if (i == roadVertexList.Count - 2) 
                {
                    // second to last point, we need to make up a "next next point"
                    nextPoint = roadVertexList[i + 1];
                    // assuming the 'next next' imaginary segment has the same
                    // direction as the real last one
                    nextNextPoint = nextPoint + (nextPoint - currentPoint);
                } 
                else 
                {
                    nextPoint = roadVertexList[i + 1];
                    nextNextPoint = roadVertexList[i + 2];
                }

                // Calculate the actual normals
                var ray = new Ray(currentPoint + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit1, 100.0f);
                var terrainNormal1 = hit1.normal;

                ray = new Ray(nextPoint + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit2, 100.0f);
                var terrainNormal2 = hit2.normal;

                // Calculate the normal to the segment, so we can displace 'left' and 'right' of
                // the point by half the road width and create our first vertices there
                var perpendicularDirection = Vector3.Cross(terrainNormal1, nextPoint - currentPoint).normalized;
                var point1 = currentPoint + perpendicularDirection * (RoadWidth * 0.5f);
                var point2 = currentPoint - perpendicularDirection * (RoadWidth * 0.5f);

                // here comes the tricky part...
                // we calculate the tangent to the corner between the current segment and the next
                var tangent = ((nextNextPoint - nextPoint).normalized + (nextPoint - currentPoint).normalized).normalized;
                var cornerNormal = Vector3.Cross(terrainNormal2, tangent).normalized;
                // project the normal line to the corner to obtain the correct length
                var cornerWidth = RoadWidth * 0.5f / Vector3.Dot(cornerNormal, perpendicularDirection);
                var cornerPoint1 = nextPoint + cornerWidth * cornerNormal;
                var cornerPoint2 = nextPoint - cornerWidth * cornerNormal;

                // The first point has no previous vertices set by past iterations
                if (i == 0) 
                {
                    vertices.Add(point1);
                    vertices.Add(point2);
                }
                vertices.Add(cornerPoint1);
                vertices.Add(cornerPoint2);

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

            return CreateGameObject(mesh, name);
        }

        // Adds points to make the path between the given points smoother
        private void AddSmoothingPoints(IList<Vector3> points)
        {
            for (var i = 0; i < points.Count - 2; i++) 
            {
                var currentPoint = points[i];
                var nextPoint = points[i + 1];
                var nextNextPoint = points[i + 2];

                var distance1 = Vector3.Distance(currentPoint, nextPoint);
                var distance2 = Vector3.Distance(nextPoint, nextNextPoint);

                var dir1 = (nextPoint - currentPoint).normalized;
                var dir2 = (nextNextPoint - nextPoint).normalized;

                points.RemoveAt(i + 1);
                points.Insert(i + 1, currentPoint + dir1 * (distance1 * (1.0f - CurveFactor)));
                points.Insert(i + 2, nextPoint + dir2 * (distance2 * CurveFactor));
                i++;
            }
        }

        // Set the y-value of each given point to the y-value of the terrain height at same xz-position
        private void AdaptPointsToTerrainHeight(
            IList<Vector3> points, MeshFilter terrainMeshFilter, Collider terrainCollider)
        {
            const float rayLength = 100;
            var up = Vector3.up * rayLength / 2;
            var heightMap = terrainMeshFilter.sharedMesh.HeightMap();
            for (var i = 0; i < points.Count; i++)
            {
                var closeY = heightMap[(int) (0.5f + points[i].x), (int) (0.5f + points[i].z)];
                var point = new Vector3(points[i].x, closeY, points[i].z);

                // Use ray casting to find the y-position of the xz-point on to the terrain mesh
                var terrainY = float.NaN;
                if (terrainCollider.Raycast(new Ray(point + up, Vector3.down), out var hit1, rayLength))
                {
                    terrainY = hit1.point.y;
                } 
                else if (terrainCollider.Raycast(new Ray(point - up, Vector3.up), out var hit2, rayLength))
                {
                    terrainY = hit2.point.y;
                }

                // If a y-value was found, update the point
                if (!float.IsNaN(terrainY))
                {
                    var y = terrainMeshFilter.transform.position.y
                            + terrainY
                            + TerrainOffsetY;
                    points[i] = new Vector3(point.x, y, point.z);   
                }
            }
        }

        // Create a game object based on a mesh and give it a name
        private GameObject CreateGameObject(Mesh mesh, string name) {
            var obj = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            obj.GetComponent<MeshFilter>().mesh = mesh;

            if (RoadMaterial == null) return obj;
                
            // If there is a material, set it
            var renderer = obj.GetComponent<MeshRenderer>();
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++) 
            {
                materials[i] = RoadMaterial;
            }
            renderer.sharedMaterials = materials;

            return obj;
        }

        // Calculate the base texture coordinates of a mesh based on its vertices
        private static List<Vector2> GenerateUVs(IEnumerable<Vector3> vertices) {
            var uvs = new List<Vector2>();

            var vertexList = new List<Vector3>(vertices);
            
            for (var vertIdx = 0; vertIdx < vertexList.Count; vertIdx++)
            {
                switch (vertIdx % 4)
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