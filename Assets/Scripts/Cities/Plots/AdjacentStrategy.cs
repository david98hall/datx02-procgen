using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using Cities.Roads;
using Interfaces;
using Terrain;
using Utils.Geometry;

namespace Cities.Plots
{
    internal class AdjacentStrategy : Strategy<(RoadNetwork, TerrainInfo), IEnumerable<Plot>>
    {
        /// <summary>
        /// The already existing plots that will be taken into account when generating the adjacent plots.
        /// </summary>
        private IEnumerable<Plot> _prevPlots;

        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        public AdjacentStrategy(IInjector<(RoadNetwork, TerrainInfo)> injector) : base(injector)
        {
            _prevPlots = new HashSet<Plot>();
        }

        /// <summary>
        /// Adds plots to the previously existing plots that will be taken into account in generation.
        /// </summary>
        /// <param name="plots">The already existing plots to add.</param>
        public void AddExistingPlots(IEnumerable<Plot> plots)
        {
            _prevPlots = _prevPlots.Concat(plots);
        }

        /// <summary>
        /// Generates differently sized rectangular plots along the road network parts. The plots that are intersecting
        /// with road parts or previous plots are discarded. Plots that are outside the terrain bounds are also
        /// discarded.
        /// </summary>
        /// <returns>The plots that lie along the parts of the road network.</returns>
        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            var rand = new System.Random();
            var roadNetwork = Injector.Get().Item1;
            // We need to terrain bounds to make sure the generated plots lie inside 
            var (minX, minZ, maxX, maxZ) = TerrainBounds(Injector.Get().Item2);

            // Attempt to generate a plot along each road part
            foreach (var (start, end) in roadNetwork.GetRoadParts())
            {
                var roadVector = end - start;
                var maxSideLength = Vector3.Magnitude(roadVector);
                const float roadOffset = 0.25f; // distance from each road part
                var randomPlot = RandomRectPlot(rand, start, roadVector, maxSideLength, maxSideLength, roadOffset);

                var collision = false;
                
                // Check if new plot lies within terrain bounds
                var (plotMinX, plotMinZ, plotMaxX, plotMaxZ) =
                    Maths2D.PolyExtremePoints(randomPlot.Vertices);
                if (plotMinX < minX || plotMinZ < minZ || plotMaxX > maxX || plotMaxZ > maxZ)
                    collision = true;

                // Check if new plot collides with any other plot
                foreach (var plot in plots.Concat(_prevPlots))
                {
                    if (Maths2D.PolyPolyCollision(randomPlot.Vertices, plot.Vertices).Item1)
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

        // Get terrain bounds from a TerrainInfo struct, could be placed in the TerrainInfo file itself?
        private static (float minX, float minZ, float maxX, float maxZ) TerrainBounds(TerrainInfo ti)
        {
            var minX = ti.Offset.x;
            var minZ = ti.Offset.z;
            var maxX = ti.HeightMap.GetLength(0) - 1 + ti.Offset.x;
            var maxZ = ti.HeightMap.GetLength(1) - 1 + ti.Offset.z;
            
            return (minX, minZ, maxX, maxZ);
        }

        // Returns a random (within given bounds) rectangular plot that lies alongside the road part defined by roadVector.
        private static Plot RandomRectPlot(System.Random rand, Vector3 start, Vector3 roadVector, float maxWidth, float maxLength, float roadOffset)
        {
            // The size of the side that lies alongside the road
            var width = (float)rand.NextDouble() * maxWidth + 1; // [1 .. maxWidth]

            // The size of the side that lies perpendicular to the road
            var length = (float)rand.NextDouble() * maxLength + 1; // [1 .. maxLength]

            var vertices = new LinkedList<Vector3>();
            // Determines which side of the road the plot ends up on
            var dir = Maths3D.PerpendicularClockwise(roadVector).normalized;

            // 50% chance to change side to make it more random
            if (rand.NextDouble() >= 0.5)
                dir *= -1f;

            // We want the plot to lie somewhere along the road, and not always have a corner at the start of the road part.
            var startOffset = (float)rand.NextDouble() * (roadVector.magnitude - width);

            start += roadVector.normalized * startOffset;

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
