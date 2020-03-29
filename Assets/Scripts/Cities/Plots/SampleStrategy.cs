using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Plots
{
    internal class SampleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {

        public SampleStrategy(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            var plots = GetPolygons().Select(polygon => new Plot(polygon));
            
            /*
            var plotStrings = plots.Select(plot =>
            {
                const string arrow = " -> ";
                var plotString = plot.Vertices.Aggregate("Plot: ", (current, vector) => current + (vector + arrow));
                return plotString.Substring(0, plotString.Length - arrow.Length);
            });

            foreach (var plotString in plotStrings)
            {
                Debug.Log(plotString);
            }
            */

            return plots;
        }

        // Gets all minimal polygons in the XZ-plane where the road network's XZ-projection intersections are found.
        private IEnumerable<IReadOnlyCollection<Vector3>> GetPolygons()
        {
            // In 3D, a plot with a "floating" road above it will thus be split in two to avoid a building colliding
            // with it.
        
            // Project the road network to the XZ-plane and get is as an undirected graph
            // in order to get access to all XZ-intersections and make it possible to find all polygons,
            // contiguous or not.
            var roadNetwork = Injector.Get().GetXZProjection().GetAsUndirected();
            
            var allPolygons = new List<IReadOnlyCollection<Vector3>>();
            foreach (var vertex in roadNetwork.RoadVertices)
            {
                if (TryGetPolygons(roadNetwork, vertex, out var polygons))
                {
                    allPolygons.AddRange(polygons);
                }
            }
            allPolygons.Sort((polygon1, polygon2) => 
                polygon1.Count < polygon2.Count ? -1 : polygon1.Count > polygon2.Count ? 1 : 0);

            var numberOfPolygons = roadNetwork.VertexCount - 2;
            var resultingPolygons = new List<IReadOnlyCollection<Vector3>>(numberOfPolygons);
            foreach (var polygon in allPolygons)
            {
                if (resultingPolygons.Count == numberOfPolygons)
                    break;
                
                if (!resultingPolygons.Any(polygon.ContainsAll))
                    resultingPolygons.Add(polygon);
            }
            
            return resultingPolygons;
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