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

namespace App
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class App : MonoBehaviour
    {
        private bool _initialized;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
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
            
            _meshFilter.sharedMesh = _model.Mesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.Texture;

            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();
            
            var plots = _model.City.Plots;
            var count = 1;
            while (plots.MoveNext())
            {
                var plot = plots.Current;
                if (plot == null) continue;
                DisplayItem(plot.Vertices, "Plot Border " + count++, plotMaterial, 0.15f); 
            }

            count = 1;
            foreach (var road in _model.City.RoadNetwork.GetRoads())
            {
                DisplayItem(road, "Road " + count++, roadMaterial, 0.3f);
            }
        }
        
        private void Init()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if (gameObjects == null) gameObjects = new HashSet<GameObject>();
            _model = new Model();
            terrainGeneratorModel.Model = new TerrainGenerator();
            cityGeneratorModel.Model = new CityGenerator(_model);
            _initialized = true;
        }

        private void DisplayItem(IEnumerable<Vector3> item, string itemName, Material material, float width)
        {
            var itemObject = new GameObject(itemName);
            var lineRenderer = itemObject.AddComponent<LineRenderer>();

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.numCornerVertices = 90;
            lineRenderer.numCapVertices = 90;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.generateLightingData = true;

            if (material != null) lineRenderer.sharedMaterial = null;

            var itemArray = item.ToArray();
            lineRenderer.positionCount = itemArray.Length;
            lineRenderer.SetPositions(itemArray);

            gameObjects.Add(itemObject);
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