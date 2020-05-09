using System;
using Noise;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Noise
{
    /// <summary>
    /// View-model for displaying and generating noise with the Perlin noise strategy.
    /// </summary>
    [Serializable]
    public class PerlinNoise : ViewModel<object, float[,]>
    {
        #region Editor Fields
        
        /// <summary>
        /// Serialized width.
        /// </summary>
        [SerializeField]
        private int width;
        
        /// <summary>
        /// Serialized depth.
        /// </summary>
        [SerializeField]
        private int depth;
        
        /// <summary>
        /// Serialized seed.
        /// </summary>
        [SerializeField]
        private int seed;
        
        /// <summary>
        /// Serialized scale
        /// </summary>
        [SerializeField]
        private float scale; 
        
        /// <summary>
        /// Serialized number of octaves.
        /// </summary>
        [SerializeField]
        private int numOctaves;
        
        /// <summary>
        /// Serialized persistence.
        /// </summary>
        [SerializeField]
        private float persistence;
        
        /// <summary>
        /// Serialized lacunarity.
        /// </summary>
        [SerializeField]
        private float lacunarity;
        
        /// <summary>
        /// Serialized noise offset.
        /// </summary>
        [SerializeField]
        private Vector2 noiseOffset;

        #endregion

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            DisplaySizeControls();
            seed = EditorGUILayout.IntField("Seed", seed);
            scale = EditorGUILayout.Slider("Scale", scale, 1, 100);
            numOctaves = EditorGUILayout.IntSlider("Number of Octaves", numOctaves, 1, 10);
            persistence = EditorGUILayout.Slider("Persistence", persistence, 0, 1);
            lacunarity = EditorGUILayout.Slider("Lacunarity", lacunarity, 1, 10);
            noiseOffset = EditorGUILayout.Vector2Field("Offset", noiseOffset);
        }

        private void DisplaySizeControls()
        {
            var oldWidth = width;
            var oldDepth = depth;
            width = EditorGUILayout.IntSlider("Width", width, 25, 250);
            depth = EditorGUILayout.IntSlider("Depth", depth, 25, 250);

            if (oldWidth != width || oldDepth != depth)
            {
                EventBus.CreateEvent(AppEvent.UpdateNoiseMapSize, (width, depth), this);
            }
        }

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override float[,] Generate()
        {
            var generator = new Factory().CreatePerlinNoiseStrategy(
                width, depth,
                seed, scale,
                numOctaves, persistence,
                lacunarity, noiseOffset);
            // Set the cancellation token so that the generation can be canceled
            generator.CancelToken = CancelToken;
            return generator.Generate();
        }

        public override void OnEvent(AppEvent eventId, object eventData, object creator)
        {
            base.OnEvent(eventId, eventData, creator);
            if (eventId == AppEvent.Broadcast)
            {
                // If a broadcast event is happening, broadcast the current noise map size
                EventBus.CreateEvent(AppEvent.UpdateNoiseMapSize, (width, depth), this);
            }
        }
    }
}