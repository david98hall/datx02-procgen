using System;
using System.Collections.Generic;
using System.Linq;
using App.Views.Cities;
using App.Views.Terrain;
using Cities;
using Extensions;
using Interfaces;
using UnityEngine;
using Utils.Paths;

namespace App
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class App : MonoBehaviour, IInitializable
    {
        private bool _initialized;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Model _model;
        
        [SerializeField]
        private HashSet<GameObject> gameObjects;
        
        [SerializeField]
        private TerrainView terrainView;
        
        [SerializeField]
        private CityView cityView;

        public void Generate()
        {
            if (!_initialized) Initialize();
            (_model.Mesh, _model.Texture) = terrainView.Generate();
            _model.City = cityView.Generate();

            _meshCollider.sharedMesh = _model.Mesh;
            _meshFilter.sharedMesh = _model.Mesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.Texture;

            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();

            // Set the values of the path object generator according to the UI-values
            var pathObjectGenerator = new PathObjectGenerator
            {
                PathWidth = cityView.RoadWidth,
                CurveFactor = cityView.RoadCurvature,
                SmoothingIterations = cityView.RoadSmoothingIterations,
                TerrainOffsetY = cityView.RoadTerrainOffsetY
            };
            
            if (cityView.DisplayPlots)
            {
                // Display plot borders
                pathObjectGenerator.PathMaterial = cityView.PlotMaterial;
                gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                    _model.City.Plots.Select(p => p.Vertices),
                    _meshFilter, _meshCollider,
                    "Plot Borders", "Plot"));
            }

            // Display roads
            pathObjectGenerator.PathMaterial = cityView.RoadMaterial;
            gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                _model.City.RoadNetwork.GetRoads(), 
                _meshFilter, _meshCollider,
                "Road Network", "Road"));
        }
        
        public void Initialize()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            
            if (gameObjects == null) gameObjects = new HashSet<GameObject>();
            
            terrainView.Initialize();
            
            _model = new Model();
            cityView.Injector = _model;
            cityView.Initialize();
            _initialized = true;
        }

        public void DisplayEditor()
        {
            terrainView.Display();
            cityView.Display();
        }
        
        private class Model : IInjector<float[,]>
        {
            internal Mesh Mesh;

            internal Texture Texture;
            internal City City;

            public float[,] Get() => Mesh.HeightMap();
        }
    }
}