﻿using System.Collections;
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
        
        /// <summary>
        /// Initializes this factory with a RoadNetwork injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The RoadNetwork injector.</param>
        public Factory(IInjector<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = roadNetworkInjector;
        }
        
        /// <summary>
        /// Creates a strategy for finding plots in cycles of the
        /// road network at hand.
        /// </summary>
        /// <returns>The found plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreateCycleStrategy()
        {
            return new CycleStrategy(_roadNetworkInjector);
        }
        
        /// <summary>
        /// Creates a strategy for plots adjacent to the road network.
        /// </summary>
        /// <returns>The plots adjacent to the roads.</returns>
        public IGenerator<IEnumerable<Plot>> CreateAdjacentStrategy()
        {
            return new AdjacentStrategy(_roadNetworkInjector);
        }

        /// <summary>
        /// Creates a strategy for manually created sample plots for testing.
        /// </summary>
        /// <returns>The sample plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreatePlotSampleStrategy()
        {
            return new PlotSampleStrategy(_roadNetworkInjector);
        }
    }
}