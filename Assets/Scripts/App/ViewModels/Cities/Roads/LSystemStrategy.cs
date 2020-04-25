using System;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Terrain;
using UnityEditor;
using UnityEngine;
using Utils;

namespace App.ViewModels.Cities.Roads
{
    /// <summary>
    /// View-model for displaying and generating road networks with the L-system strategy.
    /// It is injected with a tuple of terrain data: its offset in the scene, and its height map.
    /// This data is used to generate road networks with an L-system algorithm.
    /// </summary>
    [Serializable]
    public class LSystemStrategy : ViewModelStrategy<TerrainInfo, RoadNetwork>
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
        /// Serialized L-system generation input data.
        /// </summary>
        [SerializeField]
        private List<Input> inputs;

        private static readonly int MinRewrites = 3;
        private static readonly int MaxRewrites = 7;
        private static readonly int DefaultRewrites = (MaxRewrites - MinRewrites) / 2 + MinRewrites;

        // Fields based on events
        private (int Width, int Depth) _terrainSize;

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            GUILayout.BeginHorizontal();
            // Label
            EditorGUILayout.LabelField("Entries");
            
            // Clearing
            if (inputs.Any() && GUILayout.Button("Clear")) inputs.Clear();
            
            // Adding
            var initialOrigin = GetTerrainCenter().ToTerrainVertex(_terrainSize.Width, _terrainSize.Depth);
            if (GUILayout.Button("+")) inputs.Add(new Input(initialOrigin, DefaultRewrites));
            
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
                var editorOrigin = EditorGUILayout.Vector2Field("Origin", inputs[i].origin);
                var newOrigin = editorOrigin.ToTerrainVertex(_terrainSize.Width, _terrainSize.Depth);
                
                // Rewrites Count field
                var newRewrites = EditorGUILayout.IntSlider("Rewrites", inputs[i].rewrites, MinRewrites, MaxRewrites);
                
                inputs[i] = new Input(newOrigin, newRewrites);

                EditorGUILayout.Space();
            }
            
        }

        private Vector2 GetTerrainCenter() => new Vector2(_terrainSize.Width / 2f, _terrainSize.Depth / 2f);

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override RoadNetwork Generate()
        {
            // Concurrent generation of L-system road networks
            return TaskUtils.RunActionInTasks(inputs, input =>
                    {
                        var generator = new Factory()
                            .CreateLSystemStrategy(Injector, input.origin, input.rewrites);
                        // Set the cancellation token so that the generation can be canceled
                        generator.CancelToken = CancelToken;
                        return generator.Generate();
                    }, 
                    CancelToken)
                // Merge all L-system road networks into one
                ?.Aggregate(new RoadNetwork(), (r1, r2) => r1.Merge(r2));
        }

        public override void OnEvent(AppEvent eventId, object eventData, object creator)
        {
            if (eventId.Equals(AppEvent.UpdateNoiseMapSize))
            {
                // If the noise map size is changed, update it here as well
                _terrainSize = ((int, int)) eventData;
            }
        }
    }
}