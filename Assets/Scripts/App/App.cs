using System;
using System.Collections.Generic;
using System.Linq;
using App.ViewModel.Cities;
using App.ViewModel.Terrain;
using Cities;
using Extensions;
using Interfaces;
using Terrain;
using UnityEditor;
using UnityEngine;
using Utils.Paths;

namespace App
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class App : MonoBehaviour
    {
        private bool _initialized;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Model _model;
        
        [SerializeField]
        private HashSet<GameObject> gameObjects;

        [SerializeField]
        [HideInInspector]
        private TerrainGeneratorModel terrainGeneratorModel;

        [SerializeField]
        [HideInInspector]
        private CityGeneratorModel cityGeneratorModel;

        public void Generate()
        {
            if (!_initialized) Init();
            (_model.Mesh, _model.Texture) = terrainGeneratorModel.Model.Generate();
            _model.City = cityGeneratorModel.Model.Generate();

            _meshCollider.sharedMesh = _model.Mesh;
            _meshFilter.sharedMesh = _model.Mesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.Texture;

            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();

            // Set the values of the path object generator according to the UI-values
            var pathObjectGenerator = new PathObjectGenerator
            {
                PathWidth = cityGeneratorModel.roadWidth,
                CurveFactor = cityGeneratorModel.roadCurveFactor,
                SmoothingIterations = cityGeneratorModel.roadSmoothingIterations,
                TerrainOffsetY = cityGeneratorModel.roadTerrainOffsetY
            };
            
            if (cityGeneratorModel.displayPlots)
            {
                // Display plot borders
                pathObjectGenerator.PathMaterial = cityGeneratorModel.plotMaterial;
                gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                    _model.City.Plots.Select(p => p.Vertices),
                    _meshFilter, _meshCollider,
                    "Plot Borders", "Plot"));
            }

            // Display roads
            pathObjectGenerator.PathMaterial = cityGeneratorModel.roadMaterial;
            gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                _model.City.RoadNetwork.GetRoads(), 
                _meshFilter, _meshCollider,
                "Road Network", "Road"));
        }
        
        private void Init()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            if (gameObjects == null) gameObjects = new HashSet<GameObject>();
            _model = new Model();
            terrainGeneratorModel.Model = new TerrainGenerator();
            cityGeneratorModel.Model = new CityGenerator(_model);
            _initialized = true;
        }

        public void DisplayEditor()
        {
            terrainGeneratorModel.Display();
            cityGeneratorModel.Display();
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