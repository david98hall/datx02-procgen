using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Plots
{
    internal class SampleStrategy2 : Strategy<RoadNetwork, IEnumerable<Plot>>
    {

        public SampleStrategy2(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            var plots = GetPolygons().Select(polygon => new Plot(polygon));
            
            // TODO Remove debug log
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

            var allPolygons = new HashSet<IReadOnlyCollection<Vector3>>();
            var visitedEdges = new HashSet<(Vector3 Start, Vector3 End)>();
            foreach (var vertex in roadNetwork.RoadVertices)
            {
                if (TryGetPolygon(roadNetwork, vertex, visitedEdges, out var polygon))
                {
                    var polygonEdges = GetPolygonEdges(polygon).ToList();

                    if (!polygonEdges.Intersect(visitedEdges).Any() && !PolygonExists(polygon, allPolygons))
                    {
                        allPolygons.Add(polygon);
                        visitedEdges.AddRange(polygonEdges);
                    }
                }
            }

            return allPolygons;
        }

        private static bool PolygonExists(
            IReadOnlyCollection<Vector3> polygon, 
            IEnumerable<IReadOnlyCollection<Vector3>> allPolygons)
        {
            return allPolygons.Any(existingPolygon => existingPolygon.ContainsAll(polygon));
        }
        
        private static IEnumerable<(Vector3 Start, Vector3 End)> GetPolygonEdges(IReadOnlyCollection<Vector3> polygon)
        {
            var polygonsCopy = new LinkedList<Vector3>(polygon);
            var first = polygonsCopy.First.Value;
            polygonsCopy.RemoveFirst();
            polygonsCopy.AddLast(first);
            return polygon.Zip(polygonsCopy, (v1, v2) => (v1, v2));
        }

        // Returns false if a polygon was not found
        private static bool TryGetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            IReadOnlyCollection<(Vector3 Start, Vector3 End)> visitedEdges, 
            out IReadOnlyCollection<Vector3> polygon)
        {
            polygon = GetPolygon(
                roadNetwork, startVertex, Vector2.negativeInfinity, startVertex, visitedEdges);
            return polygon != null;
        }

        // Looks for a minimal polygon in the road network's XZ-projection.
        private static LinkedList<Vector3> GetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            Vector3 previousVertex,
            Vector3 currentVertex,
            IEnumerable<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            var path = new LinkedList<Vector3>();
            path.AddLast(currentVertex);
            
            var localVisitedEdges = new HashSet<(Vector3 Start, Vector3 End)>(visitedEdges);

            IEnumerable<Vector3> minPathExtension = null;
            var minExtensionLength = float.MaxValue;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(currentVertex))
            {
                var edge = (currentVertex, neighbour);
                if (localVisitedEdges.Contains(edge))
                    continue;
                localVisitedEdges.Add(edge);

                LinkedList<Vector3> pathExtension;
                if (startVertex.Equals(neighbour) && !previousVertex.Equals(startVertex))
                {
                    pathExtension = new LinkedList<Vector3>();
                    pathExtension.AddLast(startVertex);
                }
                else
                {
                    pathExtension = GetPolygon(
                        roadNetwork, startVertex, currentVertex, neighbour, localVisitedEdges);
                }

                if (pathExtension != null && pathExtension.Count < minExtensionLength)
                {
                    minExtensionLength = pathExtension.Count;
                    minPathExtension = pathExtension;
                }

            }

            if (minPathExtension != null)
            {
                path.AddRange(minPathExtension);
                return path;
            }

            return null;
        }

    }
}