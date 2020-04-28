using System;
using System.Threading;
using App.ViewModels.Noise;
using App.ViewModels.Terrain.Textures;
using Interfaces;
using Services;
using Noise;
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
        private NoiseViewModel noiseViewModel;
        
        [SerializeField]
        private TextureViewModel textureViewModel;

        public override EventBus<AppEvent> EventBus
        {
            get => base.EventBus;
            set
            {
                base.EventBus = value;
                try
                {   
                    noiseViewModel.EventBus = value;
                    textureViewModel.EventBus = value;
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
                    noiseViewModel.CancelToken = value;
                    textureViewModel.CancelToken = value;
                }
                catch (NullReferenceException)
                {}
            }
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

            noiseViewModel.Display();
            textureViewModel.Display();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Generates a terrain mesh and terrain texture with the selected strategies.
        /// </summary>
        /// <returns>A tuple of a mesh and a texture</returns>
        public override (Mesh, Texture2D) Generate()
        {
            var heightMap = noiseViewModel.Generate();
            textureViewModel.Injector = new Injector<float[,]>(() => heightMap);

            // Generate the mesh
            EventBus.CreateEvent(AppEvent.GenerationStart, "Generating Terrain Mesh", this);
            var meshGenerator = new Factory().CreateMeshGenerator(
                new Injector<float[,]>(() => heightMap), heightCurve, heightScale);
            // Set the cancellation token so that the generation can be canceled
            meshGenerator.CancelToken = CancelToken;
            var mesh = meshGenerator.Generate();
            EventBus.CreateEvent(AppEvent.GenerationEnd, "Generated Terrain Mesh", this);
            
            // Generate mesh texture
            var texture = textureViewModel.Generate();

            return (mesh, texture);
        }

    }
}