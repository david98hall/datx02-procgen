using System;
using Cities.Roads;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class LSystemStrategyModel : IViewAdapter<IGenerator<RoadNetwork>>
    {
        private LSystemStrategy _strategy;

        // The start point for the generation
        [HideInInspector] 
        public Vector2 origin;
        
        // How many times the L-system will be rewritten
        [HideInInspector] 
        public int rewritesCount = 3;
        
        public IGenerator<RoadNetwork> Model
        {
            get
            {
                // Update strategy parameters
                _strategy.Origin = origin;
                _strategy.RewritesCount = rewritesCount;
                return _strategy;
            }
            set => _strategy = value as LSystemStrategy;
        }

        public void Display()
        {
            // Display a field for setting the start point of the L-system generation
            origin = EditorGUILayout.Vector2Field("Origin", origin);
            
            // Display a slider for the number of rewrites the L-system
            // will go through before returning the road network
            rewritesCount = EditorGUILayout.IntSlider("Rewrite Count", rewritesCount, 3, 7);
        }
    }
}