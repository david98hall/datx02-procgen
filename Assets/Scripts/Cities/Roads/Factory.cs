using System;
using Interfaces;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class Factory
    {
        public IInjector<float[,]> MeshHeightMapInjector { get; }
        
        /// <summary>
        /// Initializes this factory with a noise map injector.
        /// </summary>
        /// <param name="meshHeightMapInjector">The noise map injector.</param>
        public Factory(IInjector<float[,]> meshHeightMapInjector)
        {
            MeshHeightMapInjector = meshHeightMapInjector;
        }

        public IGenerator<RoadNetwork> CreateAStarStrategy() => new AStarStrategy(MeshHeightMapInjector);

        public IGenerator<RoadNetwork> CreateLSystemStrategy() => new LSystemStrategy(MeshHeightMapInjector);

        /// <summary>
        /// Creates a sample strategy for testing.
        /// </summary>
        /// <returns>The strategy.</returns>
        internal IGenerator<RoadNetwork> CreateSampleStrategy() => new SampleStrategy(MeshHeightMapInjector);
    }
}