using System;
using App.ViewModels.Terrain.Noise;
using App.ViewModels.Terrain.Textures;
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
                {
                    // Ignore
                }
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
            //textureViewModel.Injector = noiseViewModel;
            textureViewModel.Injector = new Injector<float[,]>(() => heightMap);
            
            var mesh = new Factory().CreateMeshGenerator(
                new Injector<float[,]>(() => heightMap), heightCurve, heightScale).Generate();

            Debug.Log("Mesh finished!");
            
            return (mesh, textureViewModel.Generate());
        }

    }
}