using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Utils.Roads
{
    public class RoadObjectGenerator
    {
        public float RoadWidth { get; set; } = 0.3f;
        public float SmoothingFactor { get; set; } = 0.2f;
        public int SmoothingIterations { get; set; } = 3;
        public Material Material { get; set; }
        public float OffsetY { get; set; } = 0.6f;

        public RoadObjectGenerator(Material material)
        {
            Material = material;
        }

        public GameObject GenerateRoads(
            IEnumerable<IEnumerable<Vector3>> roads, 
            MeshFilter terrainMeshFilter, 
            Collider terrainCollider,
            string roadNetworkName = "Road Network")
        {
            var roadNetworkObj = new GameObject(roadNetworkName);
            roadNetworkObj.transform.SetParent(terrainMeshFilter.transform);  
            
            var count = 1;
            foreach (var road in roads)
            {
                var roadObj = GenerateRoad(road, terrainMeshFilter, terrainCollider, "Road " + count);
                roadObj.transform.SetParent(roadNetworkObj.transform);                
                count++;
            }

            return roadNetworkObj;
        }
        
        public GameObject GenerateRoad(
            IEnumerable<Vector3> points, MeshFilter terrainMeshFilter, Collider terrainCollider, string name = "Road")
        {
            var pointList = new List<Vector3>(points);
            
            // parameters validation
            if (pointList.Count < 2)
                throw new Exception("At least two points are required to make a road");
            if (SmoothingFactor < 0.0f || SmoothingFactor > 0.5f)
                throw new Exception("Smoothing factor should be between 0 and 0.5");

            if (SmoothingFactor > 0.0f) {
                for (var smoothingPass = 0; smoothingPass < SmoothingIterations; smoothingPass++) {
                    AddSmoothingPoints(pointList);
                }
            }

            // Replace the y-coordinate of every point with the height of the terrain (+ an offset)
            AdaptPointsToTerrainHeight(pointList, terrainMeshFilter);

            var mesh = new Mesh {name = name + " Mesh"};

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            Vector3 perpendicularDirection;
            Vector3 nextPoint, nextNextPoint;
            Vector3 point1, point2;
            Vector3 cornerPoint1, cornerPoint2;
            Vector3 tangent;
            Vector3 cornerNormal;
            
            var idx = 0;
            foreach (var currentPoint in pointList) {
                if (idx == pointList.Count - 1) {
                    // no need to do anything in the last point, all triangles
                    // have been created in previous iterations
                    break;
                }

                if (idx == pointList.Count - 2) {
                    // second to last point, we need to make up a "next next point"
                    nextPoint = pointList[idx + 1];
                    // assuming the 'next next' imaginary segment has the same
                    // direction as the real last one
                    nextNextPoint = nextPoint + (nextPoint - currentPoint);
                } else {
                    nextPoint = pointList[idx + 1];
                    nextNextPoint = pointList[idx + 2];
                }

                // Calculate the actual normals
                var ray = new Ray(currentPoint + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit1, 100.0f);
                var terrainNormal1 = hit1.normal;

                ray = new Ray(nextPoint + Vector3.up, Vector3.down);
                terrainCollider.Raycast(ray, out var hit2, 100.0f);
                var terrainNormal2 = hit2.normal;

                // calculate the normal to the segment, so we can displace 'left' and 'right' of
                // the point by half the road width and create our first vertices there
                perpendicularDirection = Vector3.Cross(terrainNormal1, nextPoint - currentPoint).normalized;
                point1 = currentPoint + perpendicularDirection * (RoadWidth * 0.5f);
                point2 = currentPoint - perpendicularDirection * (RoadWidth * 0.5f);

                // here comes the tricky part...
                // we calculate the tangent to the corner between the current segment and the next
                tangent = ((nextNextPoint - nextPoint).normalized + (nextPoint - currentPoint).normalized).normalized;
                cornerNormal = (Vector3.Cross(terrainNormal2, tangent)).normalized;
                // project the normal line to the corner to obtain the correct length
                var cornerWidth = RoadWidth * 0.5f / Vector3.Dot(cornerNormal, perpendicularDirection);
                cornerPoint1 = nextPoint + cornerWidth * cornerNormal;
                cornerPoint2 = nextPoint - cornerWidth * cornerNormal;

                // first point has no previous vertices set by past iterations
                if (idx == 0) {
                    vertices.Add(point1);
                    vertices.Add(point2);
                }
                vertices.Add(cornerPoint1);
                vertices.Add(cornerPoint2);

                var doubleIdx = idx * 2;

                // add first triangle
                triangles.Add(doubleIdx);
                triangles.Add(doubleIdx + 1);
                triangles.Add(doubleIdx + 2);

                // add second triangle
                triangles.Add(doubleIdx + 3);
                triangles.Add(doubleIdx + 2);
                triangles.Add(doubleIdx + 1);

                idx++;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, GenerateUVs(vertices));
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return CreateGameObject(mesh, name);
        }

        private void AddSmoothingPoints(IList<Vector3> points)
        {
            for (var i = 0; i < points.Count - 2; i++) {
                var currentPoint = points[i];
                var nextPoint = points[i + 1];
                var nextNextPoint = points[i + 2];

                var distance1 = Vector3.Distance(currentPoint, nextPoint);
                var distance2 = Vector3.Distance(nextPoint, nextNextPoint);

                var dir1 = (nextPoint - currentPoint).normalized;
                var dir2 = (nextNextPoint - nextPoint).normalized;

                points.RemoveAt(i + 1);
                points.Insert(i + 1, currentPoint + dir1 * (distance1 * (1.0f - SmoothingFactor)));
                points.Insert(i + 2, nextPoint + dir2 * (distance2 * SmoothingFactor));
                i++;
            }
        }

        private void AdaptPointsToTerrainHeight(IList<Vector3> points, MeshFilter terrainMeshFilter)
        {
            var heightMap = terrainMeshFilter.sharedMesh.HeightMap();
            for (var i = 0; i < points.Count; i++) {
                var point = points[i];
                var y = terrainMeshFilter.transform.position.y
                        + heightMap[(int)(0.5f + point.x), (int)(0.5f + point.z)]
                        + OffsetY;
                points[i] = new Vector3(point.x, y, point.z);
            }
        }

        private GameObject CreateGameObject(Mesh mesh, string name) {
            var obj = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            obj.GetComponent<MeshFilter>().mesh = mesh;

            var renderer = obj.GetComponent<MeshRenderer>();
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++) {
                materials[i] = Material;
            }
            renderer.sharedMaterials = materials;

            return obj;
        }

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