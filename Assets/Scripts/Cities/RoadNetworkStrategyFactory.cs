using System;
using Interfaces;

namespace Cities
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
            throw new NotImplementedException();
        }

    }
}