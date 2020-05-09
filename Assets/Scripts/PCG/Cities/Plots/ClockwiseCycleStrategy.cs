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
    internal class ClockwiseCycleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        internal ClockwiseCycleStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        public override IEnumerable<Plot> Generate() => GetAllCyclesXz()?.Select(cycle => new Plot(cycle));

        /// <summary>
        /// Finds all cycles in the road network.
        /// </summary>
        /// <returns>All found cycles.</returns>
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
                // Cancel if requested
                if (CancelToken.IsCancellationRequested) return null;
                
                if (TryFindCycle(vertex, roadNetwork, visitedEdges, out var cycle))
                {
                    cycles.Add(cycle);
                }
            }
            
            return cycles;
        }

        // Searches for a cycle in the road network
        private bool TryFindCycle(
            Vector3 vertex, 
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out IReadOnlyCollection<Vector3> cycle)
        {
            cycle = FindCycle(vertex, vertex, vertex, roadNetwork, visitedEdges);
            return cycle != null;
        }
        
        /// <summary>
        /// Searches for a cycle, moving clockwise from the start vertex.
        /// </summary>
        /// <param name="start">The start vertex of the search.</param>
        /// <param name="vertex">The current vertex in the search..</param>
        /// <param name="previous">The previous vertex in the search.</param>
        /// <param name="roadNetwork">The road network that is being searched.</param>
        /// <param name="visitedEdges">All visited edges during the search.</param>
        /// <param name="firstIteration">
        /// true if it is the first iteration of the search; otherwise false
        /// </param>
        /// <returns>
        /// A cycle if one is found when moving clockwise from the start vertex.
        /// null if no cycle was found.  
        /// </returns>
        private IReadOnlyCollection<Vector3> FindCycle(
            Vector3 start,
            Vector3 vertex,
            Vector3 previous,
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            bool firstIteration = true)
        {
            // Cancel if requested
            if (CancelToken.IsCancellationRequested) return null;
            
            if (!firstIteration && start.Equals(vertex))
                return new []{vertex};

            if (TryGetClockwiseNeighbour(vertex, previous, roadNetwork, visitedEdges, out var neighbour))
            {
                // Mark the edge as visited
                visitedEdges.Add((vertex, neighbour));
                
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

        
        /// <summary>
        /// Tries to get the most clockwise and non-visited
        /// neighbour of the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to look for neighbours from.</param>
        /// <param name="previous">The vertex visited directly before vertex.</param>
        /// <param name="roadNetwork">The road network to look for neighbouring vertices in.</param>
        /// <param name="visitedEdges">The set of already visited edges.</param>
        /// <param name="clockwiseNeighbour">
        /// The neighbour that is most clockwise and reachable to the given vertex.
        /// </param>
        /// <returns>true if a clockwise neighbour was found.</returns>
        private static bool TryGetClockwiseNeighbour(
            Vector3 vertex,
            Vector3 previous,
            RoadNetwork roadNetwork,
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges,
            out Vector3 clockwiseNeighbour)
        {
            // Default value if no neighbour is found
            clockwiseNeighbour = Vector3.negativeInfinity;
            
            // Traverse all neighbours to vertex
            var vertexXz = Vec3ToVec2(vertex);
            var direction = vertexXz - Vec3ToVec2(previous);
            var maxAngle = float.MinValue;
            foreach (var neighbour in roadNetwork.GetAdjacentVertices(vertex))
            {
                var newDirection = Vec3ToVec2(neighbour) - vertexXz;
                
                // Calculate the angle between the current direction (from previous to vertex)
                // and the potentially new direction (from vertex to neighbour).
                var angle = Vector2.SignedAngle(direction, newDirection);
                
                // Check if the neighbour has been visited from vertex before and
                // that it is not equal to the previous vertex
                var couldGoTo = !neighbour.Equals(previous) && !visitedEdges.Contains((vertex, neighbour));
                if (angle > maxAngle && couldGoTo)
                {
                    // If the angle is reachable and has the largest angle so far, store it as
                    // the most clockwise to vertex
                    maxAngle = angle;
                    clockwiseNeighbour = neighbour;
                }
            }

            // If a clockwise neighbour was found, return true
            return !clockwiseNeighbour.Equals(Vector3.negativeInfinity);
        }
        
        // Converts a Vector3 to a Vector2 with
        // Vector3's z-coordinate as the Vector2's y-coordinate
        private static Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
        
    }
}