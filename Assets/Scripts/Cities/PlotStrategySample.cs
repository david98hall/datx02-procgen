using System.Collections.Generic;
using Interfaces;

namespace Cities
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