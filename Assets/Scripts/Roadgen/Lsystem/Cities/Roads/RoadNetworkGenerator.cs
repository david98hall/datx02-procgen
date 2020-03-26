using Interfaces;

namespace Cities.Roads
{
    /// <summary>
    /// Generates road networks
    /// </summary>
    internal class RoadNetworkGenerator : IGenerator<RoadNetwork>
    {
        /// <summary>
        /// The strategy of generating a road network.
        /// </summary>
        internal IGenerator<RoadNetwork> Strategy { get; set; }
        
        /// <summary>
        /// Generates a road network with the set strategy.
        /// </summary>
        /// <returns>The generated noise map.</returns>
        public RoadNetwork Generate() => Strategy.Generate();

    }
}