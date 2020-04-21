using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class Factory
    {
        private readonly IInjector<float[,]> _terrainMeshNoiseMapInjector;
        
        /// <summary>
        /// Initializes this factory with a noise map injector.
        /// </summary>
        /// <param name="terrainMeshNoiseMapInjector">The noise map injector.</param>
        public Factory(IInjector<float[,]> terrainMeshNoiseMapInjector)
        {
            _terrainMeshNoiseMapInjector = terrainMeshNoiseMapInjector;
        }

        /// <summary>
        /// Creates an A* strategy for generating a road network.
        /// </summary>
        /// <param name="heightBias">The height bias for finding the optimal path.</param>
        /// <param name="paths">Paths consisting of a start and goal node to generate a road between.</param>
        /// <returns>The A* road network generator.</returns>
        public IGenerator<RoadNetwork> CreateAStarStrategy(
            float heightBias = 0.5f, IEnumerable<(Vector2Int Start, Vector2Int Goal)> paths = null)
        {
            var strategy = new AStarStrategy(_terrainMeshNoiseMapInjector) {HeightBias = heightBias};
            foreach (var (start, goal) in paths)
            {
                strategy.Add(start, goal);
            }

            return strategy;
        }

        /// <summary>
        /// Creates a L-system strategy for generating a road network. The origin is (0, 0).
        /// </summary>
        /// <param name="rewriteCount">
        /// The number of times the L-system should be rewritten,
        /// before returning the road network.
        /// </param>
        /// <returns>An L-system generator for road networks.</returns>
        public IGenerator<RoadNetwork> CreateLSystemStrategy(int rewriteCount = 6) =>
            CreateLSystemStrategy(Vector2.zero, rewriteCount);

        /// <summary>
        /// Creates a L-system strategy for generating a road network.
        /// </summary>
        /// <param name="origin">The start point of the road network generation.</param>
        /// <param name="rewriteCount">
        /// The number of times the L-system should be rewritten,
        /// before returning the road network.
        /// </param>
        /// <returns>An L-system generator for road networks.</returns>
        public IGenerator<RoadNetwork> CreateLSystemStrategy(Vector2 origin, int rewriteCount = 6)
        {
            return new LSystemStrategy(_terrainMeshNoiseMapInjector, rewriteCount)
            {
                Origin = origin
            };
        }

        /// <summary>
        /// Creates a sample strategy for testing.
        /// </summary>
        /// <returns>The strategy.</returns>
        internal IGenerator<RoadNetwork> CreateSampleStrategy() => new SampleStrategy(_terrainMeshNoiseMapInjector);
    }
}