using System;
using System.Collections.Generic;
using Cities;
using Cities.Plots;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    [Serializable]
    public class CityModel : ViewModelStrategy<float[,], City>
    {
        private CityGenerator _generator;
        private bool _visible;

        #region Road Network Strategy

        private bool _roadNetworkStrategyVisible;
        
        [SerializeField] 
        private AStarStrategy aStarStrategy;

        [SerializeField] 
        private LSystemStrategy lSystemStrategy;
        
        public enum RoadNetworkStrategy
        {
            LSystem, AStar
        }

        [SerializeField]
        private RoadNetworkStrategy roadNetworkStrategy;

        #endregion
        
        #region Plot Strategy
        
        private bool _plotStrategyVisible;
        private Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>> _plotStrategies;
        
        public enum PlotStrategy
        {
            MinimalCycle, ClockWiseCycle, BruteMinimalCycle
        }

        [SerializeField] 
        private PlotStrategy plotStrategy;

        public Material PlotMaterial
        {
            get => plotMaterial;
            set => plotMaterial = value;
        }
        
        [SerializeField] 
        private Material plotMaterial;

        public bool DisplayPlots
        {
            get => displayPlots;
            set => displayPlots = value;
        }
        
        [SerializeField] 
        private bool displayPlots;
        
        #endregion
        
        #region Road Appearance fields
        
        private bool _roadAppearanceVisible;

        public float RoadWidth
        {
            get => roadWidth;
            set => roadWidth = value;
        }
        
        [SerializeField]
        private float roadWidth = 0.3f;
        
        
        public float RoadCurvature
        {
            get => roadCurvature;
            set => roadCurvature = value;
        }
        
        [SerializeField]
        private float roadCurvature = 0.1f;
        
        public int RoadSmoothingIterations
        {
            get => roadSmoothingIterations;
            set => roadSmoothingIterations = value;
        }
        
        [SerializeField]
        private int roadSmoothingIterations = 1;

        public Material RoadMaterial
        {
            get => roadMaterial;
            set => roadMaterial = value;
        }
        
        [SerializeField]
        private Material roadMaterial;

        public float RoadTerrainOffsetY
        {
            get => roadTerrainOffsetY;
            set => roadTerrainOffsetY = value;
        }
        
        [SerializeField]
        private float roadTerrainOffsetY = 0.075f;
        
        #endregion

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
            };
        }
        
        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"City Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            
            DisplayRoadStrategy();
            DisplayPlotStrategy();

            EditorGUI.indentLevel--;
        }

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
        
        private void DisplayRoadStrategy()
        {
            _roadNetworkStrategyVisible 
                = EditorGUILayout.Foldout(_roadNetworkStrategyVisible, "Road Network Generation");
            if (_roadNetworkStrategyVisible)
            {
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
        }

        private void DisplayPlotStrategy()
        {
            _plotStrategyVisible = EditorGUILayout.Foldout(_plotStrategyVisible, "Plot Generation");
            if (_plotStrategyVisible)
            {
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
        }
        
        public override City Generate()
        {
            // Set the road network generation strategy
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

            // Set the plot generation strategy
            _generator.PlotStrategy = _plotStrategies[plotStrategy];
            
            // Generate a city
            return _generator.Generate();
        }
        
    }
}