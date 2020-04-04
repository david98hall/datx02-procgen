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
            DebugStuff();
            return GetMinimalCyclesXz().Select(cycle => new Plot(cycle));
        }

        private static void DebugStuff()
        {
            Debug.Log(Vector2.SignedAngle(new Vector2(1, 0), new Vector2(0, 1)));
            Debug.Log(Vector2.SignedAngle(new Vector2(0, 1), new Vector2(1, -1)));
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
                Debug.Log("New cycle search: ");
                if (TryFindCycle(vertex, roadNetwork, visitedEdges, out var cycle))
                {
                    cycles.Add(cycle);
                    Debug.Log("Found the cycle!");
                }
                Debug.Log("---------------------------------------------------");
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

            foreach (var neighbour in GetNeighboursClockwise(vertex, previous, roadNetwork))
            {
                var edge = (vertex, neighbour);
                if (visitedEdges.Contains(edge)) continue;

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
        
        private static IEnumerable<Vector3> GetNeighboursClockwise(
            Vector3 vertex,
            Vector3 previous,
            RoadNetwork roadNetwork)
        {
            var neighbours = new HashSet<(Vector3 Vertex, float Angle)>();
            
            var vertexXz = Vec3ToVec2(vertex);
            var direction = (vertexXz - Vec3ToVec2(previous)).normalized;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var angle = Vector2.SignedAngle(direction, Vec3ToVec2(neighbour));
                //if (angle <= 0)
                //{
                    neighbours.Add((neighbour, angle));
                //}
                Debug.Log((vertexXz, direction, Vec3ToVec2(neighbour), angle));
            }
            
            var clockwiseNeighbours = new List<(Vector3 Vertex, float Angle)>(neighbours);
            clockwiseNeighbours.Sort((vertex1, vertex2) =>
            {
                // Debug.Log("Magnitudes: " + (vertex1.Vertex, vertex1.Vertex.magnitude) + " + " + (vertex2.Vertex, vertex2.Vertex.magnitude));

                var (vector1, angle1) = vertex1;
                var (vector2, angle2) = vertex2;
                
                if (angle1 < angle2)
                    return -1;
                if (angle1 == angle2)
                    return vector1.magnitude < vector2.magnitude ? -1 : vector1.magnitude > vector2.magnitude ? 1 : 0;
                return 1;
                
                // return vertex1.Angle < vertex2.Angle ? -1 : vertex1.Angle > vertex2.Angle ? 1 : 0;
            });
            
            return clockwiseNeighbours.Select(tuple => tuple.Vertex);
        }
        
    }
}