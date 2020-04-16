using System;
using Cities.Roads;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class LSystemStrategyModel : EditorStrategyView<float[,], RoadNetwork>
    {
        /// <summary>
        /// The start point for the generation
        /// </summary>
        public Vector2 Origin
        {
            get => _origin;
            set => _origin = value;
        }

        [SerializeField]
        private Vector2 _origin;

        /// <summary>
        /// How many times the L-system will be rewritten
        /// </summary>
        public int RewritesCount
        {
            get => _rewritesCount;
            set => _rewritesCount = value;
        }

        [SerializeField]
        private int _rewritesCount;
        
        private Factory roadStrategyFactory;

        public override void Initialize()
        {
            roadStrategyFactory = new Factory(Injector);
        }
        
        public override void Display()
        {
            // Display a field for setting the start point of the L-system generation
            Origin = EditorGUILayout.Vector2Field("Origin", Origin);
            
            // Display a slider for the number of rewrites the L-system
            // will go through before returning the road network
            RewritesCount = EditorGUILayout.IntSlider("Rewrite Count", RewritesCount, 3, 7);
        }

        public override RoadNetwork Generate()
        {
            return roadStrategyFactory.CreateLSystemStrategy(Origin, RewritesCount).Generate();
        }
    }
}