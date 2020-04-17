using System;
using Cities.Roads;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// A view model strategy for displaying settings for generating road networks with the L-system strategy. 
    /// </summary>
    [Serializable]
    public class LSystemStrategy : ViewModelStrategy<float[,], RoadNetwork>
    {
        #region UI Fields

        [SerializeField]
        private Vector2 origin;

        [SerializeField]
        private int rewritesCount;
        #endregion
        
        private Factory _roadStrategyFactory;

        public override void Initialize() => _roadStrategyFactory = new Factory(Injector);
        
        public override void Display()
        {
            // Display a field for setting the start point of the L-system generation
            origin = EditorGUILayout.Vector2Field("Origin", origin);
            
            // Display a slider for the number of rewrites the L-system
            // will go through before returning the road network
            rewritesCount = EditorGUILayout.IntSlider("Rewrite Count", rewritesCount, 3, 7);
        }
        
        public override RoadNetwork Generate() => 
            _roadStrategyFactory.CreateLSystemStrategy(origin, rewritesCount).Generate();
    }
}