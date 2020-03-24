using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Cities.Plots
{
    internal class PlotStrategySample : PlotStrategy
    {

        public PlotStrategySample([NotNull] IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            return GetPolygons().Select(polygon => new Plot(polygon));
        }

        private IEnumerable<IReadOnlyCollection<Vector3>> GetPolygons()
        {
            var allPolygons = new HashSet<IReadOnlyCollection<Vector3>>();
            
            var roadNetwork = RoadNetwork.GetXZProjection().GetAsUndirected();

            var visitedEdges = new HashSet<(Vector3 Start, Vector3 End)>();
            
            foreach (var vertex in roadNetwork.RoadVertices)
            {
                var polygon = new LinkedList<Vector3>();
                if (TryGetPolygon(roadNetwork, vertex, visitedEdges, polygon))
                {
                    allPolygons.Add(polygon);
                }
            }

            return allPolygons;
        }
        
        private static bool TryGetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVector,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges, 
            ICollection<Vector3> polygon)
        {
            polygon.Add(startVector);
            
            if (TryGetRightmostNeighbour(roadNetwork, startVector, out var rightmostNeighbour))
            {
                var edge = (startVector, rightmostNeighbour);

                if (!visitedEdges.Contains(edge))
                {
                    visitedEdges.Add(edge);
                    TryGetPolygon(roadNetwork, rightmostNeighbour, visitedEdges, polygon);
                    return true;
                }   
            }

            return false;
        }
        
        private static bool TryGetRightmostNeighbour(RoadNetwork roadNetwork, Vector3 vertex, out Vector3 rightmost)
        {
            rightmost = Vector3.negativeInfinity;
            
            // Find the rightmost neighbour since we always turn right when arriving at
            // a new vertex to close a potential polygon as quick as possible.
            var vertexXZ = new Vector2(vertex.x, vertex.z);
            var foundRightmost = false;
            var minAngle = float.MaxValue;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var angleToNeighbour = vertexXZ.DegreesTo(new Vector2(neighbour.x, neighbour.z));
                    
                if (!(angleToNeighbour < minAngle)) continue;
                    
                // Update the rightmost neighbour and the minimum angle so far
                minAngle = angleToNeighbour;
                rightmost = neighbour;
                foundRightmost = true;
            }

            return foundRightmost;
        }

    }
}