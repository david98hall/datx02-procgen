using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

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
            _roadNetworkGenerator = new RoadNetworkGenerator(new RoadNetworkStrategySample());
            _plotsGenerator = new PlotsGenerator(new PlotsStrategySample(this));
        }
        
        public City Generate()
        {
            _roadNetwork = _roadNetworkGenerator.Generate();
            var city = new City
            {
                RoadNetwork = _roadNetwork, 
                PlotsEnumerable = _plotsGenerator.Generate()
            };
            return city;
        }

        public RoadNetwork Get() => _roadNetwork.Copy();
    }
}