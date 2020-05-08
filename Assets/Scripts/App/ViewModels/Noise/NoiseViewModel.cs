using System;
using System.Threading;
using Services;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Noise
{
    /// <summary>
    /// The view model for noise generation.
    /// </summary>
    [Serializable]
    public class NoiseViewModel : ViewModel<object, float[,]>
    {
        
        #region Noise Strategy

        /// <summary>
        /// Visibility of the noise strategy editor.
        /// </summary>
        private bool _noiseStrategyVisible;
        
        /// <summary>
        /// Enum for noise strategies.
        /// Is used for displaying the possible strategies in the editor.
        /// </summary>
        public enum NoiseStrategy
        {
            PerlinNoise
        }
        
        /// <summary>
        /// Serialized noise strategy that is currently selected.
        /// </summary>
        [SerializeField]
        private NoiseStrategy noiseStrategy;

        /// <summary>
        /// Serialized view-model for <see cref="PerlinNoise"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField]
        private PerlinNoise perlinNoise;

        #endregion

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    perlinNoise.EventBus = value;
                }
                catch (NullReferenceException)
                {}
            }
        }

        public override CancellationToken CancelToken
        {
            get => base.CancelToken;
            set
            {
                base.CancelToken = value;
                try
                {   
                    perlinNoise.CancelToken = value;
                }
                catch (NullReferenceException)
                {}
            }
        }
        
        /// <summary>
        /// Displays the editor of noise and the view model of the currently selected noise strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override void Display()
        {
            _noiseStrategyVisible = EditorGUILayout.Foldout(_noiseStrategyVisible, "Noise Generation");
            if (!_noiseStrategyVisible) return;
            
            EditorGUI.indentLevel++;
            noiseStrategy = (NoiseStrategy) EditorGUILayout.EnumPopup("Strategy", noiseStrategy);

            EditorGUI.indentLevel++;
            switch (noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    perlinNoise.Display();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        public override float[,] Generate()
        {
            switch (noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    return perlinNoise.Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}