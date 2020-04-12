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
    public class CityGeneratorModel : IViewAdapter<IGenerator<City>>
    {
        private CityGenerator _generator;
        private Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>> _plotStrategies;

        private bool _visible;
        private bool _roadNetworkStrategyVisible;
        private bool _plotStrategyVisible;

        [SerializeField] 
        private AStarStrategyModel aStarStrategyModel;

        [SerializeField] 
        private LSystemStrategyModel lSystemStrategyModel;
        
        public enum RoadNetworkStrategy
        {
            LSystem, AStar
        }
        
        public enum PlotStrategy
        {
            MinimalCycle, ClockWiseCycle, BruteMinimalCycle
        }

        [HideInInspector]
        public RoadNetworkStrategy roadNetworkStrategy;

        [HideInInspector] 
        public PlotStrategy plotStrategy;

        public IGenerator<City> Model
        {
            get
            {
                switch (roadNetworkStrategy)
                {
                    case RoadNetworkStrategy.LSystem:
                        break;
                    case RoadNetworkStrategy.AStar:
                        _generator.RoadNetworkStrategy = aStarStrategyModel.Model;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _generator.PlotStrategy = _plotStrategies[plotStrategy];
                return _generator;
            }
            set
            {
                _generator = value as CityGenerator;
                var plotStrategyFactory = new Factory(_generator);

                _plotStrategies = new Dictionary<PlotStrategy, IGenerator<IEnumerable<Plot>>>
                {
                    [PlotStrategy.MinimalCycle] = plotStrategyFactory.CreateMinimalCycleStrategy(),
                    [PlotStrategy.ClockWiseCycle] = plotStrategyFactory.CreateClockwiseCycleStrategy(),
                    [PlotStrategy.BruteMinimalCycle] = plotStrategyFactory.CreateBruteMinimalCycleStrategy(),
                };

                var roadNetworkStrategyFactory = _generator?.RoadNetworkFactory;
                aStarStrategyModel.Model = roadNetworkStrategyFactory?.CreateAStarStrategy();
                lSystemStrategyModel.Model = roadNetworkStrategyFactory?.CreateLSystemStrategy();
            }
        }

        public void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"City Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
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
                        break;
                    case RoadNetworkStrategy.AStar:
                        aStarStrategyModel.Display();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            
            _plotStrategyVisible = EditorGUILayout.Foldout(_plotStrategyVisible, "Plot Generation");
            if (_plotStrategyVisible)
            {
                EditorGUI.indentLevel++;
                plotStrategy = (PlotStrategy) EditorGUILayout.EnumPopup("Strategy", plotStrategy);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }
    }
}