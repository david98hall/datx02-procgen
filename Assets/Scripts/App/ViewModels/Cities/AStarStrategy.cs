using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    /// <summary>
    /// View-model for displaying and generating road networks with the A* strategy.
    /// </summary>
    [Serializable]
    public class AStarStrategy : ViewModelStrategy<float[,], RoadNetwork>
    {
        /// <summary>
        /// Underlying <see cref="Factory"/> for creating the A* strategy object
        /// </summary>
        private Factory _roadStrategyFactory;

        #region Editor fields

        /// <summary>
        /// Serialized height bias
        /// </summary>
        [SerializeField]
        private float heightBias;
        
        /// <summary>
        /// Serialized start and goal paths
        /// </summary>
        [SerializeField]
        private IList<(Vector2Int, Vector2Int)> paths;

        #endregion

        private static readonly (Vector2Int Start, Vector2Int Goal) _defaultPath = (Vector2Int.zero, Vector2Int.zero);
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            paths = new List<(Vector2Int, Vector2Int)> {_defaultPath};
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override void Display()
        {
            // Update the height bias
            heightBias = EditorGUILayout.Slider("Height Bias", heightBias, 0, 1);
            
            // Control for adding a new path
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Paths");
            
            // Clear
            if (paths.Any() && GUILayout.Button("Clear")) paths.Clear();
            
            // Add
            if (GUILayout.Button("+")) paths.Add(_defaultPath);

            GUILayout.EndHorizontal();
            
            // Path list
            EditorGUI.indentLevel++;
            for (var i = 0; i < paths.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Path {i + 1}");
                
                // Path remove button
                var removed = GUILayout.Button("X");
                GUILayout.EndHorizontal();
                if (removed)
                {
                    paths.RemoveAt(i);
                    continue;
                }

                var (start, goal) = paths[i];
                
                // Path start vector field
                var newStart = EditorGUILayout.Vector2IntField("Start", start);

                // Path end vector field
                var newGoal = EditorGUILayout.Vector2IntField("Goal", goal);
                paths[i] = (newStart, newGoal);

                EditorGUILayout.Space();
            }
            
            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override RoadNetwork Generate() => 
            _roadStrategyFactory?.CreateAStarStrategy(heightBias, paths).Generate();
    }
}