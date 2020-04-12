﻿using System.Collections.Generic;
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
        /// Creates a strategy for finding plots in minimal cycles of the
        /// road network at hand. This strategy is relatively slow.
        /// </summary>
        /// <returns>The found plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreateBruteMinimalCycleStrategy()
        {
            return new BruteMinimalCycleStrategy(_roadNetworkInjector);
        }
        
        /// <summary>
        /// Finds all cyclic plots by always turning clockwise when searching the road network.
        /// </summary>
        /// <returns>The found plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreateClockwiseCycleStrategy()
        {
            return new ClockwiseCycleStrategy(_roadNetworkInjector);
        }

        /// <summary>
        /// Finds all minimal cycle plots in the road network.
        /// </summary>
        /// <returns>All minimal cycle plots in the road network.</returns>
        public IGenerator<IEnumerable<Plot>> CreateMinimalCycleStrategy()
        {
            return new MinimalCycleStrategy(_roadNetworkInjector);
        }

    }
}