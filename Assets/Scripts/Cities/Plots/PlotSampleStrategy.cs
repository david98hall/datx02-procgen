using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

using Cities.Roads;
using Interfaces;
using Utils.Geometry;

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
            var roadNetwork = Injector.Get();
            var rand = new System.Random();

            var p1 = new Plot(RandomRotatedRect(rand, new Vector3(0f, 0f, 0f), new Vector3(5f, 0f, 5f), 5f, 5f));
            plots.Add(p1);

            bool roadCollision = false;
            foreach (var (start, end) in roadNetwork.GetRoadParts())
            {
                if (Maths2D.LinePolyCollision(start, end, p1.Vertices)) 
                {
                    roadCollision = true;
                }
            }
            Debug.Log("Plots are colliding with road (t/f): " + roadCollision);

            return plots;
        }

        // Messy code used for testing collision between polygons
        private static IEnumerable<Vector3> RandomRotatedRect(System.Random rand, Vector3 minOffset, Vector3 maxOffset, float maxWidth, float maxLength)
        {
            var start = new Vector3(
                UnityEngine.Random.Range(5f, 5f), 0f, UnityEngine.Random.Range(5f, 5f));
            var roadVector = new Vector3(
                UnityEngine.Random.Range(minOffset.x, maxOffset.x), 0f, UnityEngine.Random.Range(minOffset.z, maxOffset.z));

            // width is the size of the side that lies alongside the road
            var width = (float)rand.NextDouble() * maxWidth + 1; // [1 .. maxWidth]

            // length is the size of the side that lies perpendicular to the road
            var length = (float)rand.NextDouble() * maxLength + 1; // [1 .. maxLength]

            var vertices = new LinkedList<Vector3>();
            // dir determines which side of the road the plot ends up on
            var dir = Maths3D.PerpendicularClockwise(roadVector).normalized;

            // 50% chance to change side to make it more random
            if (rand.NextDouble() >= 0.5)
                dir *= -1f;

            // We want the plot to lie somewhere along the road, and not always have a corner at the start of the road part.
            var startOffset = (float)rand.NextDouble() * (roadVector.magnitude - width);
            start = start + roadVector.normalized * startOffset;

            // Add all the vertices to form the rectangle
            vertices.AddLast(start);
            var v1 = start + roadVector.normalized * width;
            vertices.AddLast(v1);
            vertices.AddLast(v1 + dir * length);
            vertices.AddLast(start + dir * length);
            vertices.AddLast(start);

            return vertices;
        }
    }
}

