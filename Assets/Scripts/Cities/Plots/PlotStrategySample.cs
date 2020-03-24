using System.Collections;
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
                if (TryGetPolygon(roadNetwork, vertex, visitedEdges, out var polygon))
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
            out IReadOnlyCollection<Vector3> polygon)
        {
            polygon = GetPolygon(roadNetwork, startVector, visitedEdges);
            return polygon != null;
        }
        
        private static IReadOnlyCollection<Vector3> GetPolygon(
            RoadNetwork roadNetwork,
            Vector3 startVector,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            if (TryGetRightmostNeighbour(roadNetwork, startVector, out var rightmostNeighbour))
            {
                var edge = (startVector, rightmostNeighbour);
                
                if (visitedEdges.Contains(edge)) return null;
                visitedEdges.Add(edge);
             
                var polygonPath = new LinkedList<Vector3>();
                polygonPath.AddLast(startVector);
                polygonPath.AddLast(rightmostNeighbour);
                
                var polygonPathExtension = GetPolygon(roadNetwork, rightmostNeighbour, visitedEdges);
                if (polygonPathExtension != null)
                {
                    polygonPath.AddRange(polygonPathExtension);
                }
                return polygonPath;
            }

            return null;
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