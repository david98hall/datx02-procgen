using System;
using Terrain.Textures;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Terrain
{
    /// <summary>
    /// View-model for displaying and generating textures with the Whittaker strategy.
    /// </summary>
    [Serializable]
    public class WhittakerStrategy : ViewModelStrategy<float[,], Texture2D>
    {
        /// <summary>
        /// Underlying <see cref="Factory"/> for creating the Whittaker strategy object
        /// </summary>
        private Factory _textureStrategyFactory;

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
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize() => _textureStrategyFactory = new Factory(Injector);

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            precipitationScale = EditorGUILayout.Slider("Precipitation scale", precipitationScale, 1, 100);
            temperatureScale = EditorGUILayout.Slider("Temperature scale", temperatureScale, 1, 100);
        }
        
        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override Texture2D Generate() => 
            _textureStrategyFactory?.CreateWhittakerStrategy(precipitationScale, temperatureScale).Generate();
    }
}