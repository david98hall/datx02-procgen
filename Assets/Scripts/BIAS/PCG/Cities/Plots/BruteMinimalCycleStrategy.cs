using System.Collections.Generic;
using System.Linq;
using BIAS.PCG.Cities.Roads;
using BIAS.Utils.Extensions;
using BIAS.Utils.Interfaces;
using UnityEngine;
using BIAS.Utils.Parallelism;
using BIAS.Utils.Geometry;

namespace BIAS.PCG.Cities.Plots
{
    /// <summary>
    /// Generates plots within cycles of the road network.
    /// </summary>
    internal class BruteMinimalCycleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {

        /// <summary>
        /// Initializes this strategy by setting the RoadNetwork injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The RoadNetwork injector.</param>
        internal BruteMinimalCycleStrategy(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        /// <summary>
        /// Generates plots within cycles of the road network.
        /// </summary>
        /// <returns>The generated plots.</returns>
        public override IEnumerable<Plot> Generate() => GetMinimalCyclesInXZ().Select(cycle => new Plot(cycle));

        private static Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
        
        /// <summary>
        /// Gets all minimal cycles in the XZ-plane where the road network's XZ-projection intersections are found.
        /// </summary>
        /// <returns>All minimal cycles in the XZ-plane</returns>
        private IEnumerable<IReadOnlyCollection<Vector3>> GetMinimalCyclesInXZ()
        {
            // XZ-projection of the undirected road network
            var roadNetwork = Injector.Get().GetXZProjection().GetAsUndirected();

            // If there aren't at least three vertices in the road network, there can't possibly be a cycle in it
            if (roadNetwork.VertexCount < 3) 
                return new List<IReadOnlyCollection<Vector3>>();
            
            // Get all cycles in the road network and sort them from the smallest area
            // to the largest in order to later only save the minimal cycles
            var cycles = GetAllCycles(roadNetwork).ToList();
            cycles.Sort((cycle1, cycle2) =>
            {
                var area1 = Maths2D.CalculatePolygonArea(cycle1.Select(Vec3ToVec2));
                var area2 = Maths2D.CalculatePolygonArea(cycle2.Select(Vec3ToVec2));
                return area1 < area2 ? -1 : area1 > area2 ? 1 : 0;
            });
            
            return ExtractMinimalCycles(cycles); 
        }

        /// <summary>
        /// Gets all cycles in the road network.
        /// </summary>
        /// <param name="roadNetwork">The road network.</param>
        /// <returns>All cycles in the road network.</returns>
        private IEnumerable<IReadOnlyCollection<Vector3>> GetAllCycles(RoadNetwork roadNetwork)
        {
            // Concurrent search for cycles from each vertex
            return TaskUtils.RunActionInTasks(
                    roadNetwork.RoadVertices,
                    vertex => TryGetCycles(roadNetwork, vertex, out var vertexCycles)
                        ? vertexCycles
                        : new List<IReadOnlyCollection<Vector3>>(), 
                    CancelToken)
                // Put all found cycles in one collection
                ?.SelectMany(cycle => cycle)
                // Filter out any cycles that do not have any vertices
                .Where(c => c.Any());
        }
        
        /// <summary>
        /// Extracts all minimal cycles (neither cycle contains another nor intersects another).
        /// </summary>
        /// <param name="cycles">The cycles to extract from.</param>
        /// <returns>All extracted minimal cycles in the given enumerable.</returns>
        private IEnumerable<IReadOnlyCollection<Vector3>> ExtractMinimalCycles(
            IEnumerable<IReadOnlyCollection<Vector3>> cycles)
        {
            var minimalCycles = new LinkedList<IReadOnlyCollection<Vector3>>();
            
            // Filter away any cycles that overlap minimal ones; they're not minimal
            foreach (var cycle in cycles)
            {
                // Cancel if requested
                if (CancelToken.IsCancellationRequested) return null;
                
                // Find out if the cycle is overlapping any minimal one
                var cycleXz = cycle.Select(Vec3ToVec2);
                var isOverlapping = minimalCycles.Any(minimalCycle => 
                    Maths2D.AnyPolygonCenterOverlaps(cycleXz, minimalCycle.Select(Vec3ToVec2)));

                // If this cycle doesn't overlap any minimal cycle, it is minimal as well
                if (!isOverlapping && !minimalCycles.Any(cycle.ContainsAll))
                    minimalCycles.AddLast(cycle);
            }

            return minimalCycles;
        }
        
        /// <summary>
        /// Gets all edges of the cycle as an enumerable of tuples.
        /// </summary>
        /// <param name="cycle">The source.</param>
        /// <returns>All edges in the source cycle.</returns>
        private static IEnumerable<(Vector3 Start, Vector3 End)> GetCycleEdges(IReadOnlyCollection<Vector3> cycle)
        {
            var cycleCopy = new LinkedList<Vector3>(cycle);
            var first = cycleCopy.First.Value;
            cycleCopy.RemoveFirst();
            cycleCopy.AddLast(first);
            return cycle.Zip(cycleCopy, (v1, v2) => (v1, v2));
        }
        
        /// <summary>
        /// Tries to get cycles, starting from the given vertex.
        /// </summary>
        /// <param name="roadNetwork">The road network to search in.</param>
        /// <param name="startVertex">The vertex to start searching from.</param>
        /// <param name="cycles">The found cycles in the road network.</param>
        /// <returns>true if any were found, false if not.</returns>
        private static bool TryGetCycles(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            out IReadOnlyCollection<IReadOnlyCollection<Vector3>> cycles)
        {
            cycles = GetCycles(
                roadNetwork, 
                startVertex,
                startVertex,
                new HashSet<(Vector3 Start, Vector3 End)>());
            return cycles.Any();
        }
        
        /// <summary>
        /// Searches for cycles in the road network by starting from the given vertex.
        /// </summary>
        /// <param name="roadNetwork">The road network to search in.</param>
        /// <param name="startVertex">The vertex the search started.</param>
        /// <param name="currentVertex">The vertex that the search has reached.</param>
        /// <param name="visitedEdges">All visited edges so far during the search in the road network.</param>
        /// <returns>All cycles found in the road network.</returns>
        private static IReadOnlyCollection<IReadOnlyCollection<Vector3>> GetCycles(
            RoadNetwork roadNetwork,
            Vector3 startVertex,
            Vector3 currentVertex,
            IEnumerable<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            // Copy the visited edges to only keep track of the edges visited in this recursive search
            var localVisitedEdges = new HashSet<(Vector3 Start, Vector3 End)>(visitedEdges);

            // Get all neighbours of the current vertex in the search that haven't been visited from it before
            var neighbours = roadNetwork
                .GetAdjacentVertices(currentVertex)
                .Where(n => !localVisitedEdges.Contains((currentVertex, n)))
                .ToList();

            // Search for cycles, branching the search from each neighbour
            var cycles = new List<IReadOnlyCollection<Vector3>>(neighbours.Count);
            foreach (var neighbour in neighbours)
            {
                // If the neighbour is equal to the start of the search, a cycle has been found!
                if (neighbour.Equals(startVertex))
                    cycles.Add(new [] {currentVertex, startVertex});
                
                // Update the edge history to keep track of where it's allowed to move next
                localVisitedEdges.Add((currentVertex, neighbour));

                // Get all (potential) cycles, searching from the current neighbour
                var pathExtensions = GetCycles(
                    roadNetwork, startVertex, neighbour, localVisitedEdges);
                if (!pathExtensions.Any())
                    continue;
                
                // Traverse any found cycles found when searching from the current neighbour
                foreach (var pathExtension in pathExtensions)
                {
                    if (GetCycleEdges(pathExtension).Contains((neighbour, currentVertex))) continue;
                 
                    // If the found cycle does not contain the same edge twice
                    // but in different directions, add it to the result
                    var cyclePath = new LinkedList<Vector3>();
                    cyclePath.AddLast(currentVertex);
                    cyclePath.AddRange(pathExtension);
                    cycles.Add(cyclePath);
                }
            }

            /*
            if (!cycles.Any()) return null;
            
            // If any cycles were found, sort them by their vertex count
            cycles.Sort((extension1, extension2) => 
                extension1.Count < extension2.Count ? -1 : extension1.Count > extension2.Count ? 1 : 0);
            return cycles;
            */
            
            return cycles;
        }

    }
}