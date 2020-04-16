using System;
using System.Collections.Generic;
using Cities.Roads;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class AStarStrategyModel : EditorStrategyView<float[,], RoadNetwork>
    {
        [SerializeField]
        private float _heightBias;
        
        [SerializeField]
        private IList<(Vector2Int Start, Vector2Int Goal)> _paths;
        
        private Factory _roadStrategyFactory;

        public override void Initialize()
        {
            _roadStrategyFactory = new Factory(Injector);
            _paths = new List<(Vector2Int Start, Vector2Int Goal)> {(Vector2Int.zero, Vector2Int.zero)};
        }
        
        public override void Display()
        {
            // Update the height bias
            _heightBias = EditorGUILayout.Slider("Height Bias", _heightBias, 0, 1);
            
            // Update the paths
            EditorGUILayout.LabelField("Paths");
            EditorGUI.indentLevel++;
            for (var i = 0; i < _paths.Count; i++)
            {
                var (start, goal) = _paths[i];
                var newStart = EditorGUILayout.Vector2IntField("Start", start);
                var newGoal = EditorGUILayout.Vector2IntField("Goal", goal);
                _paths[i] = (newStart, newGoal);
                EditorGUILayout.Space();
            }
            
            // Control for adding a new path
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Path"))
            {
                _paths.Add((Vector2Int.zero, Vector2Int.zero));
            }
            
            // Control for discarding all paths
            if (GUILayout.Button("Discard Paths"))
            {
                _paths.Clear();
            }
            
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        
        public override RoadNetwork Generate() => 
            _roadStrategyFactory?.CreateAStarStrategy(_heightBias, _paths).Generate();
        
    }
}