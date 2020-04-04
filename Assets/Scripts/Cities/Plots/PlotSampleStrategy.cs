using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Cities.Roads;
using Interfaces;

namespace Cities.Plots
{
    /// <summary>
    /// Manually generates plots for testing, ignoring the road network.
    /// </summary>
    internal class PlotSampleStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        /// <summary>
        /// Initializes this strategy by setting the RoadNetwork injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The RoadNetwork injector.</param>
        public PlotSampleStrategy(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }

        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            
            // A 1x1 square
            var vertices = new LinkedList<Vector3>();

            vertices.AddLast(new Vector3(0f, 0f, 0f));
            vertices.AddLast(new Vector3(5f, 0f, 0f));
            vertices.AddLast(new Vector3(5f, 0f, 5f));
            vertices.AddLast(new Vector3(0f, 0f, 5f));
            vertices.AddLast(new Vector3(0f, 0f, 0f));
            

            plots.Add(new Plot(vertices));
            return plots;
        }
    }
}

