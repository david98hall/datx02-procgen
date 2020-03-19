using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Plots
{
    internal class PlotStrategySample : PlotStrategy
    {

        public PlotStrategySample(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            
            // TODO
            
            return plots;
        }

        private static IEnumerable<Plot> GenerateTrianglePlots(IEnumerable<(Vector3 start, Vector3 end)> roadParts)
        {
            return null;
        }
        
        /*
            var cycleStrings = stronglyConnectedRoads.Select(road =>
            {
                const string arrow = " -> ";
                var cycle = road.Aggregate("Cycle: ", (current, vector) => current + (vector + arrow));
                return cycle.Substring(0, cycle.Length - arrow.Length);
            });

            foreach (var cycle in cycleStrings)
            {
                Debug.Log(cycle);
            }
            */
        
    }
}