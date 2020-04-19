using System;
using System.Collections.Generic;
using System.Linq;
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
        private IList<Vector2> origins;

        /// <summary>
        /// Serialized number of rewrites of the L-system.
        /// </summary>
        [SerializeField]
        private int rewritesCount;
        
        #endregion

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            origins = new List<Vector2>{Vector2.zero};
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            // Display a slider for the number of rewrites the L-system
            // will go through before returning the road network
            rewritesCount = EditorGUILayout.IntSlider("Rewrite Count", rewritesCount, 3, 7);
            
            // Control for adding a new origin point
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Origins");
            
            if (GUILayout.Button("Add"))
            {
                origins.Add(Vector2.zero);
            }
            
            // Control for discarding all origin points
            if (origins.Any() && GUILayout.Button("Discard All"))
            {
                origins.Clear();
            }
            
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            
            for (var i = 0; i < origins.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Origin {i + 1}");
                
                // Origin remove button
                var removed = GUILayout.Button("X");
                GUILayout.EndHorizontal();
                if (removed)
                {
                    origins.RemoveAt(i);
                    continue;
                }
                
                // Origin vector field
                origins[i] = EditorGUILayout.Vector2Field("", origins[i]);
                
                EditorGUILayout.Space();
            }
            
        }
        
        
        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override RoadNetwork Generate()
        {
            RoadNetwork roadNetwork = null;
            foreach (var origin in origins)
            {
                var tmpRoadNetwork = _roadStrategyFactory.CreateLSystemStrategy(origin, rewritesCount).Generate();
                if (roadNetwork == null)
                {
                    roadNetwork = tmpRoadNetwork;
                }
                else
                {
                    roadNetwork.Merge(tmpRoadNetwork);
                }
            }

            return roadNetwork;
        }
    }
}