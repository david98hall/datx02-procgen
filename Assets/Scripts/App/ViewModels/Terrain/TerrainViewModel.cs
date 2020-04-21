using System;
using Interfaces;
using Terrain.Noise;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Terrain
{
    /// <summary>
    /// View-model for displaying and generating terrain
    /// </summary>
    [Serializable]
    public class TerrainViewModel : ViewModelStrategy<object, (Mesh, Texture2D)>
    {
        /// <summary>
        /// Injector used for storing a valid noise map generated in run-time
        /// </summary>
        private Injector _noiseMapInjector;
        
        /// <summary>
        /// Visibility of the editor.
        /// </summary>
        private bool _visible;
        
        /// <summary>
        /// Serialized height curve.
        /// </summary>
        [SerializeField] 
        private AnimationCurve heightCurve;
        
        /// <summary>
        /// Serialized height scale.
        /// </summary>
        [SerializeField]
        private float heightScale;

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
        /// Serialized view-model for <see cref="PerlinNoiseStrategy"/> view model.
        /// Is required to be explicitly defined to be serializable.
        /// </summary>
        [SerializeField]
        private PerlinNoiseStrategy perlinNoiseStrategy;

        #endregion

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
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _noiseMapInjector = new Injector();

            whittakerStrategy.Injector = _noiseMapInjector;
            grayScaleStrategy.Injector = _noiseMapInjector;
            
            perlinNoiseStrategy.EventBus = EventBus;
            whittakerStrategy.EventBus = EventBus;
            grayScaleStrategy.EventBus = EventBus;

            perlinNoiseStrategy.Initialize();
            whittakerStrategy.Initialize();
            grayScaleStrategy.Initialize();
        }

        /// <summary>
        /// Displays the editor of the view model.
        /// </summary>
        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"Terrain Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            heightCurve = EditorGUILayout.CurveField("Height Curve", heightCurve ?? new AnimationCurve());
            heightScale = EditorGUILayout.Slider("Height Scale", heightScale, 0, 100);

            DisplayNoiseStrategy();
            DisplayTextureStrategy();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of noise and the view model of the currently selected noise strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private void DisplayNoiseStrategy()
        {
            _noiseStrategyVisible = EditorGUILayout.Foldout(_noiseStrategyVisible, "Noise Generation");
            if (!_noiseStrategyVisible) return;
            
            EditorGUI.indentLevel++;
            noiseStrategy = (NoiseStrategy) EditorGUILayout.EnumPopup("Strategy", noiseStrategy);

            EditorGUI.indentLevel++;
            switch (noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    perlinNoiseStrategy.Display();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Displays the editor of texture and the view model of the currently selected texture strategy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private void DisplayTextureStrategy()
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
        /// Generates a terrain mesh and terrain texture with the selected strategies.
        /// </summary>
        /// <returns>A tuple of a mesh and a texture</returns>
        public override (Mesh, Texture2D) Generate() => (GenerateMesh(), GenerateTexture());

        /// <summary>
        /// Generates a mesh with the selected strategy.
        /// </summary>
        /// <returns>A mesh object</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected</exception>
        private Mesh GenerateMesh()
        {
            switch (noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    _noiseMapInjector.NoiseMap = perlinNoiseStrategy.Generate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return new Factory().CreateMeshGenerator(_noiseMapInjector, heightCurve, heightScale).Generate();
        }

        /// <summary>
        /// Generates a texture with the selected strategy.
        /// </summary>
        /// <returns>A texture object.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no strategy is selected.</exception>
        private Texture2D GenerateTexture()
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

        /// <summary>
        /// Injector class used in run-time for storing a generated height map.
        /// Is used by texture strategies.
        /// </summary>
        private new class Injector : IInjector<float[,]>
        {
            internal float[,] NoiseMap;

            public float[,] Get() => NoiseMap;
        }
    }
}