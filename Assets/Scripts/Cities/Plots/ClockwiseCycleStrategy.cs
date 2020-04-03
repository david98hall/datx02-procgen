using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
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

        public override IEnumerable<Plot> Generate() => GetMinimalCyclesXz().Select(cycle => new Plot(cycle));

        private IEnumerable<IReadOnlyCollection<Vector3>> GetMinimalCyclesXz()
        {
            // XZ-projection of the undirected road network
            var roadNetwork = Injector.Get().GetXZProjection().GetAsUndirected();

            // If there aren't at least three vertices in the road network, there can't possibly be a cycle in it
            if (roadNetwork.VertexCount < 3) 
                return new List<IReadOnlyCollection<Vector3>>();

            // Find all minimal cycles in the road network (if any)
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
            cycle = FindCycle(vertex, roadNetwork, visitedEdges);
            return cycle != null;
        }

        private static IReadOnlyCollection<Vector3> FindCycle(
            Vector3 vertex, 
            RoadNetwork roadNetwork, 
            ICollection<(Vector3 Start, Vector3 End)> visitedEdges)
        {
            // TODO Implement clockwise search
            return null;
        }
        
    }
}