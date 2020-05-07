using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Parallelism;

namespace App.ViewModels.Cities.Roads
{
    /// <summary>
    /// View-model for displaying and generating road networks with the A* strategy.
    /// </summary>
    [Serializable]
    public class AStarStrategy : ViewModelStrategy<float[,], RoadNetwork>
    {
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
        private List<Path> paths;

        private (int Width, int Depth) _terrainSize;

        #endregion

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
            var initialGoal = new Vector2Int(_terrainSize.Width - 1, _terrainSize.Depth - 1);
            if (GUILayout.Button("+")) paths.Add(new Path(Vector2Int.zero, initialGoal));

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
                    paths.RemoveAt(i--);
                    continue;
                }

                // Path start vector field
                paths[i].start = EditorGUILayout.Vector2IntField("Start", paths[i].start)
                    .ToTerrainVertex(_terrainSize.Width, _terrainSize.Depth);

                // Path end vector field
                paths[i].goal = EditorGUILayout.Vector2IntField("Goal", paths[i].goal)
                    .ToTerrainVertex(_terrainSize.Width, _terrainSize.Depth);

                EditorGUILayout.Space();
            }
            
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override RoadNetwork Generate()
        {
            // Concurrent generation of A* road networks
            return TaskUtils.RunActionInTasks(paths, path =>
                    {
                        var generator = new Factory()
                            .CreateAStarStrategy(Injector, heightBias, new []{(path.start, path.goal)});
                        // Set the cancellation token so that the generation can be canceled
                        generator.CancelToken = CancelToken;
                        return generator.Generate();
                    },
                    CancelToken)
                // Merge all A* road networks into one
                ?.Aggregate(new RoadNetwork(), (r1, r2) => r1.Merge(r2));
        }

        public override void OnEvent(AppEvent eventId, object eventData, object creator)
        {
            base.OnEvent(eventId, eventData, creator);
            if (eventId.Equals(AppEvent.UpdateNoiseMapSize))
            {
                // If the noise map size is changed, update it here as well
                _terrainSize = ((int, int)) eventData;
            }
        }
        
        /// <summary>
        /// Serializable type representing a path with a start and goal vertex.
        /// </summary>
        [Serializable]
        private class Path
        {
            public Vector2Int start;
            public Vector2Int goal;

            public Path(Vector2Int start, Vector2Int goal)
            {
                this.start = start;
                this.goal = goal;
            }
        }
    }
}