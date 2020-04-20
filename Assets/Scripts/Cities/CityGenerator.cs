using System.Collections.Generic;
using Cities.Plots;
using Cities.Roads;
using UnityEngine;
using System.Linq;
using Interfaces;
using Factory = Cities.Plots.Factory;

namespace Cities
{
    /// <summary>
    /// Generates cities.
    /// </summary>
    public class CityGenerator : IGenerator<City>, IInjector<RoadNetwork>, IInjector<(float[,], IEnumerable<Plot>)>
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
        private IEnumerable<Plot> _plots;

        /// <summary>
        /// The strategy for generating city buildings.
        /// </summary>
        public IGenerator<IEnumerable<Building>> BuildingStrategy
        {
            get => _buildingsGenerator.Strategy;
            set => _buildingsGenerator.Strategy = value;
        }
        private readonly Generator<IEnumerable<Building>> _buildingsGenerator;

        internal IInjector<float[,]> _heightMapInjector { get; set; }

        public CityGenerator()
        {
            _roadNetworkGenerator = new Generator<RoadNetwork>();
            _plotsGenerator = new Generator<IEnumerable<Plot>>();
            _buildingsGenerator = new Generator<IEnumerable<Building>>();
        }

        public City Generate()
        {

            _roadNetwork = _roadNetworkGenerator.Generate();
            _plots = _plotsGenerator.Generate();
            return new City
            {
                RoadNetwork = _roadNetwork,
                Plots = _plots,
                Buildings = _buildingsGenerator.Generate()
            };
        }


        public RoadNetwork Get() => (RoadNetwork)_roadNetwork.Clone();

        //IEnumerable<Plot> IInjector<IEnumerable<Plot>>.Get() => _plots;

        (float[,], IEnumerable<Plot>) IInjector<(float[,], IEnumerable<Plot>)>.Get()
        {
            return (_heightMapInjector.Get(), _plots);
        }
    }
}