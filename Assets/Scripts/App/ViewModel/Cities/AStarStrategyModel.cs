using System;
using System.Collections.Generic;
using Cities.Roads;
using Interfaces;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Cities
{
    [Serializable]
    public class AStarStrategyModel : IViewAdapter<IGenerator<RoadNetwork>>
    {
        private AStarStrategy _aStarStrategy;

        [HideInInspector] 
        public float heightBias;
        
        [HideInInspector]
        public List<Path> paths;

        public IGenerator<RoadNetwork> Model
        {
            get
            {
                _aStarStrategy.HeightBias = heightBias;
                foreach (var path in paths)
                {
                    _aStarStrategy.Add(path.start, path.goal);
                }
                return _aStarStrategy;
            }
            set => _aStarStrategy = value as AStarStrategy;
        }

        public void Display()
        {
            heightBias = EditorGUILayout.Slider("Height Bias", heightBias, 0, 1);
            
            EditorGUILayout.LabelField("Paths");
            EditorGUI.indentLevel++;
            foreach (var node in paths)
            {
                node.start = EditorGUILayout.Vector2IntField("Start", node.start);
                node.goal = EditorGUILayout.Vector2IntField("Goal", node.goal);
                EditorGUILayout.Space();
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Path"))
            {
                paths.Add(new Path());
            }
            
            if (GUILayout.Button("Clear Paths"))
            {
                paths.Clear();
            }
            
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        
        [Serializable]
        public class Path
        {
            public Vector2Int start;
            public Vector2Int goal;
        }
    }
}