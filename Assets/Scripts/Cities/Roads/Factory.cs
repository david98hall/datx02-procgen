using System;
using System.Collections.Generic;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class Factory
    {

        /// <summary>
        /// Creates an A* strategy for generating a road network.
        /// </summary>
        /// <param name="heightMapInjector">An injector of a height map.</param>
        /// <param name="heightBias">The height bias for finding the optimal path.</param>
        /// <param name="paths">Paths consisting of a start and goal node to generate a road between.</param>
        /// <returns>The A* road network generator.</returns>
        public IGenerator<RoadNetwork> CreateAStarStrategy(
            IInjector<float[,]> heightMapInjector,
            float heightBias = 0.5f, 
            IEnumerable<(Vector2Int Start, Vector2Int Goal)> paths = null)
        {
            var strategy = new AStarStrategy(heightMapInjector) {HeightBias = heightBias};
            foreach (var (start, goal) in paths)
            {
                strategy.Add(start, goal);
            }

            return strategy;
        }

        /// <summary>
        /// Creates a L-system strategy for generating a road network.
        /// </summary>
        /// <param name="terrainFilterInjector">The terrain mesh filter injector.</param>
        /// <param name="origin">The start point of the road network generation.</param>
        /// <param name="rewriteCount">
        /// The number of times the L-system should be rewritten,
        /// before returning the road network.
        /// </param>
        /// <returns>An L-system generator for road networks.</returns>
        public IGenerator<RoadNetwork> CreateLSystemStrategy(
            IInjector<MeshFilter> terrainFilterInjector, Vector2 origin, int rewriteCount = 6)
        {
            return new LSystemStrategy(terrainFilterInjector, rewriteCount)
            {
                Origin = origin
            };
        }
    }
}