using Interfaces;

namespace Cities
{
    /// <summary>
    /// A strategy for generating road networks.
    /// </summary>
    internal abstract class RoadNetworkStrategy : IGenerator<RoadNetwork>
    {

        protected readonly IInjector<float[,]> _terrainNoiseMapInjector;
        
        /// <summary>
        /// Sets the terrain noise map injector
        /// </summary>
        /// <param name="terrainNoiseMapInjector">The injector for the terrain noise map.</param>
        internal RoadNetworkStrategy(IInjector<float[,]> terrainNoiseMapInjector)
        {
            _terrainNoiseMapInjector = terrainNoiseMapInjector;
        }

        public abstract RoadNetwork Generate();
    }
}