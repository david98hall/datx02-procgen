using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
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
            
            // A square
            var vertices = new LinkedList<Vector3>();
            vertices.AddLast(new Vector3(0f, 0f, 0f));
            vertices.AddLast(new Vector3(5f, 0f, 0f));
            vertices.AddLast(new Vector3(5f, 0f, 5f));
            vertices.AddLast(new Vector3(0f, 0f, 5f));
            vertices.AddLast(new Vector3(0f, 0f, 0f));
            var p1 = new Plot(vertices);
            plots.Add(p1);

            // Another square, offset and rotated 45 degrees around the y-axis
            vertices = new LinkedList<Vector3>();
            const int offsetX = 0;
            const int offsetZ = 5;
            vertices.AddLast(new Vector3(offsetX, 0f, offsetZ));
            vertices.AddLast(new Vector3(offsetX + 5f, 0f, offsetZ));
            vertices.AddLast(new Vector3(offsetX + 5f, 0f, 5f + offsetZ));
            vertices.AddLast(new Vector3(offsetX, 0f, 5f + offsetZ));
            vertices.AddLast(new Vector3(offsetX, 0f, offsetZ));
            var p2 = new Plot(vertices.Select(v => Quaternion.Euler(0f, 45f, 0f) * v));
            plots.Add(p2);

            Debug.Log("Plots are colliding (t/f): " + p1.CollidesWith(p2));

            return plots;
        }
    }
}

