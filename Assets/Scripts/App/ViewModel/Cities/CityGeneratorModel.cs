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
        private AStarStrategyModel _aStarStrategyModel;

        [SerializeField] 
        private LSystemStrategyModel _lSystemStrategyModel;
        
        public enum RoadNetworkStrategy
        {
            LSystem, AStar
        }

        [SerializeField]
        private RoadNetworkStrategy _roadNetworkStrategy;

        #endregion
        
        #region Plot Strategy
        
        private bool _plotStrategyVisible;
        private Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>> _plotStrategies;
        
        public enum PlotStrategy
        {
            MinimalCycle, ClockWiseCycle, BruteMinimalCycle
        }

        [SerializeField] 
        private PlotStrategy _plotStrategy;

        public Material PlotMaterial
        {
            get => _plotMaterial;
            set => _plotMaterial = value;
        }
        
        [SerializeField] 
        private Material _plotMaterial;

        public bool DisplayPlots
        {
            get => _displayPlots;
            set => _displayPlots = value;
        }
        
        [SerializeField] 
        private bool _displayPlots;
        
        #endregion
        
        #region Road Appearance fields
        
        private bool _roadAppearanceVisible;

        public float RoadWidth
        {
            get => _roadWidth;
            set => _roadWidth = value;
        }
        
        [SerializeField]
        private float _roadWidth = 0.3f;
        
        
        public float RoadCurvature
        {
            get => _roadCurvature;
            set => _roadCurvature = value;
        }
        
        [SerializeField]
        private float _roadCurvature = 0.1f;
        
        public int RoadSmoothingIterations
        {
            get => _roadSmoothingIterations;
            set => _roadSmoothingIterations = value;
        }
        
        [SerializeField]
        private int _roadSmoothingIterations = 1;

        public Material RoadMaterial
        {
            get => _roadMaterial;
            set => _roadMaterial = value;
        }
        
        [SerializeField]
        private Material _roadMaterial;

        public float RoadTerrainOffsetY
        {
            get => _roadTerrainOffsetY;
            set => _roadTerrainOffsetY = value;
        }
        
        [SerializeField]
        private float _roadTerrainOffsetY = 0.075f;
        
        #endregion

        public override void Initialize()
        {
            _generator = new CityGenerator();
            
            // Road network strategies
            _aStarStrategyModel = new AStarStrategyModel();
            _lSystemStrategyModel = new LSystemStrategyModel();
            
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
                _roadMaterial = (Material) EditorGUILayout.ObjectField(
                    "Material", _roadMaterial, typeof(Material), true);
                
                // Road width
                _roadWidth = EditorGUILayout.Slider("Width", _roadWidth, 0.1f, 10);
                
                // Road/Terrain y-offset
                _roadTerrainOffsetY = EditorGUILayout.FloatField("Y-Offset", _roadTerrainOffsetY);

                // Road curvature and smoothing
                _roadCurvature = EditorGUILayout.Slider("Curvature", _roadCurvature, 0, 0.5f);
                if (_roadCurvature > 0)
                {
                    _roadSmoothingIterations = EditorGUILayout.IntSlider(
                        "Smoothing Iterations", _roadSmoothingIterations, 1, 10);   
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
                _roadNetworkStrategy 
                    = (RoadNetworkStrategy) EditorGUILayout.EnumPopup("Strategy", _roadNetworkStrategy);
                EditorGUI.indentLevel++;
                switch (_roadNetworkStrategy)
                {
                    case RoadNetworkStrategy.LSystem:
                        _lSystemStrategyModel.Display();
                        break;
                    case RoadNetworkStrategy.AStar:
                        _aStarStrategyModel.Display();
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
                _plotStrategy = (PlotStrategy) EditorGUILayout.EnumPopup("Strategy", _plotStrategy);
                _displayPlots = EditorGUILayout.Toggle("Display Plots", _displayPlots);
                if (_displayPlots)
                {
                    _plotMaterial = (Material) EditorGUILayout.ObjectField(
                        "Plot Material", _plotMaterial, typeof(Material), true);   
                }
                EditorGUI.indentLevel--;
            }
        }

        public override City Generate()
        {
            switch (_roadNetworkStrategy)
            {
                case RoadNetworkStrategy.LSystem:
                    _generator.RoadNetworkStrategy = _lSystemStrategyModel;
                    break;
                case RoadNetworkStrategy.AStar:
                    _generator.RoadNetworkStrategy = _aStarStrategyModel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _generator.PlotStrategy = _plotStrategies[_plotStrategy];
            
            return _generator.Generate();
        }
        
    }
}