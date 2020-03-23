using System.Collections.Generic;
using System.Linq;
using Cities.Plots;
using Cities.Roads;
using Extensions;
using UnityEngine;

namespace Cities.Testing
{
    public class CityDisplay : MonoBehaviour
    {
        private readonly CityGenerator _cityGenerator;

        private readonly HashSet<GameObject> roadRenderers;

        public Material roadMaterial;
        
        public CityDisplay()
        {
            _cityGenerator = new CityGenerator
            {
                RoadNetworkStrategy = new RoadNetworkStrategyFactory(null).CreateSampleStrategy()                
            };
            _cityGenerator.PlotsStrategy = new PlotsStrategyFactory(_cityGenerator).CreateSampleStrategy();
            roadRenderers = new HashSet<GameObject>();
        }
        
        public void GenerateCity()
        {
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
        
    }
}