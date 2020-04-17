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
    /// <summary>
    /// View controller script for controlling the procedural generator from a Unity editor
    /// 
    /// </summary>
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), 
        typeof(MeshCollider))]
    public class AppController : MonoBehaviour, IInitializable, IDisplayable
    {
        #region Models

        /// <summary>
        /// Boolean for if the run-time objects have been initialized.
        /// Is required for performance to not perform expensive operations twice.
        /// </summary>
        private bool _initialized;
        
        /// <summary>
        /// Model containing all generated content.
        /// </summary>
        private Model _model;
        
        /// <summary>
        /// Serialized view-model for <see cref="TerrainViewModel"/> view model.
        /// </summary>
        [SerializeField]
        private TerrainViewModel terrainViewModel;
        
        /// <summary>
        /// Serialized view-model for <see cref="TerrainViewModel"/> view model.
        /// </summary>
        [SerializeField]
        private CityViewModel cityViewModel;

        #endregion

        #region Unity objects

        /// <summary>
        /// Filter for displaying meshes and game objects
        /// </summary>
        private MeshFilter _meshFilter;
        
        /// <summary>
        /// Renderer for display textures.
        /// </summary>
        private MeshRenderer _meshRenderer;
        
        /// <summary>
        /// Collider for displaying game objects.
        /// </summary>
        private MeshCollider _meshCollider;
        
        /// <summary>
        /// Serialized set of all created game objects.
        /// </summary>
        [SerializeField]
        private HashSet<GameObject> gameObjects;

        #endregion
        
        /// <summary>
        /// Delegates the generation to the underlying view models.
        /// Displays the generated content using the unity objects
        /// </summary>
        public void Generate()
        {
            if (!_initialized) Initialize();
            (_model.Mesh, _model.Texture) = terrainViewModel.Generate();
            _model.City = cityViewModel.Generate();

            _meshCollider.sharedMesh = _model.Mesh;
            _meshFilter.sharedMesh = _model.Mesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.Texture;

            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();

            // Set the values of the path object generator according to the UI-values
            var pathObjectGenerator = new PathObjectGenerator
            {
                PathWidth = cityViewModel.RoadWidth,
                CurveFactor = cityViewModel.RoadCurvature,
                SmoothingIterations = cityViewModel.RoadSmoothingIterations,
                TerrainOffsetY = cityViewModel.RoadTerrainOffsetY
            };
            
            if (cityViewModel.DisplayPlots)
            {
                // Display plot borders
                pathObjectGenerator.PathMaterial = cityViewModel.PlotMaterial;
                gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                    _model.City.Plots.Select(p => p.Vertices),
                    _meshFilter, _meshCollider,
                    "Plot Borders", "Plot"));
            }

            // Display roads
            pathObjectGenerator.PathMaterial = cityViewModel.RoadMaterial;
            gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                _model.City.RoadNetwork.GetRoads(), 
                _meshFilter, _meshCollider,
                "Road Network", "Road"));
        }
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// Should only be called once due to expensive operations.
        /// </summary>
        public void Initialize()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            
            if (gameObjects == null) gameObjects = new HashSet<GameObject>();
            
            terrainViewModel.Initialize();
            
            _model = new Model();
            cityViewModel.Injector = _model;
            cityViewModel.Initialize();
            _initialized = true;
        }

        /// <summary>
        /// Displays the editors of the underlying view models.
        /// </summary>
        public void Display()
        {
            terrainViewModel.Display();
            cityViewModel.Display();
        }
        
        /// <summary>
        /// The run-time model of all generated content.
        /// </summary>
        private class Model : IInjector<float[,]>
        {
            /// <summary>
            /// Generated terrain mesh.
            /// </summary>
            internal Mesh Mesh;
            
            /// <summary>
            /// Generated texture.
            /// </summary>
            internal Texture Texture;
            
            /// <summary>
            /// Generated City
            /// </summary>
            internal City City;

            /// <summary>
            /// Injector method used by the city view model.
            /// </summary>
            /// <returns>The height map of the terrain mesh.</returns>
            public float[,] Get() => Mesh.HeightMap();
        }
    }
}