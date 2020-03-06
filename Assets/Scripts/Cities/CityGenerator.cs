using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities
{
    /// <summary>
    /// Generates cities.
    /// </summary>
    public class CityGenerator : IGenerator<City>
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
            _plotsGenerator = new PlotsGenerator(new PlotsStrategySample());
        }
        
        public City Generate()
        {
            var city = new City {RoadNetwork = _roadNetworkGenerator.Generate()};
            // TODO city.PlotsEnumerable = _plotsGenerator.Generate();
            return city;
        }

    }
}