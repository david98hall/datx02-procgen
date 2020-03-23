using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Plots;
using Cities.Roads;
using Interfaces;
using UnityEngine;
using Utils;

namespace Cities.Testing
{
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class CityDisplay : MonoBehaviour
    {
        private readonly CityGenerator _cityGenerator;
        private readonly HashSet<GameObject> roadRenderers;
        public enum RoadStrategy
        {
            Sample, 
            AStar,
        }

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public RoadStrategy roadStrategy;
        public TerrainUtil.HeightMapInjector.MapType heightMapType;
        public int width;
        public int depth;
        public int scale;
        public float beta;

        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
            _cityGenerator.PlotsStrategy = new PlotsStrategyFactory(_cityGenerator).CreateSampleStrategy();
            roadRenderers = new HashSet<GameObject>();
        }
        
        public void GenerateCity()
        {
            var heightMapInjector = new TerrainUtil.HeightMapInjector 
                {Width = width, Depth = depth, Type = heightMapType};

            meshFilter.sharedMesh = TerrainUtil.Mesh(heightMapInjector.Get(), scale);
            meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
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
                // Add the vertices of the road
                var roadVertices = road.ToArray();
                roadRenderer.positionCount = roadVertices.Length;
                roadRenderer.SetPositions(roadVertices);
            }
            
        }

        internal void Clear()
        {
            ClearRoads();
        }
        
        private void ClearRoads()
        {
            foreach (var roadRenderer in roadRenderers)
            {
                DestroyImmediate(roadRenderer);
            }
            roadRenderers.Clear();
        }

        private IGenerator<RoadNetwork> GetRoadStrategy()
        {
            
            var heightMapInjector = new TerrainUtil.HeightMapInjector
            {
                Width = width, 
                Depth = depth, 
                Type = heightMapType
            };
            
            switch (roadStrategy)
            {
                case RoadStrategy.Sample:
                    return new RoadNetworkStrategySample(heightMapInjector);
                case RoadStrategy.AStar:
                    var aStar =  new AStarGenerator(heightMapInjector) {HeightBias = beta};
                    aStar.Add((0, 0), (width - 1, depth - 1));
                    return aStar;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}