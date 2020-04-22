using System;
using System.Collections.Generic;
using Cities.Plots;
using Cities.Roads;
using UnityEditor;
using UnityEngine;
using Factory = Cities.Plots.Factory;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// The view model for plot related generators.
    /// </summary>
    [Serializable]
    public class PlotViewModel : ViewModelStrategy<RoadNetwork, IEnumerable<Plot>>
    {
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
        private bool _displayPlots;
        
        /// <summary>
        /// Getter for the boolean if plots are visible.
        /// </summary>
        public bool DisplayPlots => _displayPlots;

        #endregion

        /// <summary>
        /// Displays the editor of plots and the view model of the currently selected plot strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override void Display()
        {
            _plotStrategyVisible = EditorGUILayout.Foldout(_plotStrategyVisible, "Plot Generation");
            if (!_plotStrategyVisible) return;
            
            EditorGUI.indentLevel++;
            plotStrategy = (PlotStrategy) EditorGUILayout.EnumPopup("Strategy", plotStrategy);
            _displayPlots = EditorGUILayout.Toggle("Display Plots", _displayPlots);
            if (_displayPlots)
            {
                plotMaterial = (Material) EditorGUILayout.ObjectField(
                    "Plot Material", plotMaterial, typeof(Material), true);   
            }
            EditorGUI.indentLevel--;
        }

        public override IEnumerable<Plot> Generate()
        {
            if (InjectedValue == null) return null;
            
            var plotStrategyFactory = new Factory(Injector);

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
    }
}