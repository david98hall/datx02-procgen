using System;
using App.ViewModels.Noise;
using App.ViewModels.Noise.Textures;
using Interfaces;
using Services;
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

        [SerializeField]
        private NoiseViewModel _noiseViewModel;
        
        [SerializeField]
        private TextureViewModel _textureViewModel;

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    _noiseViewModel.EventBus = value;
                    _textureViewModel.EventBus = value;
                }
                catch (NullReferenceException)
                {
                    // Ignore
                }
            }
        }
        
        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize()
        {
            _textureViewModel.Injector = _noiseViewModel;

            _noiseViewModel.Initialize();
            _textureViewModel.Initialize();
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

            _noiseViewModel.Display();
            _textureViewModel.Display();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Generates a terrain mesh and terrain texture with the selected strategies.
        /// </summary>
        /// <returns>A tuple of a mesh and a texture</returns>
        public override (Mesh, Texture2D) Generate()
        {
            var heightMap = _noiseViewModel.Generate();
            _textureViewModel.Injector = new Injector<float[,]>(() => heightMap);
            
            var mesh = new Factory().CreateMeshGenerator(
                new Injector<float[,]>(() => heightMap), heightCurve, heightScale).Generate();

            return (mesh, _textureViewModel.Generate());
        }

    }
}