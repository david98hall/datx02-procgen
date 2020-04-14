using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Lsystem
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

        [Range(1,10)]
        public int lsystemIterations;
        
        public Material roadMaterial;
        public Material plotBorderMaterial;

        public CityDisplay()
        {
            _cityGenerator = new CityGenerator();
            _cityGenerator.PlotStrategy = new Plots.Factory(_cityGenerator).CreateMinimalCycleStrategy();
            _roadRenderers = new HashSet<GameObject>();
        }
        
        public void GenerateCity()
        {
            ClearRoads();
            meshFilter.sharedMesh = GetTerrainMesh();
            meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
            DisplayCity(_cityGenerator.Generate());
        }
        
        /*
        public async void GenerateCityAsync()
        {
            ClearRoads();
            meshFilter.sharedMesh = GetTerrainMesh();
            meshRenderer.sharedMaterial.mainTexture = Texture2D.redTexture;
            _cityGenerator.RoadNetworkStrategy = GetRoadStrategy();
            DisplayCity(await Task.Run(() => _cityGenerator.Generate()));
        }
        */

        private void DisplayCity(City city)
        {
            ClearRoads();
            DisplayPlotBorders(city.Plots);
            // DisplayRoadNetworkParts(city.RoadNetwork);
            DrawRoads(city.RoadNetwork.GetRoads());

            DisplayLotBorders(GetLots());
        }
        
        private void DisplayPlotBorders(IEnumerator<Plot> plots)
        {
            var plotCount = 1;
            while (plots.MoveNext())
            {
                if (plots.Current != null)
                {
                    DrawRoad(plots.Current.Vertices, 
                        "Plot Border " + plotCount++, 
                        plotBorderMaterial, 
                        0.15f); 
                }
            }
        }

        private void DisplayRoadNetworkParts(RoadNetwork roadNetwork)
        {
            DrawRoads(roadNetwork.GetRoadParts().Select(road => new [] {road.Start, road.End}));
        }

        private void DrawRoads(IEnumerable<IEnumerable<Vector3>> roads, float roadWidth = 0.3f)
        {
            var roadCount = 1;
            foreach (var road in roads)
            {
                DrawRoad(road, "Road " + roadCount++, roadMaterial, roadWidth);
            }
        }

        private void DrawRoad(IEnumerable<Vector3> road, string roadName, Material material, float roadWidth)
        {
            // Create a game object with a LineRenderer component
            var item = new GameObject(roadName);
            var roadRenderer = item.AddComponent<LineRenderer>();
            _roadRenderers.Add(item);
                
            // Set the appearance of the road
            roadRenderer.startWidth = roadWidth;
            roadRenderer.endWidth = roadWidth;
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
                    aStar.Add(new Vector2Int(0, 0), new Vector2Int(width/2, depth/2));
                    return aStar;
                case RoadStrategy.Lsystem:
                    return new LSystemStrategy(this, lsystemIterations);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float[,] Get() => GetTerrainMesh().HeightMap();


        #region Building and lot test methods

        public Material buildingMaterial;

        // Lots for testing
        private ICollection<Lot> GetLots()
        {

            Plot plot = new Plot();

            IEnumerable<Vector3> plotPoints = new List<Vector3>
            {
                new Vector3(0,0,0),
                new Vector3(5, 0, 0),
                new Vector3(7, 0, 4),
                new Vector3(6, 0, 10),
                new Vector3(2, 0, 10),
                new Vector3(0,0,0)
            };

            plot.SetShapeVertices(plotPoints);
            IEnumerable<Plot> plots = new List<Plot>
            {
                plot
            };

            // Get lots in plot
            LotGenerator lg = new LotGenerator(plot, 0);
            ICollection<Lot> lots = lg.Generate();

            // Need two dependencies
            ExtrusionStrategy bg = new ExtrusionStrategy(this, 5f);
            ICollection<Building> buildings = bg.GetBuildings(lots);

            foreach (Building b in buildings)
            {
                ShowBuilding(b);
            }

            return lots;
        }

        private void ShowBuilding(Building b)
        {

            GameObject building = new GameObject("Building");
            building.isStatic = true;
            //building.transform.position = b.position;
            //building.transform.rotation = Quaternion.LookRotation(b.facing, Vector3.up);
            MeshFilter meshFilter = building.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = b.mesh;
            MeshRenderer meshRenderer = building.AddComponent<MeshRenderer>();
            meshRenderer.material = buildingMaterial;
            _roadRenderers.Add(building);
        }

        private void DisplayLotBorders(ICollection<Lot> lots)
        {
            var lotsE = lots.GetEnumerator();

            var lotCount = 1;
            while (lotsE.MoveNext())
            {
                if (lotsE.Current != null)
                {
                    DrawRoad(lotsE.Current.Vertices,
                        "Lot Border " + lotCount++,
                        plotBorderMaterial,
                        0.15f);
                }
            }
        }

        #endregion
    }
}