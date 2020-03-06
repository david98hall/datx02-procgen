using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities.Testing
{
    public class CityDisplay : MonoBehaviour
    {
        public LineRenderer roadRenderer;

        public bool autoUpdate;

        private readonly CityGenerator _cityGenerator;

        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
        }
        
        public void GenerateCity()
        {
            var city = _cityGenerator.Generate();
            DisplayCity(city);
        }

        private void DisplayCity(City city)
        {
            DisplayRoadNetwork(city.RoadNetwork);
        }

        private void DisplayRoadNetwork(RoadNetwork roadNetwork)
        {
            // TODO
            throw new NotImplementedException();
        }
        
    }
}
