using System;
using System.Collections.Generic;
using Cities.Plots;
using Cities.Roads;
using Interfaces;
using Terrain;
using UnityEditor;
using UnityEngine;
using Factory = Cities.Plots.Factory;

namespace App.ViewModels.Cities.Plots
{
    /// <summary>
    /// The view model for plot related generators.
    /// </summary>
    [Serializable]
    public class PlotViewModel : ViewModelStrategy<(RoadNetwork, TerrainInfo), IEnumerable<Plot>>
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
            MinimalCycle, Adjacent, Combined // ClockWiseCycle, BruteMinimalCycle,
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
            displayPlots = EditorGUILayout.Toggle("Display Plots", displayPlots);
            if (displayPlots)
            {
                plotMaterial = (Material) EditorGUILayout.ObjectField(
                    "Plot Material", plotMaterial, typeof(Material), true);   
            }
            EditorGUI.indentLevel--;
        }

        public override IEnumerable<Plot> Generate()
        {
            // We can't generate plots with no roads
            if (Injector.Get().Item1 == null) return null;
         
            EventBus.CreateEvent(AppEvent.GenerationStart, "Generating Plots", this);
            
            var plotStrategyFactory = new Factory(Injector);
            IGenerator<IEnumerable<Plot>> generator;
            switch (plotStrategy)
            {
                case PlotStrategy.MinimalCycle:
                    generator = plotStrategyFactory.CreateMinimalCycleStrategy();
                    break;
                /*
                case PlotStrategy.ClockWiseCycle:
                    generator = plotStrategyFactory.CreateClockwiseCycleStrategy();
                    break;
                case PlotStrategy.BruteMinimalCycle:
                    generator = plotStrategyFactory.CreateBruteMinimalCycleStrategy();
                    break;
                */
                case PlotStrategy.Adjacent:
                    generator = plotStrategyFactory.CreateAdjacentStrategy();
                    break;
                case PlotStrategy.Combined:
                    generator = plotStrategyFactory.CreateCombinedStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Set the cancellation token so that the generation can be canceled
            generator.CancelToken = CancelToken;
            var plots = generator.Generate();

            EventBus.CreateEvent(AppEvent.GenerationEnd, "Generated Plots", this);
            return plots;
        }
    }
}