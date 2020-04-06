using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Plots
{
    public class MinimalCycleStrategy : ClockwiseCycleStrategy
    {
        
        public MinimalCycleStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        public override IEnumerable<Plot> Generate() => 
            GetMinimalCyclesXz().Select(cycle => new Plot(cycle));

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

    }
}