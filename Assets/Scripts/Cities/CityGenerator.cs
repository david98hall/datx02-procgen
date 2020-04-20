using System.Collections.Generic;
using Cities.Plots;
using Cities.Roads;
using Interfaces;

namespace Cities
{
    /// <summary>
    /// Generates cities.
    /// </summary>
    public class CityGenerator : IGenerator<City>, IInjector<RoadNetwork>
    {

        /// <summary>
        /// The strategy for generating road networks.
        /// </summary>
        public IGenerator<RoadNetwork> RoadNetworkStrategy
        {
            get => _roadNetworkGenerator.Strategy;
            set => _roadNetworkGenerator.Strategy = value;
        }
        private readonly Generator<RoadNetwork> _roadNetworkGenerator;
        private RoadNetwork _roadNetwork;
        
        /// <summary>
        /// The strategy for generating city plots.
        /// </summary>
        public IGenerator<IEnumerable<Plot>> PlotStrategy
        {
            get => _plotsGenerator.Strategy;
            set => _plotsGenerator.Strategy = value;
        }
        private readonly Generator<IEnumerable<Plot>> _plotsGenerator;

        private readonly IInjector<float[,]> _heightMapInjector;

        public CityGenerator()
        {
            _roadNetworkGenerator = new Generator<RoadNetwork>();
            _plotsGenerator = new Generator<IEnumerable<Plot>>();
        }

        public City Generate()
        {
            _roadNetwork = _roadNetworkGenerator.Generate();
            return new City
            {
                RoadNetwork = _roadNetwork,
                Plots = _plotsGenerator.Generate()
            };
        }

        public RoadNetwork Get() => (RoadNetwork)_roadNetwork.Clone();
    }
}