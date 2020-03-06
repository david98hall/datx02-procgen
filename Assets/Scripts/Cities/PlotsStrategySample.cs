using System.Collections.Generic;
using Interfaces;

namespace Cities
{
    internal class PlotsStrategySample : IGenerator<IEnumerable<Plot>>
    {

        private readonly IInjector<RoadNetwork> _roadNetworkInjector;
        
        internal PlotsStrategySample(IInjector<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = roadNetworkInjector;
        }
        
        public IEnumerable<Plot> Generate()
        {
            var roadNetwork = _roadNetworkInjector.Get();
            // TODO
            throw new System.NotImplementedException();
        }
    }
}