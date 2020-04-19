using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cities.Roads;
using Interfaces;
using Utils.Geometry;

namespace Cities.Plots 
{
    internal class AdjacentStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        public AdjacentStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            var roadNetwork = Injector.Get();
            var rand = new System.Random();

            foreach (var (start, end) in roadNetwork.GetRoadParts())
            {
                var roadVector = end - start;
                var maxSideLength = Vector3.Magnitude(roadVector);
                const float roadOffset = 0.25f; // distance from each road part
                var randomPlot = RandomRectPlot(rand, start, roadVector, maxSideLength, maxSideLength, roadOffset);

                bool collision = false;

                // Check if new plot collides with any other plot
                foreach (var plot in plots)
                {
                    if (Maths2D.AreColliding(randomPlot.Vertices, plot.Vertices))
                    {
                        collision = true;
                        break;
                    }
                }

                // Check if new plot collides with any road part
                foreach (var (s, e) in roadNetwork.GetRoadParts())
                {
                    if (Maths2D.LinePolyCollision(s, e, randomPlot.Vertices))
                    {
                        collision = true;
                        break;
                    }
                }
                if (!collision)
                    plots.Add(randomPlot);

            }

            return plots;
        }

        // Returns a random (within given bounds) rectangular plot that lies alongside the road part defined by roadVector.
        private static Plot RandomRectPlot(System.Random rand, Vector3 start, Vector3 roadVector, float maxWidth, float maxLength, float roadOffset) 
        {            
            // The size of the side that lies alongside the road
            var width = (float) rand.NextDouble() * maxWidth + 1; // [1 .. maxWidth]

            // The size of the side that lies perpendicular to the road
            var length = (float) rand.NextDouble() * maxLength + 1; // [1 .. maxLength]

            var vertices = new LinkedList<Vector3>();
            // Determines which side of the road the plot ends up on
            var dir = Maths3D.PerpendicularClockwise(roadVector).normalized;

            // 50% chance to change side to make it more random
            if (rand.NextDouble() >= 0.5)
                dir *= -1f;
            
            // We want the plot to lie somewhere along the road, and not always have a corner at the start of the road part.
            var startOffset = (float) rand.NextDouble() * (roadVector.magnitude - width);
            start = start + roadVector.normalized * startOffset;

            // Add all the vertices to form the rectangle
            vertices.AddLast(start);
            var v1 = start + roadVector.normalized * width;
            vertices.AddLast(v1);
            vertices.AddLast(v1 + dir * length);
            vertices.AddLast(start + dir * length);
            vertices.AddLast(start);

            return new Plot(vertices.Select(v => v + dir * roadOffset));
        }
    }
}
