using System;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class Factory
    {
        private readonly IInjector<MeshFilter> _terrainMeshInjector;
        
        /// <summary>
        /// Initializes this factory with a terrain mesh filter injector.
        /// </summary>
        /// <param name="terrainMeshInjector">The terrain mesh filter injector.</param>
        public Factory(IInjector<MeshFilter> terrainMeshInjector)
        {
            _terrainMeshInjector = terrainMeshInjector;
        }

        public IGenerator<RoadNetwork> CreateAStarStrategy() => new AStarStrategy(_terrainMeshInjector);

        public IGenerator<RoadNetwork> CreateLSystemStrategy() => new LSystemStrategy(_terrainMeshInjector);

        /// <summary>
        /// Creates a sample strategy for testing.
        /// </summary>
        /// <returns>The strategy.</returns>
        internal IGenerator<RoadNetwork> CreateSampleStrategy() => new SampleStrategy(_terrainMeshInjector);
    }
}