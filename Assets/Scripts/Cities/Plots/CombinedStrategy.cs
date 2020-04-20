using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Interfaces;

namespace Cities.Plots
{
    internal class CombinedStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        private readonly AdjacentStrategy _adjacentStrategy;
        private readonly MinimalCycleStrategy _minimalCycleStrategy;
        
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        public CombinedStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
            _adjacentStrategy = new AdjacentStrategy(injector);
            _minimalCycleStrategy = new MinimalCycleStrategy(injector);
        }

        /// <summary>
        /// Combines the adjacent and minimal cycle strategies to generates plots both within cycles of the road network
        /// and along road parts.
        /// </summary>
        /// <returns>The plots created by combining the strategies.</returns>
        public override IEnumerable<Plot> Generate()
        {
            var cyclicPlots = _minimalCycleStrategy.Generate().ToList();
            _adjacentStrategy.AddExistingPlots(cyclicPlots);
            return _adjacentStrategy.Generate().Concat(cyclicPlots);
        }
    }
}
