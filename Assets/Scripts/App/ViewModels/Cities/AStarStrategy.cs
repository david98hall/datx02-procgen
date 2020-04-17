using System;
using System.Collections.Generic;
using Cities.Roads;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Cities
{
    [Serializable]
    public class AStarStrategy : ViewModelStrategy<float[,], RoadNetwork>
    {
        [SerializeField]
        private float heightBias;
        
        [SerializeField]
        private IList<(Vector2Int, Vector2Int)> paths;
        
        private Factory _roadStrategyFactory;

        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            paths = new List<(Vector2Int, Vector2Int)> {(Vector2Int.zero, Vector2Int.zero)};
        }
        
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
        
        public override RoadNetwork Generate() => 
            _roadStrategyFactory?.CreateAStarStrategy(heightBias, paths).Generate();
        
    }
}