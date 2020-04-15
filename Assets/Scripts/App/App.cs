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

        [SerializeField]
        [HideInInspector]
        public Material roadMaterial;

        [SerializeField]
        [HideInInspector] 
        public Material plotMaterial;

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

            // Display plot borders
            var pathObjectGenerator = new PathObjectGenerator(plotMaterial);
            gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                _model.City.Plots.Select(p => p.Vertices),
                _meshFilter, _meshCollider,
                "Plot Borders", "Plot"));
            

            // Display roads
            pathObjectGenerator.PathMaterial = roadMaterial;
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
            roadMaterial = (Material) EditorGUILayout.ObjectField("Road Material", 
                roadMaterial, typeof(Material), true);
            
            plotMaterial = (Material) EditorGUILayout.ObjectField("Plot Material", 
                plotMaterial, typeof(Material), true);
            
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