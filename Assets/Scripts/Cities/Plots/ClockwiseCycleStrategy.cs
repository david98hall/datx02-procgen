using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

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

        public override IEnumerable<Plot> Generate() => GetMinimalCyclesXz().Select(cycle => new Plot(cycle));

        private IEnumerable<IReadOnlyCollection<Vector3>> GetMinimalCyclesXz()
        {
            var allCycles = new List<IReadOnlyCollection<Vector3>>(GetAllCyclesXz());
            allCycles.Sort((cycle1, cycle2) =>
            {
                var area1 = Maths2D.CalculatePolygonArea(cycle1.Select(Vec3ToVec2));
                var area2 = Maths2D.CalculatePolygonArea(cycle2.Select(Vec3ToVec2));
                return area1 < area2 ? -1 : area1 > area2 ? 1 : 0;
            });
            
            var minimalCycles = new HashSet<IReadOnlyCollection<Vector3>>();
            for (var i = allCycles.Count - 1; i >= 0; i--)
            {
                var isMinimal = true;
                var cycleXz = allCycles[i].Select(Vec3ToVec2).ToList();
                for (var j = i - 1; j >= 0; j--)
                {
                    if (!Maths2D.AnyPolygonCenterOverlaps(allCycles[j].Select(Vec3ToVec2), cycleXz)) 
                        continue;
                    isMinimal = false;
                    break;
                }

                if (isMinimal)
                    minimalCycles.Add(allCycles[i]);
            }

            return minimalCycles;
        }
        
        private IEnumerable<IReadOnlyCollection<Vector3>> GetAllCyclesXz()
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
            cycle = FindCycle(vertex, vertex, Vector3.zero, roadNetwork, visitedEdges);
            return cycle != null;
        }

        private static IReadOnlyCollection<Vector3> FindCycle(
            Vector3 start,
            Vector3 vertex,
            Vector3 previous,
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            bool firstIteration = true)
        {
            if (!firstIteration && start.Equals(vertex))
                return new []{vertex};

            if (TryGetClockwiseNeighbour(vertex, previous, roadNetwork, visitedEdges, out var neighbour))
            {
                var edge = (vertex, neighbour);
                if (visitedEdges.Contains(edge))
                    return null;

                // Mark the edge as visited
                visitedEdges.Add(edge);
                
                // Find the end of the cycle (if there is one)
                var cycleEnd = FindCycle(start, neighbour, vertex, roadNetwork, visitedEdges, false);
                
                if (cycleEnd != null)
                {
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

        private static Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
        
        private static bool TryGetClockwiseNeighbour(
            Vector3 vertex,
            Vector3 previous,
            RoadNetwork roadNetwork,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out Vector3 clockwiseNeighbour)
        {
            clockwiseNeighbour = Vector3.negativeInfinity;
            
            var vertexXz = Vec3ToVec2(vertex);
            var direction = vertexXz - Vec3ToVec2(previous);
            var maxAngle = float.MinValue;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var newDirection = Vec3ToVec2(neighbour) - vertexXz;
                var angle = Vector2.SignedAngle(direction, newDirection);
                var couldGoTo = !neighbour.Equals(previous) && !visitedEdges.Contains((vertex, neighbour));
                if (angle > maxAngle && couldGoTo)
                {
                    maxAngle = angle;
                    clockwiseNeighbour = neighbour;
                }
            }

            return !clockwiseNeighbour.Equals(Vector3.negativeInfinity);
        }
        
    }
}