using System;
using Interfaces;

namespace Cities.Roads
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public class Factory
    {

        private readonly IInjector<float[,]> _terrainMeshNoiseMapInjector;
        
        public Factory(IInjector<float[,]> terrainMeshNoiseMapInjector)
        {
            _terrainMeshNoiseMapInjector = terrainMeshNoiseMapInjector;
        }

        public IGenerator<RoadNetwork> CreateAStarStrategy()
        {
            return new AStarStrategy(_terrainMeshNoiseMapInjector);
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

        internal IGenerator<RoadNetwork> CreateSampleStrategy()
        {
            return new SampleStrategy(_terrainMeshNoiseMapInjector);
        }

    }
}