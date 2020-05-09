using System;
using System.Collections.Generic;
using BIAS.PCG.Cities.Roads;
using BIAS.Utils.Interfaces;
using BIAS.PCG.Terrain;

namespace BIAS.PCG.Cities.Plots
{
    /// <summary>
    /// Creates strategies for generating city plots.
    /// </summary>
    public class Factory
    {
        private readonly IInjector<RoadNetwork> _roadNetworkInjector;
        private readonly IInjector<TerrainInfo> _terrainInjector;
        private readonly IInjector<(RoadNetwork, TerrainInfo)> _combinedInjector;

        /// <summary>
        /// Initializes this factory with a RoadNetwork and TerrainInfo injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The RoadNetwork injector.</param>
        /// <param name="terrainInjector">The TerrainInfo injector</param>
        /// <param name="combinedInjector">The RoadNetwork and TerrainInfo injectors combined.</param>
        public Factory(IInjector<(RoadNetwork, TerrainInfo)> combinedInjector)
        {
            _roadNetworkInjector = new Injector<RoadNetwork>(() => combinedInjector.Get().Item1);
            _terrainInjector = new Injector<TerrainInfo>(() => combinedInjector.Get().Item2);
            _combinedInjector = combinedInjector;
        }
        
        /// <summary>
        /// Initializes this factory with a RoadNetwork and TerrainInfo injector.
        /// </summary>
        /// <param name="roadNetworkInjector">The RoadNetwork injector.</param>
        public Factory(Func<RoadNetwork> roadNetworkInjector)
        {
            _roadNetworkInjector = new RoadNetworkInjector(roadNetworkInjector);
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
        /// Creates a strategy for plots adjacent to the road network.
        /// </summary>
        /// <returns>The plots adjacent to the roads.</returns>
        public IGenerator<IEnumerable<Plot>> CreateAdjacentStrategy()
        {
            return new AdjacentStrategy(_combinedInjector);
        }

        /// <summary>
        /// Creates a strategy for manually created sample plots for testing.
        /// </summary>
        /// <returns>The sample plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreatePlotSampleStrategy()
        {
            return new PlotSampleStrategy(_roadNetworkInjector);
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
        /// Creates a strategy that combines adjacent and cyclic plot generation.
        /// </summary>
        /// <returns>The found plots.</returns>
        public IGenerator<IEnumerable<Plot>> CreateCombinedStrategy()
        {
            return new CombinedStrategy(_combinedInjector);
        }

        /// <summary>
        /// Finds all minimal cycle plots in the road network.
        /// </summary>
        /// <returns>All minimal cycle plots in the road network.</returns>
        public IGenerator<IEnumerable<Plot>> CreateMinimalCycleStrategy()
        {
            return new MinimalCycleStrategy(_roadNetworkInjector);
        }
      
        // Converts a Func with the return type RoadNetwork to an injector of the same type
        private class RoadNetworkInjector : IInjector<RoadNetwork>
        {
            private readonly Func<RoadNetwork> _roadNetworkInjector;
            
            public RoadNetworkInjector(Func<RoadNetwork> roadNetworkInjector)
            {
                _roadNetworkInjector = roadNetworkInjector;
            }
            
            public RoadNetwork Get() => _roadNetworkInjector();
        }
        
      
    }
}