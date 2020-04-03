using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Plots
{
    /// <summary>
    /// Finds cyclic plots by always turning clockwise when searching the road network.
    /// </summary>
    public class ClockwiseCycleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        public ClockwiseCycleStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        public override IEnumerable<Plot> Generate()
        {
            // DebugStuff();
            return GetMinimalCyclesXz().Select(cycle => new Plot(cycle));
        }

        private static void DebugStuff()
        {
            Debug.Log(Vector2.SignedAngle(new Vector2(1, 0), new Vector2(0, 1)));
            Debug.Log(Vector2.SignedAngle(new Vector2(1, 0), new Vector2(0, -1)));
        }
        
        private IEnumerable<IReadOnlyCollection<Vector3>> GetMinimalCyclesXz()
        {
            // XZ-projection of the undirected road network
            var roadNetwork = Injector.Get().GetXZProjection().GetAsUndirected();

            // If there aren't at least three vertices in the road network, there can't possibly be a cycle in it
            if (roadNetwork.VertexCount < 3) 
                return new List<IReadOnlyCollection<Vector3>>();

            // Find all minimal cycles in the road network (if there are any)
            var cycles = new HashSet<IReadOnlyCollection<Vector3>>();
            var visitedEdges = new HashSet<(Vector3 Start, Vector3 End)>();
            foreach (var vertex in roadNetwork.RoadVertices)
            {
                if (TryFindCycle(vertex, roadNetwork, visitedEdges, out var cycle))
                {
                    cycles.Add(cycle);
                }
            }
            
            return cycles;
        }

        private static bool TryFindCycle(
            Vector3 vertex, 
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out IReadOnlyCollection<Vector3> cycle)
        {
            cycle = FindCycle(vertex, vertex, roadNetwork, visitedEdges);
            return cycle != null;
        }

        private static IReadOnlyCollection<Vector3> FindCycle(
            Vector3 startVertex,
            Vector3 vertex, 
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            bool firstIteration = true)
        {
            if (!firstIteration && startVertex.Equals(vertex))
                return new[] {vertex};
            
            if (!TryGetNeighboursClockwise(vertex, roadNetwork, visitedEdges, out var neighbours))
                return null;

            foreach (var neighbour in neighbours)
            {
                Debug.Log((vertex, neighbour));
                
                // Mark the edge as visited
                visitedEdges.Add((vertex, neighbour));
                
                // Find the end of the cycle (if there is one)
                var cycleEnd = FindCycle(startVertex, neighbour, roadNetwork, visitedEdges, false);
                
                if (cycleEnd != null)
                {
                    Debug.Log("Hej");
                    
                    // A cycle was found!
                    var cyclePath = new LinkedList<Vector3>();
                    cyclePath.AddLast(vertex);
                    cyclePath.AddRange(cycleEnd);
                    return cyclePath;
                }
            }

            // No cycle was found
            return null;
        }

        private static bool TryGetNeighboursClockwise(
            Vector3 vertex, 
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out IEnumerable<Vector3> neighbours)
        {
            neighbours = null;
            
            var clockwiseNeighbours = GetNeighboursClockwise(vertex, roadNetwork);
            if (clockwiseNeighbours == null)
                return false;

            var neighboursList = new LinkedList<Vector3>();
            foreach (var neighbour in clockwiseNeighbours)
            {
                if (!visitedEdges.Contains((vertex, neighbour)))
                {
                    neighboursList.AddLast(neighbour);
                }
            }

            neighbours = neighboursList;
            return neighbours.Any();
        }
        
        private static Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
        
        private static IEnumerable<Vector3> GetNeighboursClockwise(
            Vector3 vertex, 
            RoadNetwork roadNetwork)
        {
            var neighbours = new HashSet<(Vector3 Vertex, float Angle)>();
            
            var vector = Vec3ToVec2(vertex);
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var neighbourVector = Vec3ToVec2(neighbour) - vector;
                var neighbourAngle = Vector2.SignedAngle(Vector3.right, neighbourVector);
                if (-90 < neighbourAngle && neighbourAngle < 90)
                {
                    neighbours.Add((neighbour, neighbourAngle));
                }
            }
            
            var clockwiseNeighbours = new List<(Vector3 Vertex, float Angle)>(neighbours);
            clockwiseNeighbours.Sort((vertex1, vertex2) =>
            {
                var clockwiseDiff1 = Math.Abs(vertex1.Angle + 90);
                var clockwiseDiff2 = Math.Abs(vertex2.Angle + 90);
                return clockwiseDiff1 < clockwiseDiff2 ? -1 : clockwiseDiff1 > clockwiseDiff2 ? 1 : 0;
            });
            
            return clockwiseNeighbours.Any() ? clockwiseNeighbours.Select(tuple => tuple.Vertex) : null;
        }
        
    }
}