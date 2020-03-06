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
        private readonly CityGenerator _cityGenerator;

        private readonly HashSet<GameObject> roadRenderers;
        
        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
            roadRenderers = new HashSet<GameObject>();
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
            ClearRoads();
            
            var roads = roadNetwork.GetRoads();

            foreach (var road in roads)
            {
                var item = new GameObject("LineRenderer");
                var roadRenderer = item.AddComponent<LineRenderer>();
                roadRenderer.positionCount = road.Count();
                roadRenderer.SetPositions(road.ToArray());
                roadRenderer.startWidth = 0.3f;
                roadRenderer.endWidth = 0.3f;
                roadRenderers.Add(item);
            }
            
        }

        private void ClearRoads()
        {
            foreach (var roadRenderer in roadRenderers)
            {
                DestroyImmediate(roadRenderer);
            }
            roadRenderers.Clear();
        }
        
    }
}