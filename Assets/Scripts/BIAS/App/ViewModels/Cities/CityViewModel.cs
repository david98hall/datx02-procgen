using System;
using System.Collections.Generic;
using System.Threading;
using BIAS.App.ViewModels.Cities.Buildings;
using BIAS.App.ViewModels.Cities.Plots;
using BIAS.App.ViewModels.Cities.Roads;
using BIAS.PCG.Cities;
using BIAS.PCG.Cities.Plots;
using BIAS.PCG.Cities.Roads;
using BIAS.Utils.Interfaces;
using BIAS.Utils.Services;
using BIAS.PCG.Terrain;
using UnityEditor;
using UnityEngine;
using BuildingFactory = BIAS.PCG.Cities.Buildings.Factory;

namespace BIAS.App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating a city
    /// </summary>
    [Serializable]
    public class CityViewModel : ViewModel<TerrainInfo, City>
    {
        /// <summary>
        /// Visibility of the editor.
        /// </summary>
        private bool _visible;

        [SerializeField]
        private RoadViewModel roadViewModel = null;
        
        [SerializeField]
        private PlotViewModel plotViewModel = null;
        
        [SerializeField]
        private BuildingViewModel buildingViewModel = null;

        #region View Model Properties
        /// <summary>
        /// <see cref="RoadViewModel.RoadWidth"/>
        /// </summary>
        public float RoadWidth => roadViewModel.RoadWidth;

        /// <summary>
        /// <see cref="RoadViewModel.RoadTerrainOffsetY"/>
        /// </summary>
        public float RoadTerrainOffsetY => roadViewModel.RoadTerrainOffsetY;
        
        /// <summary>
        /// <see cref="RoadViewModel.RoadMaterial"/>
        /// </summary>
        public Material RoadMaterial => roadViewModel.RoadMaterial;
        
        /// <summary>
        /// <see cref="PlotViewModel.DisplayPlots"/>
        /// </summary>
        public bool DisplayPlots => plotViewModel.DisplayPlots;
        
        /// <summary>
        /// <see cref="PlotViewModel.PlotMaterial"/>
        /// </summary>
        public Material PlotMaterial => plotViewModel.PlotMaterial;
                
        /// <summary>
        /// <see cref="BuildingViewModel.DisplayBuildings"/>
        /// </summary>
        public bool DisplayBuildings => buildingViewModel.DisplayBuildings;
        
        /// <summary>
        /// <see cref="BuildingViewModel.BuildingMaterial"/>
        /// </summary>
        public Material BuildingMaterial => buildingViewModel.BuildingMaterial;
        #endregion
        
        internal override IInjector<TerrainInfo> Injector
        {
            get => base.Injector;
            set
            {
                base.Injector = value;
                try
                {   
                    roadViewModel.Injector = value;
                    buildingViewModel.Injector = new Injector<(TerrainInfo, IEnumerable<Plot>)>(() =>
                        (Injector.Get(), plotViewModel.Generate()));
                }
                catch (NullReferenceException)
                {}
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
                    roadViewModel.EventBus = value;
                    plotViewModel.EventBus = value;
                    buildingViewModel.EventBus = value;
                }
                catch (NullReferenceException)
                {}
            }
        }

        public override CancellationToken CancelToken
        {
            get => base.CancelToken;
            set
            {
                base.CancelToken = value;
                try
                {   
                    roadViewModel.CancelToken = value;
                    plotViewModel.CancelToken = value;
                    buildingViewModel.CancelToken = value;
                }
                catch (NullReferenceException)
                {}
            }
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"City Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            
            // Display sub view models
            roadViewModel.Display();
            plotViewModel.Display();
            buildingViewModel.Display();

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
            var roadNetwork = roadViewModel.Generate();
            if (roadNetwork == null) return null;

            // Plots
            plotViewModel.Injector = new Injector<(RoadNetwork, TerrainInfo)>(() => 
                (roadNetwork, Injector.Get()));
            var plots = plotViewModel.Generate();
            if (plots == null) return null;
            
            // Buildings
            buildingViewModel.Injector = new Injector<(TerrainInfo, IEnumerable<Plot>)>(() => 
                (Injector.Get(), plots));
            var buildings = buildingViewModel.Generate();
            if (buildings == null) return null;
            
            return new City
            {
                RoadNetwork = roadNetwork,
                Plots = plots,
                Buildings = buildings
            };
        }

    }
}