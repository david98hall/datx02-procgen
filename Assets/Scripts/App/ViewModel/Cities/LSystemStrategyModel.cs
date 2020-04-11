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
        
        public IGenerator<RoadNetwork> Model
        {
            get
            {
                // Update the origin
                _strategy.Origin = origin;
                return _strategy;
            }
            set => _strategy = value as LSystemStrategy;
        }

        public void Display()
        {
            // Display a field for setting the start point of the L-system generation
            origin = EditorGUILayout.Vector2Field("Origin", origin);
        }
    }
}