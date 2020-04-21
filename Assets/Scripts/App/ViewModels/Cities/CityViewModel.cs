using System;
using System.Collections.Generic;
using Cities;
using Cities.Plots;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating a city
    /// </summary>
    [Serializable]
    public class CityViewModel : ViewModelStrategy<MeshFilter, City>
    {
        /// <summary>
        /// Underlying <see cref="CityGenerator"/> model.
        /// Is required to be set explicitly in run-time.
        /// </summary>
        private CityGenerator _generator;
        
        /// <summary>
        /// Visibility of the editor.
        /// </summary>
        private bool _visible;

        #region Road Network Strategy

        /// <summary>
        /// Visibility of the road network strategy editor.
        /// </summary>
        private bool _roadNetworkStrategyVisible;

        /// <summary>
        /// Enum for road network strategies.
        /// Is used for displaying the possible strategies in the editor.
        /// </summary>
        public enum RoadNetworkStrategy
        {
            LSystem, AStar
        }

        /// <summary>
        /// Serialized road network strategy that is currently selected.
        /// </summary>
        [SerializeField]
        private RoadNetworkStrategy roadNetworkStrategy;

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
        /// All plot strategy models.
        /// No editor is required for plot strategies so no view models are required either.
        /// Is required to be set explicitly in run-time.
        /// </summary>
        private Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>> _plotStrategies;
        
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
        public override void Initialize()
        {
            _generator = new CityGenerator();

            aStarStrategy.Injector = Injector;
            lSystemStrategy.Injector = Injector;
            
            aStarStrategy.Initialize();
            lSystemStrategy.Initialize();
            
            // Plot strategies
            var plotStrategyFactory = new Factory(_generator);
            _plotStrategies = new Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>>
            {
                [PlotStrategy.MinimalCycle] = plotStrategyFactory.CreateMinimalCycleStrategy(),
                [PlotStrategy.ClockWiseCycle] = plotStrategyFactory.CreateClockwiseCycleStrategy(),
                [PlotStrategy.BruteMinimalCycle] = plotStrategyFactory.CreateBruteMinimalCycleStrategy(),
                [PlotStrategy.Adjacent] = plotStrategyFactory.CreateAdjacentStrategy(),
                [PlotStrategy.Combined] = plotStrategyFactory.CreateCombinedStrategy(),
            };
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
            roadNetworkStrategy 
                = (RoadNetworkStrategy) EditorGUILayout.EnumPopup("Strategy", roadNetworkStrategy);
            EditorGUI.indentLevel++;
            switch (roadNetworkStrategy)
            {
                case RoadNetworkStrategy.LSystem:
                    lSystemStrategy.Display();
                    break;
                case RoadNetworkStrategy.AStar:
                    aStarStrategy.Display();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel--;
                
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
        /// Updates the underlying generator with the serialized values from the editor.
        /// Delegates the generation to the underlying generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override City Generate()
        {
            switch (roadNetworkStrategy)
            {
                case RoadNetworkStrategy.LSystem:
                    _generator.RoadNetworkStrategy = lSystemStrategy;
                    break;
                case RoadNetworkStrategy.AStar:
                    _generator.RoadNetworkStrategy = aStarStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _generator.PlotStrategy = _plotStrategies[plotStrategy];
            
            return _generator.Generate();
        }
    }
}