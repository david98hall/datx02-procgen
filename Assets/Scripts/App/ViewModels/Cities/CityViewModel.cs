using System;
using System.Collections.Generic;
using System.Linq;
using Cities;
using Cities.Plots;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEditor;
using UnityEngine;
using Factory = Cities.Plots.Factory;
using BuildingFactory = Cities.Buildings.Factory;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating a city
    /// </summary>
    [Serializable]
    public class CityViewModel : ViewModelStrategy<MeshFilter, City>, IInitializable
    {
        /// <summary>
        /// Visibility of the editor.
        /// </summary>
        private bool _visible;
        private bool _aStarVisible;
        private bool _lSystemVisible;
        
        #region Road Network Strategy

        /// <summary>
        /// Visibility of the road network strategy editor.
        /// </summary>
        private bool _roadNetworkStrategyVisible;

        /// <summary>
        /// Serialized view-model for <see cref="AStarStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField]
        private AStarStrategy aStarStrategy;

        /// <summary>
        /// Serialized view-model for <see cref="LSystemStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField] 
        private LSystemStrategy lSystemStrategy;

        #endregion
        
        #region Plot Strategy

        /// <summary>
        /// Visibility of the plot strategy editor.
        /// </summary>
        private bool _plotStrategyVisible;

        /// <summary>
        /// Enum for plot strategies.
        /// Is used for displaying the possible strategies in the editor.
        /// </summary>
        public enum PlotStrategy
        {
            MinimalCycle, ClockWiseCycle, BruteMinimalCycle, Adjacent, Combined
        }

        /// <summary>
        /// Serialized road network strategy that is currently selected.
        /// </summary>
        [SerializeField] 
        private PlotStrategy plotStrategy;

        /// <summary>
        /// Material used for the generated plots.
        /// </summary>
        [SerializeField] 
        private Material plotMaterial;
        
        /// <summary>
        /// Getter for the material used for the generated plots.
        /// </summary>
        public Material PlotMaterial => plotMaterial;

        /// <summary>
        /// Boolean if plots are visible.
        /// </summary>
        [SerializeField] 
        private bool displayPlots;
        
        /// <summary>
        /// Getter for the boolean if plots are visible.
        /// </summary>
        public bool DisplayPlots => displayPlots;

        #endregion

        #region Building Strategy

        /// <summary>
        /// Visibility of the building strategy editor.
        /// </summary>
        private bool _buildingStrategyVisible;

        /// <summary>
        /// Enum for building strategies.
        /// Is used for displaying the possible strategies in the editor.
        /// </summary>
        public enum BuildingStrategy
        {
            Extrusion
        }

        /// <summary>
        /// Serialized building strategy that is currently selected.
        /// </summary>
        [SerializeField]
        private BuildingStrategy buildingStrategy;

        /// <summary>
        /// Serialized view-model for <see cref="ExtrusionStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField]
        private ExtrusionStrategy extrusionStrategy;

        /// <summary>
        /// Boolean to set building visibility.
        /// </summary>
        [SerializeField]
        private bool displayBuildings;

        /// <summary>
        /// Getter for building visibility.
        /// </summary>
        public bool DisplayBuildings => displayBuildings;

        #endregion

        #region Building Appearance Fields

        private bool _buildingAppearanceVisible;

        /// <summary>
        /// Building material.
        /// </summary>
        [SerializeField]
        private Material buildingMaterial;


        /// <summary>
        /// Building material getter.
        /// </summary>
        public Material BuildingMaterial => buildingMaterial;

        #endregion

        #region Road Appearance fields

        /// <summary>
        /// Visibility of the plot strategy editor.
        /// </summary>
        private bool _roadAppearanceVisible;
        
        /// <summary>
        /// Toad width of the generated road network.
        /// </summary>
        [SerializeField]
        private float roadWidth = 0.3f;
        
        /// <summary>
        /// Getter for the road width of the generated road network.
        /// </summary>
        public float RoadWidth => roadWidth;

        /// <summary>
        /// Toad curvature of the generated road network.
        /// </summary>
        [SerializeField]
        private float roadCurvature = 0.1f;

        /// <summary>
        /// Getter for the road curvature of the generated road network.
        /// </summary>
        public float RoadCurvature => roadCurvature;

        /// <summary>
        /// Road smoothing iterations of the generated road network.
        /// </summary>
        [SerializeField]
        private int roadSmoothingIterations = 1;
        
        /// <summary>
        /// Getter for the road smoothing iterations of the generated road network.
        /// </summary>
        public int RoadSmoothingIterations => roadSmoothingIterations;

        [SerializeField]
        private Material roadMaterial;
        
        /// <summary>
        /// Road material of the generated road network.
        /// </summary>
        public Material RoadMaterial => roadMaterial;

        /// <summary>
        /// Getter for the road material of the generated road network.
        /// </summary>
        public float RoadTerrainOffsetY => roadTerrainOffsetY;

        [SerializeField]
        private float roadTerrainOffsetY = 0.075f;
        
        #endregion

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public void Initialize()
        {
            aStarStrategy.EventBus = EventBus;
            lSystemStrategy.EventBus = EventBus;

            var injector = new Injector<float[,]>(() => Injector.Get().sharedMesh.HeightMap());
            aStarStrategy.Injector = injector;
            lSystemStrategy.Injector = injector;
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"City Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            
            DisplayRoadStrategy();
            DisplayPlotStrategy();
            DisplayBuildingStrategy();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of road networks and the view model of the currently selected road network strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private void DisplayRoadStrategy()
        {
            _roadNetworkStrategyVisible 
                = EditorGUILayout.Foldout(_roadNetworkStrategyVisible, "Road Network Generation");
            if (!_roadNetworkStrategyVisible) return;
            EditorGUI.indentLevel++;

            // A* Strategy
            _aStarVisible = EditorGUILayout.Toggle("A*", _aStarVisible);
            if (_aStarVisible)
            {
                EditorGUI.indentLevel++;
                aStarStrategy.Display();
                EditorGUI.indentLevel--;
            }

            // L-system Strategy
            _lSystemVisible = EditorGUILayout.Toggle("L-system", _lSystemVisible);
            if (_lSystemVisible)
            {
                EditorGUI.indentLevel++;
                lSystemStrategy.Display();
                EditorGUI.indentLevel--;
            }
            
            DisplayRoadAppearance();
            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// Displays the editor of the road network appearance.
        /// </summary>
        private void DisplayRoadAppearance()
        {
            _roadAppearanceVisible 
                = EditorGUILayout.Foldout(_roadAppearanceVisible, "Road Appearance");
            if (!_roadAppearanceVisible) return;
            EditorGUI.indentLevel++;
                
            // Road material
            roadMaterial = (Material) EditorGUILayout.ObjectField(
                "Material", roadMaterial, typeof(Material), true);
                
            // Road width
            roadWidth = EditorGUILayout.Slider("Width", roadWidth, 0.1f, 10);
                
            // Road/Terrain y-offset
            roadTerrainOffsetY = EditorGUILayout.FloatField("Y-Offset", roadTerrainOffsetY);

            // Road curvature and smoothing
            roadCurvature = EditorGUILayout.Slider("Curvature", roadCurvature, 0, 0.5f);
            if (roadCurvature > 0)
            {
                roadSmoothingIterations = EditorGUILayout.IntSlider(
                    "Smoothing Iterations", roadSmoothingIterations, 1, 10);   
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of plots and the view model of the currently selected plot strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private void DisplayPlotStrategy()
        {
            _plotStrategyVisible = EditorGUILayout.Foldout(_plotStrategyVisible, "Plot Generation");
            if (!_plotStrategyVisible) return;
            
            EditorGUI.indentLevel++;
            plotStrategy = (PlotStrategy) EditorGUILayout.EnumPopup("Strategy", plotStrategy);
            displayPlots = EditorGUILayout.Toggle("Display Plots", displayPlots);
            if (displayPlots)
            {
                plotMaterial = (Material) EditorGUILayout.ObjectField(
                    "Plot Material", plotMaterial, typeof(Material), true);   
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of buildings and the view model of the currently selected building strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private void DisplayBuildingStrategy()
        {
            _buildingStrategyVisible = EditorGUILayout.Foldout(_buildingStrategyVisible, "Building Generation");

            if (!_buildingStrategyVisible)
                return;

            EditorGUI.indentLevel++;
            buildingStrategy = (BuildingStrategy)EditorGUILayout.EnumPopup("Strategy", buildingStrategy);

            EditorGUI.indentLevel++;
            switch (buildingStrategy)
            {
                case BuildingStrategy.Extrusion:
                    extrusionStrategy.Display();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel--;

            displayBuildings = EditorGUILayout.Toggle("Display Buildings", displayBuildings);

            DisplayBuildingAppearance();
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of the building appearance.
        /// </summary>
        private void DisplayBuildingAppearance()
        {
            _buildingAppearanceVisible = EditorGUILayout.Foldout(_buildingAppearanceVisible, "Building Appearance");

            if (!_buildingAppearanceVisible)
                return;

            EditorGUI.indentLevel++;

            // Material
            buildingMaterial = (Material)EditorGUILayout.ObjectField(
                "Building Material", buildingMaterial, typeof(Material), true);

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
            var roadNetwork = GenerateRoadNetwork();
            var plots = GeneratePlots(roadNetwork);
            if (roadNetwork == null) return null;
            
            var enumerable = plots as Plot[] ?? plots.ToArray();
            return new City
            {
                RoadNetwork = roadNetwork,
                Plots = enumerable,
                Buildings = GenerateBuildings((Injector.Get(), enumerable))
            };

        }

        private IEnumerable<Plot> GeneratePlots(RoadNetwork roadNetwork)
        {
            if (roadNetwork == null) return null;
            
            var plotStrategyFactory = new Factory(() => roadNetwork);

            switch (plotStrategy)
            {
                case PlotStrategy.MinimalCycle:
                    return plotStrategyFactory.CreateMinimalCycleStrategy().Generate();
                case PlotStrategy.ClockWiseCycle:
                    return plotStrategyFactory.CreateClockwiseCycleStrategy().Generate();
                case PlotStrategy.BruteMinimalCycle:
                    return plotStrategyFactory.CreateBruteMinimalCycleStrategy().Generate();
                case PlotStrategy.Adjacent:
                    return plotStrategyFactory.CreateAdjacentStrategy().Generate();
                case PlotStrategy.Combined:
                    return plotStrategyFactory.CreateCombinedStrategy().Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<Building> GenerateBuildings((MeshFilter, IEnumerable<Plot>) dependencies)
        {
            var buildingStrategyFactory = new BuildingFactory(() => dependencies);

            switch (buildingStrategy)
            {
                case BuildingStrategy.Extrusion:
                    return buildingStrategyFactory.CreateExtrusionStrategy().Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RoadNetwork GenerateRoadNetwork()
        {
            var aStarRoadNetwork = _aStarVisible ? aStarStrategy.Generate() : null;
            var lSystemRoadNetwork = _lSystemVisible ? lSystemStrategy.Generate() : null;

            RoadNetwork mergedRoadNetwork;
            if (aStarRoadNetwork == null)
            {
                mergedRoadNetwork = lSystemRoadNetwork;
            } 
            else if (lSystemRoadNetwork == null)
            {
                mergedRoadNetwork = aStarRoadNetwork;
            }
            else
            {
                aStarRoadNetwork.Merge(lSystemRoadNetwork);
                mergedRoadNetwork = aStarRoadNetwork;
            }

            return mergedRoadNetwork;
        }
        
    }
}