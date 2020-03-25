using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Plots;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;
using Utils;

namespace Cities.Testing
{
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class CityDisplay : MonoBehaviour, IInjector<float[,]>
    {
        private readonly CityGenerator _cityGenerator;
        private readonly HashSet<GameObject> _roadRenderers;

        public MeshFilter meshFilter;
        
        public MeshRenderer meshRenderer;
        public enum RoadStrategy
        {
            Sample, 
            AStar,
        }
        
        public RoadStrategy roadStrategy;
        
        public enum MapType
        {
            Flat, 
            Slope,
            Pyramid
        }
        
        public MapType heightMapType;
        
        [Range(1,100)]
        public int width;
        
        [Range(1,100)]
        public int depth;
        [Range(1,10)]
        public int scale;
        
        [Range(0,1)]
        public float heightBias;
        
        public Material roadMaterial;

        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
            _cityGenerator.PlotStrategy = new Plots.Factory(_cityGenerator).CreateSampleStrategy();
            _roadRenderers = new HashSet<GameObject>();
        }
        
        public void GenerateCity()
        {
            ClearRoads();
            meshFilter.sharedMesh = GetTerrainMesh();
            meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
            DisplayCity(_cityGenerator.Generate());
            
            /*
            var heightMapInjector = new TerrainUtil.HeightMapInjector 
                {Width = width, Depth = depth, Type = heightMapType};

            meshFilter.sharedMesh = TerrainUtil.Mesh(heightMapInjector.Get(), scale);
            meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
            var city = _cityGenerator.Generate();
            DisplayCity(city);
            */
        }

        private Mesh GetTerrainMesh()
        {
            float[,] heightMap;
            switch (heightMapType)
            {
                case MapType.Flat:
                    heightMap = TerrainUtil.Flat(width, depth);
                    break;
                case MapType.Slope:
                    heightMap = TerrainUtil.Slope(width, depth);
                    break;
                case MapType.Pyramid:
                    heightMap = TerrainUtil.Pyramid(width, depth);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return TerrainUtil.Mesh(heightMap, scale);
        }
        
        private IGenerator<RoadNetwork> GetRoadStrategy()
        {
            switch (roadStrategy)
            {
                case RoadStrategy.Sample:
                    return new Roads.SampleStrategy(this);
                case RoadStrategy.AStar:
                    var aStar =  new AStarStrategy(this) {HeightBias = heightBias};
                    aStar.Add((0, 0), (width/2, depth/2));
                    return aStar;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DisplayCity(City city)
        {
            //ClearRoads();
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
            _roadRenderers.Add(item);
                
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
            foreach (var roadRenderer in _roadRenderers)
            {
                DestroyImmediate(roadRenderer);
            }
            _roadRenderers.Clear();
        }
        
        public float[,] Get() => GetTerrainMesh().HeightMap();

    }
}