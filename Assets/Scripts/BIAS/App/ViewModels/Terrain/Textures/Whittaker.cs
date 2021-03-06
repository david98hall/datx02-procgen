using System;
using BIAS.PCG.Textures;
using UnityEditor;
using UnityEngine;

namespace BIAS.App.ViewModels.Terrain.Textures
{
    /// <summary>
    /// View-model for displaying and generating textures with the Whittaker strategy.
    /// </summary>
    [Serializable]
    public class Whittaker : ViewModel<float[,], Texture2D>
    {
        #region Editor Fields
        
        /// <summary>
        /// Serialized precipitation scale.
        /// </summary>
        [SerializeField] 
        private float precipitationScale;
        
        /// <summary>
        /// Serialized temperature scale.
        /// </summary>
        [SerializeField]
        private float temperatureScale;

        #endregion

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            precipitationScale = EditorGUILayout.Slider(
                "Precipitation scale", precipitationScale, 1, 100);
            temperatureScale = EditorGUILayout.Slider(
                "Temperature scale", temperatureScale, 1, 100);
        }

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override Texture2D Generate()
        {
            var generator = new Factory(Injector).CreateWhittakerStrategy(precipitationScale, temperatureScale);
            // Set the cancellation token so that the generation can be canceled
            generator.CancelToken = CancelToken;
            return generator.Generate();
        }
    }
}