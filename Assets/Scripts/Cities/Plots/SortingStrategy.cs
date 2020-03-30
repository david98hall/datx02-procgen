﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Plots
{
    /// <summary>
    /// Generates plots based on a road network by sorting enclosed plots by their vertex counts and then extracts
    /// them in order until all plots are found.
    /// </summary>
    internal class SortingStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {

        public SortingStrategy(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            // TODO Generate plots along roads, i.e., where there are no existing polygons
            
            return GetPolygons()
                .Where(p => p.Count > 2)
                .Select(polygon => new Plot(polygon));
        }

        // Gets all minimal polygons in the XZ-plane where the road network's XZ-projection intersections are found.
        private IEnumerable<IReadOnlyCollection<Vector3>> GetPolygons()
        {
            var roadNetwork = Injector.Get().GetXZProjection().GetAsUndirected();

            if (roadNetwork.VertexCount - 2 < 1) 
                return new List<IReadOnlyCollection<Vector3>>();
            
            var polygonTasks = new Task[roadNetwork.VertexCount];
            var i = 0;
            foreach (var vertex in roadNetwork.RoadVertices)
            {
                polygonTasks[i] = Task.Run(() =>
                    TryGetPolygons(roadNetwork, vertex, out var polygons)
                        ? polygons
                        : new List<IReadOnlyCollection<Vector3>>());
                i++;
            }

            Task.WaitAll(polygonTasks);
            var allPolygons = polygonTasks
                .SelectMany(task => ((Task<IReadOnlyCollection<IReadOnlyCollection<Vector3>>>)task).Result)
                .Where(p => p.Any())
                .ToList();

            allPolygons.Sort((polygon1, polygon2) => 
                polygon1.Count < polygon2.Count ? -1 : polygon1.Count > polygon2.Count ? 1 : 0);

            Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
            
            var resultingPolygons = new LinkedList<IReadOnlyCollection<Vector3>>();
            foreach (var polygon in allPolygons)
            {
                var traceCenters = Maths2D.GetRayCastPolygonCenters(polygon.Select(Vec3ToVec2), 1f);
                var isOverlapping = resultingPolygons.Any(resultingPolygon =>
                {
                    foreach (var centerPoint in traceCenters)
                    {
                        Debug.Log(centerPoint);
                        if (Maths2D.IsInsidePolygon(centerPoint, resultingPolygon.Select(Vec3ToVec2)))
                            return true;
                    }

                    return false;
                });

                if (!isOverlapping && !resultingPolygons.Any(polygon.ContainsAll))
                    resultingPolygons.AddLast(polygon);
            }
            
            return resultingPolygons; 
        }

        private static IEnumerable<(Vector2 Start, Vector2 End)> GetPolygonEdgesXz(IReadOnlyCollection<Vector3> polygon)
        {
            var polygonsCopy = new LinkedList<Vector3>(polygon);
            var first = polygonsCopy.First.Value;
            polygonsCopy.RemoveFirst();
            polygonsCopy.AddLast(first);
            return polygon.Zip(polygonsCopy, (v1, v2) => (new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z)));
        }
        
        private static IEnumerable<(Vector3 Start, Vector3 End)> GetPolygonEdges(IReadOnlyCollection<Vector3> polygon)
        {
            var polygonsCopy = new LinkedList<Vector3>(polygon);
            var first = polygonsCopy.First.Value;
            polygonsCopy.RemoveFirst();
            polygonsCopy.AddLast(first);
            return polygon.Zip(polygonsCopy, (v1, v2) => (v1, v2));
        }
        
        private static bool TryGetPolygons(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            out IReadOnlyCollection<IReadOnlyCollection<Vector3>> polygons)
        {
            polygons = GetPolygons(
                roadNetwork, 
                startVertex,
                startVertex,
                new HashSet<(Vector3 Start, Vector3 End)>());
            return polygons != null;
        }
        
        private static IReadOnlyCollection<IReadOnlyCollection<Vector3>> GetPolygons(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            Vector3 currentVertex,
            IEnumerable<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            var localVisitedEdges = new HashSet<(Vector3 Start, Vector3 End)>(visitedEdges);

            var neighbours = roadNetwork
                .GetAdjacentVertices(currentVertex)
                .Where(n => !localVisitedEdges.Contains((currentVertex, n)))
                .ToList();

            var polygons = new List<IReadOnlyCollection<Vector3>>(neighbours.Count);
            
            foreach (var neighbour in neighbours)
            {
                if (neighbour.Equals(startVertex))
                {
                    polygons.Add(new []{currentVertex, startVertex});
                }
                
                localVisitedEdges.Add((currentVertex, neighbour));

                var pathExtensions = GetPolygons(
                    roadNetwork, startVertex, neighbour, localVisitedEdges);
                
                if (pathExtensions == null)
                    continue;
                
                foreach (var pathExtension in pathExtensions)
                {
                    if (GetPolygonEdges(pathExtension).Contains((neighbour, currentVertex))) continue;
                    
                    var polygonPath = new LinkedList<Vector3>();
                    polygonPath.AddLast(currentVertex);
                    polygonPath.AddRange(pathExtension);
                    polygons.Add(polygonPath);
                }
            }

            if (polygons.Any())
            {
                polygons.Sort((extension1, extension2) => 
                    extension1.Count < extension2.Count ? -1 : extension1.Count > extension2.Count ? 1 : 0);
                return polygons;
            }

            return null;
        }

    }
}