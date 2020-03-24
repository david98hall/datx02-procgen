using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Plots;
using Cities.Roads;
using Interfaces;
using Extensions;
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

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public RoadStrategy roadStrategy;
        public TerrainUtil.HeightMapInjector.MapType heightMapType;
        public int width;
        public int depth;
        public int scale;
        public float heightBias;
        
        public Material roadMaterial;
        public enum RoadStrategy
        {
            Sample, 
            AStar,
        }

        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
            _cityGenerator.PlotsStrategy = new PlotsStrategyFactory(_cityGenerator).CreateSampleStrategy();
            roadRenderers = new HashSet<GameObject>();
        }
        
        public void GenerateCity()
        {
            if (roadStrategy != RoadStrategy.Sample)
            {
                var heightMapInjector = new TerrainUtil.HeightMapInjector 
                    {Width = width, Depth = depth, Type = heightMapType};

                meshFilter.sharedMesh = TerrainUtil.Mesh(heightMapInjector.Get(), scale);
                meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            }
            
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
            var city = _cityGenerator.Generate();
            DisplayCity(city);
        }

        private void DisplayCity(City city)
        {
            ClearRoads();
            // DisplayPlotBorders(city.Plots);
            DisplayRoadNetwork(city.RoadNetwork);
        }

        private void DisplayPlotBorders(IEnumerator<Plot> plots)
        {
            var plotCount = 1;
            while (plots.MoveNext())
            {
                if (plots.Current != null)
                {
                    DrawRoad(plots.Current.Vertices, "Plot Border " + plotCount++, null); 
                }
            }
        }
        
        private void DisplayRoadNetwork(RoadNetwork roadNetwork)
        {
            DrawRoads(roadNetwork.GetRoads());
        }

        private void DrawRoads(IEnumerable<IEnumerable<Vector3>> roads)
        {
            var roadCount = 1;
            foreach (var road in roads)
            {
                DrawRoad(road, "Road " + roadCount++, roadMaterial);
            }
        }

        private void DrawRoad(IEnumerable<Vector3> road, string roadName, Material material)
        {
            // Create a game object with a LineRenderer component
            var item = new GameObject(roadName);
            var roadRenderer = item.AddComponent<LineRenderer>();
            roadRenderers.Add(item);
                
            // Set the appearance of the road
            roadRenderer.startWidth = 0.3f;
            roadRenderer.endWidth = 0.3f;
            roadRenderer.numCornerVertices = 90;
            roadRenderer.numCapVertices = 90;
            roadRenderer.textureMode = LineTextureMode.Tile;
            roadRenderer.generateLightingData = true;
            if (material != null)
            {
                roadRenderer.sharedMaterial = material;
            }
                
            // Add the vertices of the road
            var roadArray = road.ToArray();
            roadRenderer.positionCount = roadArray.Length;
            roadRenderer.SetPositions(roadArray);
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
                    var aStar =  new AStarGenerator(heightMapInjector) {HeightBias = heightBias};
                    aStar.Add((0, 0), (width - 1, depth - 1));
                    return aStar;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}