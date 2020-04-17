using System;
using System.Collections.Generic;
using System.Linq;
using App.ViewModels.Cities;
using App.ViewModels.Terrain;
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
    public class AppController : MonoBehaviour, IInitializable
    {
        private bool _initialized;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Model _model;
        
        [SerializeField]
        private HashSet<GameObject> gameObjects;
        
        [SerializeField]
        private TerrainViewModel terrainViewModel;
        
        [SerializeField]
        private CityModel cityModel;

        public void Generate()
        {
            if (!_initialized) Initialize();
            (_model.Mesh, _model.Texture) = terrainViewModel.Generate();
            _model.City = cityModel.Generate();

            _meshCollider.sharedMesh = _model.Mesh;
            _meshFilter.sharedMesh = _model.Mesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.Texture;

            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();

            // Set the values of the path object generator according to the UI-values
            var pathObjectGenerator = new PathObjectGenerator
            {
                PathWidth = cityModel.RoadWidth,
                CurveFactor = cityModel.RoadCurvature,
                SmoothingIterations = cityModel.RoadSmoothingIterations,
                TerrainOffsetY = cityModel.RoadTerrainOffsetY
            };
            
            if (cityModel.DisplayPlots)
            {
                // Display plot borders
                pathObjectGenerator.PathMaterial = cityModel.PlotMaterial;
                gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                    _model.City.Plots.Select(p => p.Vertices),
                    _meshFilter, _meshCollider,
                    "Plot Borders", "Plot"));
            }

            // Display roads
            pathObjectGenerator.PathMaterial = cityModel.RoadMaterial;
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
            
            terrainViewModel.Initialize();
            
            _model = new Model();
            cityModel.Injector = _model;
            cityModel.Initialize();
            _initialized = true;
        }

        public void DisplayEditor()
        {
            terrainViewModel.Display();
            cityModel.Display();
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