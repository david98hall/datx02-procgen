using System;
using System.Threading;
using BIAS.Utils.Interfaces;
using BIAS.Utils.Services;
using BIAS.PCG.Textures;
using UnityEditor;
using UnityEngine;

namespace BIAS.App.ViewModels.Terrain.Textures
{
    /// <summary>
    /// The view model for terrain texture generation.
    /// </summary>
    [Serializable]
    public class TextureViewModel : ViewModel<float[,], Texture2D>
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
            Whittaker, GrayScale
        }

        /// <summary>
        /// Serialized texture strategy that is currently selected.
        /// </summary>
        [SerializeField]
        private TextureStrategy textureStrategy;
        
        /// <summary>
        /// Serialized view-model for <see cref="Whittaker"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField] 
        private Whittaker whittaker = null;

        /// <summary>
        /// Serialized view-model for <see cref="GrayScaleStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        //[SerializeField] 
        //private GrayScaleStrategy grayScaleStrategy;

        #endregion

        internal override IInjector<float[,]> Injector
        {
            get => base.Injector;
            set
            {
                base.Injector = value;
                try
                {   
                    whittaker.Injector = value;
                }
                catch (NullReferenceException)
                {}
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
                    whittaker.EventBus = value;
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
                    whittaker.CancelToken = value;
                }
                catch (NullReferenceException)
                {}
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
            if (textureStrategy == TextureStrategy.Whittaker) 
                whittaker.Display();

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
            EventBus.CreateEvent(AppEvent.GenerationStart, "Generating Terrain Texture", this);
            Texture2D texture;
            switch (textureStrategy)
            {
                case TextureStrategy.GrayScale:
                    var generator = new Factory(Injector).CreateGrayScaleStrategy();
                    // Set the cancellation token so that the generation can be canceled
                    generator.CancelToken = CancelToken;
                    return generator.Generate();
                case TextureStrategy.Whittaker:
                    texture = whittaker.Generate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EventBus.CreateEvent(AppEvent.GenerationEnd, "Generated Terrain Texture", this);
            return texture;
        }
    }
}