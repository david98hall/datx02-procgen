using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

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
        private readonly RoadNetworkGenerator _roadNetworkGenerator;
        private RoadNetwork _roadNetwork;
        
        /// <summary>
        /// The strategy for generating city plots.
        /// </summary>
        public IGenerator<IEnumerable<Plot>> PlotsStrategy
        {
            get => _plotsGenerator.Strategy;
            set => _plotsGenerator.Strategy = value;
        }
        private readonly PlotsGenerator _plotsGenerator;

        public CityGenerator()
        {
            _roadNetworkGenerator = new RoadNetworkGenerator();
            _plotsGenerator = new PlotsGenerator();
        }
        
        public City Generate()
        {
            _roadNetwork = _roadNetworkGenerator.Generate();
            var city = new City
            {
                RoadNetwork = _roadNetwork // TODO, 
                // TODO PlotsEnumerable = _plotsGenerator.Generate()
            };
            return city;
        }

        public RoadNetwork Get() => (RoadNetwork)_roadNetwork.Clone();
        
    }
}