using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Interfaces;
using Terrain;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Plots
{
    internal class CombinedStrategy : Strategy<(RoadNetwork, TerrainInfo), IEnumerable<Plot>>
    {
        private readonly AdjacentStrategy _adjacentStrategy;
        private readonly MinimalCycleStrategy _minimalCycleStrategy;
        
        /// <summary>
        /// Initializes the strategy with a RoadNetwork injector.
        /// </summary>
        /// <param name="injector">The RoadNetwork injector.</param>
        internal CombinedStrategy(IInjector<(RoadNetwork, TerrainInfo)> injector) : base(injector)
        {
            _adjacentStrategy = new AdjacentStrategy(injector);

            // The minimal cycle strategy doesn't need the terrain info
            var roadInjector = new Injector<RoadNetwork>(() => injector.Get().Item1);
            _minimalCycleStrategy = new MinimalCycleStrategy(roadInjector);
        }

        /// <summary>
        /// Combines the adjacent and minimal cycle strategies to generates plots both within cycles of the road network
        /// and along road parts.
        /// </summary>
        /// <returns>The plots created by combining the strategies.</returns>
        public override IEnumerable<Plot> Generate()
        {
            var cyclicPlots = _minimalCycleStrategy.Generate();
            if (cyclicPlots == null) return null;

            // What area is big enough?
            const float bigEnoughArea = 2f;
            var bigEnoughPlots = new HashSet<Plot>();
            foreach (var plot in cyclicPlots)
            {
                // Cancel if requested
                if (CancelToken.IsCancellationRequested) return null;
                
                var vertices2D = plot.Vertices.Select(v => new Vector2(v.x, v.z));
                if (Maths2D.CalculatePolygonArea(vertices2D) > bigEnoughArea)
                    bigEnoughPlots.Add(plot);
            }

            _adjacentStrategy.AddExistingPlots(bigEnoughPlots);
            return _adjacentStrategy.Generate()?.Concat(bigEnoughPlots);
        }
    }
}
