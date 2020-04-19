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

        /// <summary>
        /// Serialized L-system generation input data.
        /// </summary>
        [SerializeField]
        private IList<(Vector2 Origin, int Rewrites)> inputs;

        private static readonly int _minRewrites = 3;
        private static readonly int _maxRewrites = 7;
        
        private static readonly (Vector2 Origin, int Rewrites) _defaultInput = 
            (Vector2.zero, (_maxRewrites - _minRewrites) / 2 + _minRewrites);
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            inputs = new List<(Vector2 Origin, int Rewrites)>{_defaultInput};
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            GUILayout.BeginHorizontal();
            // Label
            EditorGUILayout.LabelField("L-systems");
            
            // Clearing
            if (inputs.Any() && GUILayout.Button("Clear")) inputs.Clear();
            
            // Adding
            if (GUILayout.Button("+")) inputs.Add(_defaultInput);
            
            GUILayout.EndHorizontal();
            
            // Input list
            EditorGUI.indentLevel++;
            for (var i = 0; i < inputs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"L-system {i + 1}");
                
                // Origin remove button
                var removed = GUILayout.Button("X");
                GUILayout.EndHorizontal();
                if (removed)
                {
                    inputs.RemoveAt(i);
                    continue;
                }

                var (origin, rewrites) = inputs[i];
                
                // Origin vector field
                var newOrigin = EditorGUILayout.Vector2Field("Origin", origin);
                
                // Rewrites Count field
                var newRewrites = EditorGUILayout.IntSlider("Rewrites", rewrites, _minRewrites, _maxRewrites);
                
                inputs[i] = (newOrigin, newRewrites);

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
            foreach (var (origin, rewrites) in inputs)
            {
                var tmpRoadNetwork = _roadStrategyFactory.CreateLSystemStrategy(origin, rewrites).Generate();
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