using System;
using System.Collections.Generic;
using Cities.Buildings;
using Cities.Plots;
using Terrain;
using UnityEditor;
using UnityEngine;
using Factory = Cities.Buildings.Factory;

namespace App.ViewModels.Cities.Buildings
{

    /// <summary>
    /// The view model for using the extrusion algorithm for generating buildings.
    /// </summary>
    [Serializable]
    public class Extrusion : ViewModel<(TerrainInfo, IEnumerable<Plot>), IEnumerable<Building>>
    {

        #region Editor Fields

        /// <summary>
        /// Minimal building size.
        /// </summary>
        [SerializeField]
        private float minArea;

        /// <summary>
        /// Maximal building size.
        /// </summary>
        [SerializeField]
        private float maxArea;

        #endregion

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            // Display a field for minimal building size
            minArea = EditorGUILayout.FloatField("Min Size", minArea);

            // Display a field for maximal building size
            maxArea = EditorGUILayout.FloatField("Max Size", maxArea);
        }


        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override IEnumerable<Building> Generate()
        {
            var generator = new Factory(Injector).CreateExtrusionStrategy(minArea, maxArea);
            // Set the cancellation token so that the generation can be canceled
            generator.CancelToken = CancelToken;
            return generator.Generate();
        }
    }
}