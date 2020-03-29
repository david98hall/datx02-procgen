using System.Collections.Generic;
using Cities.Roads;
using Interfaces;
using JetBrains.Annotations;

namespace Cities.Plots
{
    /// <summary>
    /// Generates plots based on a road network.
    /// </summary>
    internal abstract class PlotStrategy : IGenerator<IEnumerable<Plot>>
    {
        /// <summary>
        /// The road network to base plots on.
        /// </summary>
        protected RoadNetwork RoadNetwork => _roadNetworkInjector.Get();
        
        private readonly IInjector<RoadNetwork> _roadNetworkInjector;
        
        /// <summary>
        /// Sets the road network injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The road network injector.</param>
        internal PlotStrategy([NotNull] IInjector<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = roadNetworkInjector;
        }
        
        public abstract IEnumerable<Plot> Generate();
    }
}