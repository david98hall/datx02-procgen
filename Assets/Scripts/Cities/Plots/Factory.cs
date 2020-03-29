using System.Collections.Generic;
using Cities.Roads;
using Interfaces;

namespace Cities.Plots
{
    /// <summary>
    /// Creates strategies for generating city plots.
    /// </summary>
    public class Factory
    {
        private readonly IInjector<RoadNetwork> _roadNetworkInjector;
        
        public Factory(IInjector<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = roadNetworkInjector;
        }
        
        internal IGenerator<IEnumerable<Plot>> CreateSortingStrategy()
        {
            return new SortingStrategy(_roadNetworkInjector);
        }
    }
}