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
                    allPolygons.Add(polygon);
                }
            }

            return allPolygons;
        }
        
        // Returns false if a polygon was not found
        private static bool TryGetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges, 
            out IReadOnlyCollection<Vector3> polygon)
        {
            polygon = GetPolygon(roadNetwork, startVertex, Vector3.negativeInfinity, startVertex, visitedEdges);
            return polygon != null;
        }

        // Looks for a minimal polygon in the road network's XZ-projection.
        private static LinkedList<Vector3> GetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            Vector3 previousVertex,
            Vector3 currentVertex,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            if (TryGetUnvisitedRightmostNeighbour(
                roadNetwork, currentVertex, visitedEdges, out var rightmostNeighbour))
            {

                if (previousVertex.Equals(rightmostNeighbour)) 
                    return null;
                
                var edge = (currentVertex, rightmostNeighbour);
                visitedEdges.Add(edge);

                var polygonPath = new LinkedList<Vector3>();
                polygonPath.AddLast(currentVertex);

                if (startVertex.Equals(rightmostNeighbour))
                {
                    // The rightmost neighbour is equal to the start vertex of the polygon.
                    // The polygon has thus been found; do not keep looking for more vertices.
                    polygonPath.AddLast(rightmostNeighbour);
                }
                else
                {
                    // The rightmost neighbour does not equal the start vertex of the potential polygon and
                    // the search for vertices thus has to continue in order to potentially find a polygon.
                    var pathExtension = GetPolygon(
                        roadNetwork, startVertex, currentVertex, rightmostNeighbour, visitedEdges);

                    if (pathExtension == null)
                        return null;
                    
                    polygonPath.AddRange(pathExtension);
                }

                return polygonPath;
            }

            return null;
        }
        
        // Returns true if a rightmost vertex is found that has not been visited from the given vertex before.
        private static bool TryGetUnvisitedRightmostNeighbour(
            RoadNetwork roadNetwork, 
            Vector3 vertex,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out Vector3 rightmost)
        {
            rightmost = Vector3.negativeInfinity;
            
            // Find the rightmost neighbour since we always turn right when arriving at
            // a new vertex to close a potential polygon as quick as possible.
            var vertexXz = new Vector2(vertex.x, vertex.z);
            var foundRightmost = false;
            var minAngle = float.MaxValue;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var angleToNeighbour = vertexXz.TurningDegreesTo(new Vector2(neighbour.x, neighbour.z)) % 360;
                
                if (angleToNeighbour < minAngle && !visitedEdges.Contains((vertex, neighbour)))
                {
                    // Update the rightmost neighbour and the minimum angle so far
                    minAngle = angleToNeighbour;
                    rightmost = neighbour;
                    foundRightmost = true;
                }
            }

            return foundRightmost;
        }


        
    }
}