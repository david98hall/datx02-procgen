using System;
using System.Collections.Generic;
using System.Linq;
using App.ViewModels.Cities;
using App.ViewModels.Terrain;
using Cities;
using Extensions;
using Interfaces;
using Services;
using Terrain;
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
    public class AppController : MonoBehaviour, IDisplayable
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

        // Used for view model communications
        private EventBus<AppEvent> _eventBus;
        
        public void OnEnable()
        {
            if (!_initialized) Initialize();
        }

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// Should only be called once due to expensive operations.
        /// </summary>
        public void Initialize()
        {
            _eventBus = new EventBus<AppEvent>();
            
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            
            if (gameObjects == null) gameObjects = new HashSet<GameObject>();

            terrainViewModel.EventBus = _eventBus;

            _model = new Model();
            cityViewModel.EventBus = _eventBus;
            cityViewModel.Injector = _model;

            // Create an event making all capable subscribers broadcast their values
            _eventBus.CreateEvent(AppEvent.Broadcast, null, this);
            
            _initialized = true;
        }
        
        /// <summary>
        /// Delegates the generation to the underlying view models.
        /// Displays the generated content using the unity objects
        /// </summary>
        public void Generate()
        {
            foreach (var obj in gameObjects) DestroyImmediate(obj);
            gameObjects.Clear();
            
            (_meshFilter.sharedMesh, _model.TerrainTexture) = terrainViewModel.Generate();
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;
            _meshRenderer.sharedMaterial.mainTexture = _model.TerrainTexture;

            // Update the model's terrain data
            _model.TerrainHeightMap = _meshFilter.sharedMesh.HeightMap();
            _model.TerrainOffset = _meshFilter.transform.position;
            
            _model.City = cityViewModel.Generate();
            if (_model.City == null) return;
            
            // Set the values of the path object generator according to the UI-values
            var pathObjectGenerator = new PathObjectGenerator
            {
                PathWidth = cityViewModel.RoadWidth,
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

            // Display buildings
            if (cityViewModel.DisplayBuildings)
            {
                var container = new GameObject("Buildings", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
                foreach (var b in _model.City.Buildings)
                {
                    var obj = new GameObject("Building", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
                    obj.GetComponent<MeshFilter>().mesh = b.mesh;
                    obj.GetComponent<MeshRenderer>().sharedMaterial = cityViewModel.BuildingMaterial;

                    obj.transform.parent = container.transform;
                }
                gameObjects.Add(container);
            }

            // Display roads
            pathObjectGenerator.PathMaterial = cityViewModel.RoadMaterial;
            gameObjects.Add(pathObjectGenerator.GeneratePathNetwork(
                _model.City.RoadNetwork.GetRoads(), 
                _meshFilter, _meshCollider,
                "Road Network", "Road"));
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
        private class Model : IInjector<TerrainInfo>
        {
            /// <summary>
            /// Generated terrain mesh.
            /// </summary>
            internal float[,] TerrainHeightMap { get; set; }

            internal Vector3 TerrainOffset { get; set; }
            
            /// <summary>
            /// Generated texture.
            /// </summary>
            internal Texture TerrainTexture { get; set; }
            
            /// <summary>
            /// Generated City
            /// </summary>
            internal City City { get; set; }

            /// <summary>
            /// Injector method used by the city view model.
            /// </summary>
            /// <returns>The height map of the terrain mesh.</returns>
            public TerrainInfo Get() => new TerrainInfo
            {
                HeightMap = TerrainHeightMap,
                Offset = TerrainOffset
            };
        }
    }
}