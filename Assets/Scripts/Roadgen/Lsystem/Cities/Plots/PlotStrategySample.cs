using System.Collections.Generic;
using Cities.Roads;
using Interfaces;

namespace Cities.Plots
{
    internal class PlotStrategySample : PlotStrategy
    {

        public PlotStrategySample(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            // TODO
            throw new System.NotImplementedException();
        }
    }
}