using System;
using Cities.Roads;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating road networks with the L-system strategy.
    /// </summary>
    [Serializable]
    public class LSystemStrategy : ViewModelStrategy<float[,], RoadNetwork>
    {

        /// <summary>
        /// Underlying <see cref="Factory"/> for creating the A* strategy object
        /// </summary>
        private Factory _roadStrategyFactory;

        #region Editor Fields

        /// <summary>
        /// Serialized origin of the L-system.
        /// </summary>
        [SerializeField]
        private Vector2 origin;

        /// <summary>
        /// Serialized number of rewrites of the L-system.
        /// </summary>
        [SerializeField]
        private int rewritesCount;
        
        #endregion
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize() => _roadStrategyFactory = new Factory(Injector);
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            // Display a field for setting the start point of the L-system generation
            origin = EditorGUILayout.Vector2Field("Origin", origin);
            
            // Display a slider for the number of rewrites the L-system
            // will go through before returning the road network
            rewritesCount = EditorGUILayout.IntSlider("Rewrite Count", rewritesCount, 3, 7);
        }
        
        
        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override RoadNetwork Generate() => 
            _roadStrategyFactory.CreateLSystemStrategy(origin, rewritesCount).Generate();
    }
}