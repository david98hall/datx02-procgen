using System;
using System.Collections.Generic;
using Cities;
using Cities.Plots;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class CityGeneratorModel : EditorStrategyView<float[,], City>
    {
        private CityGenerator _generator;
        private bool _visible;

        #region Road Network Strategy

        private bool _roadNetworkStrategyVisible;
        
        [SerializeField] 
        private AStarStrategyModel aStarStrategyModel;

        [SerializeField] 
        private LSystemStrategyModel lSystemStrategyModel;
        
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

        [SerializeField] 
        private Material plotMaterial;

        [SerializeField] 
        private bool displayPlots;
        
        #endregion
        
        #region Road Appearance fields
        
        private bool _roadAppearanceVisible;
        
        [SerializeField]
        private float roadWidth = 0.3f;
        
        [SerializeField]
        private float roadCurveFactor = 0.1f;
        
        [SerializeField]
        private int roadSmoothingIterations = 1;

        [SerializeField]
        private Material roadMaterial;

        [SerializeField]
        private float roadTerrainOffsetY = 0.075f;
        
        #endregion

        public override void Initialize()
        {
            _generator = new CityGenerator();
            
            // Road network strategies
            aStarStrategyModel = new AStarStrategyModel();
            lSystemStrategyModel = new LSystemStrategyModel();
            
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
            if (_roadAppearanceVisible)
            {
                EditorGUI.indentLevel++;
                
                // Road material
                roadMaterial = (Material) EditorGUILayout.ObjectField(
                    "Material", roadMaterial, typeof(Material), true);
                
                // Road width
                roadWidth = EditorGUILayout.Slider("Width", roadWidth, 0.1f, 10);
                
                // Road/Terrain y-offset
                roadTerrainOffsetY = EditorGUILayout.FloatField("Y-Offset", roadTerrainOffsetY);

                // Road curvature and smoothing
                roadCurveFactor = EditorGUILayout.Slider("Curvature", roadCurveFactor, 0, 0.5f);
                if (roadCurveFactor > 0)
                {
                    roadSmoothingIterations = EditorGUILayout.IntSlider(
                        "Smoothing Iterations", roadSmoothingIterations, 1, 10);   
                }

                EditorGUI.indentLevel--;
            }
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
                        lSystemStrategyModel.Display();
                        break;
                    case RoadNetworkStrategy.AStar:
                        aStarStrategyModel.Display();
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
            switch (roadNetworkStrategy)
            {
                case RoadNetworkStrategy.LSystem:
                    _generator.RoadNetworkStrategy = lSystemStrategyModel;
                    break;
                case RoadNetworkStrategy.AStar:
                    _generator.RoadNetworkStrategy = aStarStrategyModel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _generator.PlotStrategy = _plotStrategies[plotStrategy];
            
            return _generator.Generate();
        }
        
    }
}