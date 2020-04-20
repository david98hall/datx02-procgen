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

        [Serializable]
        private class Input
        {
            public Vector2 origin;
            public int rewrites;

            public Input(Vector2 origin, int rewrites)
            {
                this.origin = origin;
                this.rewrites = rewrites;
            }
        }
        
        /// <summary>
        /// Underlying <see cref="Factory"/> for creating the A* strategy object
        /// </summary>
        private Factory _roadStrategyFactory;

        /// <summary>
        /// Serialized L-system generation input data.
        /// </summary>
        [SerializeField]
        private IList<Input> inputs;

        private static readonly int _minRewrites = 3;
        private static readonly int _maxRewrites = 7;
        private static readonly int _defaultRewrites = (_maxRewrites - _minRewrites) / 2 + _minRewrites;
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            inputs = new List<Input>{new Input(Vector2.zero, _defaultRewrites)};
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
            if (GUILayout.Button("+")) inputs.Add(new Input(Vector2.zero, _defaultRewrites));
            
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
                    inputs.RemoveAt(i--);
                    continue;
                }

                // Origin vector field
                var newOrigin = EditorGUILayout.Vector2Field("Origin", inputs[i].origin);
                
                // Rewrites Count field
                var newRewrites = EditorGUILayout.IntSlider("Rewrites", inputs[i].rewrites, _minRewrites, _maxRewrites);
                
                inputs[i] = new Input(newOrigin, newRewrites);

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
            foreach (var input in inputs)
            {
                var tmpNetwork = _roadStrategyFactory.CreateLSystemStrategy(input.origin, input.rewrites).Generate();
                if (roadNetwork == null)
                {
                    roadNetwork = tmpNetwork;
                }
                else
                {
                    roadNetwork.Merge(tmpNetwork);
                }
            }

            return roadNetwork;
        }
    }
}