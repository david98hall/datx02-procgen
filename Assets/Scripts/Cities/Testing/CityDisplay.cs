using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            var roads = roadNetwork.GetRoads();
            
            // TODO Create one LineRenderer for each road

            // TODO Remove everything below in this method
            if (roads.Any())
            {
                var road = roads.First();
                
                roadRenderer.positionCount = road.Count();
                roadRenderer.SetPositions(road.ToArray());
            }
            else
            {
                Debug.Log("No road to display!");
            }
            
        }
        
    }
}