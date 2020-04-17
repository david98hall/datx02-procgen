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

        private readonly IInjector<float[,]> _terrainMeshNoiseMapInjector;
        private readonly IInjector<MeshFilter> filterInjector;
        
        /// <summary>
        /// Initializes this factory with a noise map injector.
        /// </summary>
        /// <param name="terrainMeshNoiseMapInjector">The noise map injector.</param>
        public Factory(IInjector<float[,]> terrainMeshNoiseMapInjector)
        {
            _terrainMeshNoiseMapInjector = terrainMeshNoiseMapInjector;
        }
        public Factory(IInjector<MeshFilter> filterInjector)
        {
            this.filterInjector = filterInjector;
        }

        public IGenerator<RoadNetwork> CreateAStarStrategy() => new AStarStrategy(filterInjector);

        public IGenerator<RoadNetwork> CreateLSystemStrategy() => new LSystemStrategy(filterInjector);

        /// <summary>
        /// Creates a sample strategy for testing.
        /// </summary>
        /// <returns>The strategy.</returns>
        internal IGenerator<RoadNetwork> CreateSampleStrategy() => new SampleStrategy(filterInjector);
    }
}