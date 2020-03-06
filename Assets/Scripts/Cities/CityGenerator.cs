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
        private RoadNetworkGenerator _roadNetworkGenerator;

        public CityGenerator()
        {
            _roadNetworkGenerator = new RoadNetworkGenerator(new RoadNetworkStrategySample());
        }
        
        public City Generate()
        {
            var city = new City {RoadNetwork = _roadNetworkGenerator.Generate()};
            city.PlotsEnumerable = GeneratePlots(city.RoadNetwork);
            return city;
        }

        private IEnumerable<Plot> GeneratePlots(RoadNetwork roadNetwork)
        {
            throw new NotImplementedException();
        }

    }
}