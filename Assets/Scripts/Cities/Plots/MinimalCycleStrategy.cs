using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Plots
{
    /// <summary>
    /// Finds all minimal cycles in a road network by searching it clockwise from each vertex.
    /// </summary>
    internal class MinimalCycleStrategy : ClockwiseCycleStrategy
    {
        
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        public MinimalCycleStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        /// <summary>
        /// Finds all minimal cycle plots in the road network.
        /// </summary>
        /// <returns>All minimal cycle plots found in the road network.</returns>
        public override IEnumerable<Plot> Generate() => 
            GetMinimalCyclesXz().Select(cycle => new Plot(cycle));

        // Extracts only minimal cycles from the set of cycles found in the road network
        private IEnumerable<IReadOnlyCollection<Vector3>> GetMinimalCyclesXz()
        {
            // Sort all cycles according to their areas (smallest first)
            var allCycles = new List<IReadOnlyCollection<Vector3>>(GetAllCyclesXz());
            allCycles.Sort((cycle1, cycle2) =>
            {
                var area1 = Maths2D.CalculatePolygonArea(cycle1.Select(Vec3ToVec2));
                var area2 = Maths2D.CalculatePolygonArea(cycle2.Select(Vec3ToVec2));
                return area1 < area2 ? -1 : area1 > area2 ? 1 : 0;
            });
            
            // Extract all cycles that do not overlap any smaller cycle, i.e., all minimal ones
            var minimalCycles = new HashSet<IReadOnlyCollection<Vector3>>();
            for (var i = allCycles.Count - 1; i >= 0; i--)
            {
                // Assume that the cycle is minimal before looking for overlaps
                var isMinimal = true;
                var cycleXz = allCycles[i].Select(Vec3ToVec2).ToList();
                for (var j = i - 1; j >= 0; j--)
                {
                    // Check if the bigger cycle overlaps the smaller
                    if (!Maths2D.AnyPolygonCenterOverlaps(allCycles[j].Select(Vec3ToVec2), cycleXz)) 
                        // Since the bigger cycle doesn't overlap the smaller,
                        // keep looking for overlaps with other, smaller, cycles
                        continue;
                    
                    // Since the bigger cycle overlaps the smaller, don't extract it 
                    isMinimal = false;
                    break;
                }

                if (isMinimal)
                    // The cycle is minimal, extract it
                    minimalCycles.Add(allCycles[i]);
            }

            // Return all minimal cycles found (a subset of all cycles)
            return minimalCycles;
        }

    }
}