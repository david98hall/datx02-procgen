using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Roads
{
    public class Roadifier
    {
        public float RoadWidth { get; set; } = 5.0f;
        public float SmoothingFactor { get; set; } = 0.2f;
        public int SmoothingIterations { get; set; } = 3;
        public Material Material { get; set; }
        public float TerrainClearance { get; set; } = 0.05f;

        public Roadifier(Material material)
        {
            Material = material;
        }

        public GameObject GenerateRoad(IEnumerable<Vector3> points, UnityEngine.Terrain terrain = null)
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

            // if a terrain parameter was specified, replace the y-coordinate
            // of every point with the height of the terrain (+ an offset)
            if (terrain) {
                AdaptPointsToTerrainHeight(pointList, terrain);
            }

            Vector3 perpendicularDirection;
            Vector3 nextPoint, nextNextPoint;
            Vector3 point1, point2;
            Vector3 cornerPoint1, cornerPoint2;
            Vector3 tangent;
            Vector3 cornerNormal;

            var mesh = new Mesh {name = "Road Mesh"};

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

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

                var terrainNormal1 = Vector3.up; // default normal: straight up
                var terrainNormal2 = Vector3.up; // default normal: straight up
                if (terrain) {
                    // if there's a terrain, calculate the actual normals
                    var ray = new Ray(currentPoint + Vector3.up, Vector3.down);
                    terrain.GetComponent<Collider>().Raycast(ray, out var hit, 100.0f);
                    terrainNormal1 = hit.normal;

                    ray = new Ray(nextPoint + Vector3.up, Vector3.down);
                    terrain.GetComponent<Collider>().Raycast(ray, out hit, 100.0f);
                    terrainNormal2 = hit.normal;
                }

                // calculate the normal to the segment, so we can displace 'left' and 'right' of
                // the point by half the road width and create our first vertices there
                perpendicularDirection = (Vector3.Cross(terrainNormal1, nextPoint - currentPoint)).normalized;
                point1 = currentPoint + perpendicularDirection * (RoadWidth * 0.5f);
                point2 = currentPoint - perpendicularDirection * (RoadWidth * 0.5f);

                // here comes the tricky part...
                // we calculate the tangent to the corner between the current segment and the next
                tangent = ((nextNextPoint - nextPoint).normalized + (nextPoint - currentPoint).normalized).normalized;
                cornerNormal = (Vector3.Cross(terrainNormal2, tangent)).normalized;
                // project the normal line to the corner to obtain the correct length
                var cornerWidth = (RoadWidth * 0.5f) / Vector3.Dot(cornerNormal, perpendicularDirection);
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

            return CreateGameObject(mesh);
        }

        private void AddSmoothingPoints(IEnumerable<Vector3> points)
        {
            var pointList = new List<Vector3>(points);
            
            for (var i = 0; i < pointList.Count - 2; i++) {
                var currentPoint = pointList[i];
                var nextPoint = pointList[i + 1];
                var nextNextPoint = pointList[i + 2];

                var distance1 = Vector3.Distance(currentPoint, nextPoint);
                var distance2 = Vector3.Distance(nextPoint, nextNextPoint);

                var dir1 = (nextPoint - currentPoint).normalized;
                var dir2 = (nextNextPoint - nextPoint).normalized;

                pointList.RemoveAt(i + 1);
                pointList.Insert(i + 1, currentPoint + dir1 * (distance1 * (1.0f - SmoothingFactor)));
                pointList.Insert(i + 2, nextPoint + dir2 * (distance2 * SmoothingFactor));
                i++;
            }
        }

        private void AdaptPointsToTerrainHeight(IEnumerable<Vector3> points, UnityEngine.Terrain terrain)
        {
            var pointList = new List<Vector3>(points);
            
            for (var i = 0; i < pointList.Count; i++) {
                var point = pointList[i];
                var y = terrain.transform.position.y 
                        + TerrainClearance 
                        + terrain.SampleHeight(new Vector3(point.x, 0, point.z));
                pointList[i] = new Vector3(point.x, y, point.z);
            }
        }

        private GameObject CreateGameObject(Mesh mesh) {
            var obj = new GameObject("Road", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            obj.GetComponent<MeshFilter>().mesh = mesh;
            // obj.transform.SetParent(transform);

            var renderer = obj.GetComponent<MeshRenderer>();
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++) {
                materials[i] = Material;
            }
            renderer.materials = materials;

            return obj;
        }

        private List<Vector2> GenerateUVs(IEnumerable<Vector3> vertices) {
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