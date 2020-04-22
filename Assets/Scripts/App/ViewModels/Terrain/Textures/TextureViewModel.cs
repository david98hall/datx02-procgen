using System;
using Interfaces;
using Services;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Noise.Textures
{
    /// <summary>
    /// The view model for terrain texture generation.
    /// </summary>
    [Serializable]
    public class TextureViewModel : ViewModelStrategy<float[,], Texture2D>
    {
        #region Texture Strategy 

        /// <summary>
        /// Visibility of the texture strategy editor.
        /// </summary>
        private bool _textureStrategyVisible;
        
        /// <summary>
        /// Enum for texture strategies.
        /// Is used for displaying the possible strategies in the editor.
        /// </summary>
        public enum TextureStrategy
        {
            GrayScale, Whittaker
        }

        /// <summary>
        /// Serialized texture strategy that is currently selected.
        /// </summary>
        [SerializeField]
        private TextureStrategy textureStrategy;
        
        /// <summary>
        /// Serialized view-model for <see cref="WhittakerStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField] 
        private WhittakerStrategy whittakerStrategy;

        /// <summary>
        /// Serialized view-model for <see cref="GrayScaleStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField] 
        private GrayScaleStrategy grayScaleStrategy;

        #endregion

        internal override IInjector<float[,]> Injector
        {
            get => base.Injector;
            set
            {
                base.Injector = value;
                try
                {   
                    whittakerStrategy.Injector = value;
                    grayScaleStrategy.Injector = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    whittakerStrategy.EventBus = value;
                    grayScaleStrategy.EventBus = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Displays the editor of texture and the view model of the currently selected texture strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override void Display()
        {
            _textureStrategyVisible = EditorGUILayout.Foldout(_textureStrategyVisible, "Texture Generation");
            if (!_textureStrategyVisible) return;
            
            EditorGUI.indentLevel++;
            textureStrategy = (TextureStrategy) EditorGUILayout.EnumPopup("Strategy", textureStrategy);

            EditorGUI.indentLevel++;
            switch (textureStrategy)
            {
                case TextureStrategy.GrayScale:
                    grayScaleStrategy.Display();
                    break;
                case TextureStrategy.Whittaker:
                    whittakerStrategy.Display();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Generates a texture with the selected strategy.
        /// </summary>
        /// <returns>A texture object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        public override Texture2D Generate()
        {
            switch (textureStrategy)
            {
                case TextureStrategy.GrayScale:
                    return grayScaleStrategy.Generate();
                case TextureStrategy.Whittaker:
                    return whittakerStrategy.Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}