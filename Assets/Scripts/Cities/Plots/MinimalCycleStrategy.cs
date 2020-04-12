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
    internal class MinimalCycleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {

        private readonly ClockwiseCycleStrategy _clockwiseCycleStrategy;
        
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        public MinimalCycleStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
            _clockwiseCycleStrategy = new ClockwiseCycleStrategy(injector);
        }

        /// <summary>
        /// Finds all minimal cycle plots in the road network.
        /// </summary>
        /// <returns>All minimal cycle plots found in the road network.</returns>
        public override IEnumerable<Plot> Generate()
        {
            Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
            
            // Sort all cycles according to their areas (smallest first)
            var allPlots = new List<Plot>(_clockwiseCycleStrategy.Generate());
            allPlots.Sort((plot1, plot2) =>
            {
                var area1 = Maths2D.CalculatePolygonArea(plot1.Vertices.Select(Vec3ToVec2));
                var area2 = Maths2D.CalculatePolygonArea(plot2.Vertices.Select(Vec3ToVec2));
                return area1 < area2 ? -1 : area1 > area2 ? 1 : 0;
            });
            
            // Extract all cycles that do not overlap any smaller cycle, i.e., all minimal ones
            var minimalCyclePlots = new HashSet<Plot>();
            for (var i = allPlots.Count - 1; i >= 0; i--)
            {
                // Assume that the cycle is minimal before looking for overlaps
                var isMinimal = true;
                var cycleXz = allPlots[i].Vertices.Select(Vec3ToVec2).ToList();
                for (var j = i - 1; j >= 0; j--)
                {
                    // Check if the bigger cycle overlaps the smaller
                    if (!Maths2D.AnyPolygonCenterOverlaps(allPlots[j].Vertices.Select(Vec3ToVec2), cycleXz)) 
                        // Since the bigger cycle doesn't overlap the smaller,
                        // keep looking for overlaps with other, smaller, cycles
                        continue;
                    
                    // Since the bigger cycle overlaps the smaller, don't extract it 
                    isMinimal = false;
                    break;
                }

                if (isMinimal)
                    // The cycle is minimal, extract it
                    minimalCyclePlots.Add(allPlots[i]);
            }

            // Return all minimal cycles found (a subset of all cycles)
            return minimalCyclePlots;
        }

    }
}