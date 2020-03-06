using System;
using Interfaces;

namespace Cities
{
    /// <summary>
    /// Creates strategies for generating road networks.
    /// </summary>
    public static class RoadNetworkStrategyFactory
    {

        public static IGenerator<RoadNetwork> CreateAgentStrategy()
        {
            // TODO
            throw new NotImplementedException();
        }        
        
        public static IGenerator<RoadNetwork> CreateLSystemStrategy()
        {
            // TODO
            throw new NotImplementedException();
        }

    }
}