using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            paths = new List<(Vector2Int, Vector2Int)> {(Vector2Int.zero, Vector2Int.zero)};
        }
        
        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override void Display()
        {
            // Update the height bias
            heightBias = EditorGUILayout.Slider("Height Bias", heightBias, 0, 1);
            
            // Update the paths
            EditorGUILayout.LabelField("Paths");
            EditorGUI.indentLevel++;
            for (var i = 0; i < paths.Count; i++)
            {
                var (start, goal) = paths[i];
                var newStart = EditorGUILayout.Vector2IntField("Start", start);
                var newGoal = EditorGUILayout.Vector2IntField("Goal", goal);
                paths[i] = (newStart, newGoal);
                EditorGUILayout.Space();
            }
            
            // Control for adding a new path
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Path"))
            {
                paths.Add((Vector2Int.zero, Vector2Int.zero));
            }
            
            // Control for discarding all paths
            if (GUILayout.Button("Discard Paths"))
            {
                paths.Clear();
            }
            
            GUILayout.EndHorizontal();
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