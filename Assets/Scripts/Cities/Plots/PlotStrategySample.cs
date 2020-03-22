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
    internal class PlotStrategySample : PlotStrategy
    {

        public PlotStrategySample(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            
            var roadParts = RoadNetwork.GetRoadParts().ToList();

//            plots.AddRange(GeneratePlots(roadParts));

            var plotStrings = plots.Select(plot =>
            {
                const string arrow = " -> ";
                var plotString = plot.Vertices.Aggregate("Plot: ", (current, vector) => current + (vector + arrow));
                return plotString.Substring(0, plotString.Length - arrow.Length);
            });

            Debug.Log("Number of plots: " + plots.Count);
            
            foreach (var plotString in plotStrings)
            {
                Debug.Log(plotString);
            }
            
            return plots;
        }

    }
}