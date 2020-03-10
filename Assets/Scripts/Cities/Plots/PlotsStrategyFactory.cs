using System.Collections.Generic;
using Cities.Roads;
using Interfaces;

namespace Cities.Plots
{
    /// <summary>
    /// Creates strategies for generating city plots.
    /// </summary>
    public class PlotsStrategyFactory
    {

        private readonly IInjector<RoadNetwork> _roadNetworkInjector;
        
        public PlotsStrategyFactory(IInjector<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = roadNetworkInjector;
        }

        // TODO Remove the method:
        internal IGenerator<IEnumerable<Plot>> CreateSamplePlotGenerator()
        {
            return new PlotStrategySample(_roadNetworkInjector);
        } 
        
    }
}