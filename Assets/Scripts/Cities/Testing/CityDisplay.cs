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

            var roadCount = 1;
            foreach (var road in roads)
            {
                // Create a game object with a LineRenderer component
                var item = new GameObject("Road " + roadCount++);
                var roadRenderer = item.AddComponent<LineRenderer>();
                roadRenderers.Add(item);
                // Set the appearance of the road
                roadRenderer.startWidth = 0.3f;
                roadRenderer.endWidth = 0.3f;
                // TODO Change color/material
                // Add the vertices of the road
                var roadVertices = road.ToArray();
                roadRenderer.positionCount = roadVertices.Length;
                roadRenderer.SetPositions(roadVertices);
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