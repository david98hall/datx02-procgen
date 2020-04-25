﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cities.Plots;
using Interfaces;
using Services;
using Terrain;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities.Buildings
{
    /// <summary>
    /// The view model for building related generators.
    /// </summary>
    [Serializable]
    public class BuildingViewModel : ViewModelStrategy<(TerrainInfo, IEnumerable<Plot>), IEnumerable<Building>>
    {
        
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

        internal override IInjector<(TerrainInfo, IEnumerable<Plot>)> Injector
        {
            get => base.Injector;
            set
            {
                try
                {
                    extrusionStrategy.Injector = value;
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
                    extrusionStrategy.EventBus = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
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
                    extrusionStrategy.CancelToken = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }
        
        public override void Display()
        {
            _buildingStrategyVisible = EditorGUILayout.Foldout(_buildingStrategyVisible, "Building Generation");

            if (!_buildingStrategyVisible) return;

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

            if (!_buildingAppearanceVisible) return;

            EditorGUI.indentLevel++;

            // Material
            buildingMaterial = (Material)EditorGUILayout.ObjectField(
                "Building Material", buildingMaterial, typeof(Material), true);

            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// Generates buildings with the current strategy from a enumerable of plots
        /// </summary>
        /// <returns>An enumerable object of buildings.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override IEnumerable<Building> Generate()
        {
            switch (buildingStrategy)
            {
                case BuildingStrategy.Extrusion:
                    return extrusionStrategy.Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}