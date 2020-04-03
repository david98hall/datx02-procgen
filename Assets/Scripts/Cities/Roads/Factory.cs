﻿using System;
using Interfaces;

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

        public IGenerator<RoadNetwork> CreateAStarStrategy() => new AStarStrategy(_terrainMeshNoiseMapInjector);

        public IGenerator<RoadNetwork> CreateLSystemStrategy() => new LSystemStrategy(_terrainMeshNoiseMapInjector);

        /// <summary>
        /// Creates a sample strategy for testing.
        /// </summary>
        /// <returns>The strategy.</returns>
        internal IGenerator<RoadNetwork> CreateSampleStrategy() => new SampleStrategy(_terrainMeshNoiseMapInjector);
    }
}