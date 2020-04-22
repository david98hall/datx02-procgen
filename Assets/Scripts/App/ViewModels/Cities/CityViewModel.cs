using System;
using System.Collections.Generic;
using System.Linq;
using App.ViewModels.Cities.Buildings;
using App.ViewModels.Cities.Plots;
using App.ViewModels.Cities.Roads;
using Cities;
using Cities.Plots;
using Cities.Roads;
using Interfaces;
using Services;
using Terrain;
using UnityEditor;
using UnityEngine;
using BuildingFactory = Cities.Buildings.Factory;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating a city
    /// </summary>
    [Serializable]
    public class CityViewModel : ViewModelStrategy<TerrainInfo, City>, IInitializable
    {
        /// <summary>
        /// Visibility of the editor.
        /// </summary>
        private bool _visible;

        [SerializeField]
        private RoadViewModel _roadViewModel;
        
        [SerializeField]
        private PlotViewModel _plotViewModel;
        
        [SerializeField]
        private BuildingViewModel _buildingViewModel;

        #region View Model Properties
        /// <summary>
        /// <see cref="RoadViewModel.RoadWidth"/>
        /// </summary>
        public float RoadWidth => _roadViewModel.RoadWidth;
        
        /// <summary>
        /// <see cref="RoadViewModel.RoadCurvature"/>
        /// </summary>
        public float RoadCurvature => _roadViewModel.RoadCurvature;
        
        /// <summary>
        /// <see cref="RoadViewModel.RoadSmoothingIterations"/>
        /// </summary>
        public int RoadSmoothingIterations => _roadViewModel.RoadSmoothingIterations;
        
        /// <summary>
        /// <see cref="RoadViewModel.RoadTerrainOffsetY"/>
        /// </summary>
        public float RoadTerrainOffsetY => _roadViewModel.RoadTerrainOffsetY;
        
        /// <summary>
        /// <see cref="RoadViewModel.RoadMaterial"/>
        /// </summary>
        public Material RoadMaterial => _roadViewModel.RoadMaterial;
        
        /// <summary>
        /// <see cref="PlotViewModel.DisplayPlots"/>
        /// </summary>
        public bool DisplayPlots => _plotViewModel.DisplayPlots;
        
        /// <summary>
        /// <see cref="PlotViewModel.PlotMaterial"/>
        /// </summary>
        public Material PlotMaterial => _plotViewModel.PlotMaterial;
                
        /// <summary>
        /// <see cref="BuildingViewModel.DisplayBuildings"/>
        /// </summary>
        public bool DisplayBuildings => _buildingViewModel.DisplayBuildings;
        
        /// <summary>
        /// <see cref="BuildingViewModel.BuildingMaterial"/>
        /// </summary>
        public Material BuildingMaterial => _buildingViewModel.BuildingMaterial;
        #endregion
        
        internal override IInjector<TerrainInfo> Injector
        {
            get => base.Injector;
            set
            {
                base.Injector = value;
                try
                {   
                    _roadViewModel.Injector = value;
                    _plotViewModel.Injector = _roadViewModel;
                    _buildingViewModel.Injector = new Injector<(TerrainInfo, IEnumerable<Plot>)>(() =>
                        (InjectedValue, _plotViewModel.Generate()));
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    _roadViewModel.EventBus = value;
                    _plotViewModel.EventBus = value;
                    _buildingViewModel.EventBus = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadViewModel.Initialize();
            _plotViewModel.Initialize();
            _buildingViewModel.Initialize();
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"City Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            
            _roadViewModel.Display();
            _plotViewModel.Display();
            _buildingViewModel.Display();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Updates the underlying generator with the serialized values from the editor.
        /// Delegates the generation to the underlying generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override City Generate()
        {
            // Road network
            _roadViewModel.Injector = Injector;
            var roadNetwork = _roadViewModel.Generate();
            if (roadNetwork == null) return null;
            
            // Plots
            _plotViewModel.Injector = new Injector<RoadNetwork>(() => roadNetwork);
            var plots = _plotViewModel.Generate();
            var enumerable = plots as Plot[] ?? plots.ToArray();
            
            // Buildings
            _buildingViewModel.Injector = new Injector<(TerrainInfo, IEnumerable<Plot>)>(() => 
                (InjectedValue, enumerable));
            
            return new City
            {
                RoadNetwork = roadNetwork,
                Plots = enumerable,
                Buildings = _buildingViewModel.Generate()
            };
        }

    }
}