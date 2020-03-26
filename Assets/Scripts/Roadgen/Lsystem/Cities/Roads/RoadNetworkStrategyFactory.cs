using System;
using Interfaces;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class RoadNetworkStrategyFactory
    {

        private readonly IInjector<float[,]> _terrainMeshNoiseMapInjector;
        
        public RoadNetworkStrategyFactory(IInjector<float[,]> terrainMeshNoiseMapInjector)
        {
            _terrainMeshNoiseMapInjector = terrainMeshNoiseMapInjector;
        }
        
        public IGenerator<RoadNetwork> CreateAgentStrategy()
        {
            // TODO
            throw new NotImplementedException();
        }        
        
        public IGenerator<RoadNetwork> CreateLSystemStrategy()
        {
            // TODO
            return new LsystemStrategy(_terrainMeshNoiseMapInjector);
        }

        internal IGenerator<RoadNetwork> CreateSampleStrategy()
        {
            return new RoadNetworkStrategySample(_terrainMeshNoiseMapInjector);
        }

    }
}