using System;
using Interfaces;
using Terrain;
using UnityEditor;
using UnityEngine;
using Factory = Terrain.Noise.Factory;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class TerrainGeneratorModel : IViewAdapter<IGenerator<(Mesh, Texture2D)>>
    {
        private TerrainGenerator _generator;
        private bool _visible;
        private bool _noiseStrategyVisible;
        private bool _textureStrategyVisible;

        [SerializeField]
        private PerlinNoiseStrategyModel perlinNoiseStrategy;

        [SerializeField] 
        private WhittakerStrategyModel whittakerStrategy;

        [SerializeField] 
        private GrayScaleModel grayScaleStrategy;

        public enum NoiseStrategy
        {
            PerlinNoise
        }

        public enum TextureStrategy
        {
            GrayScale, Whittaker
        }

        [HideInInspector] 
        public AnimationCurve heightCurve;
        
        [HideInInspector]
        public float heightScale;

        [HideInInspector]
        public NoiseStrategy noiseStrategy;

        [HideInInspector]
        public TextureStrategy textureStrategy;

        public IGenerator<(Mesh, Texture2D)> Model {
            get
            {
                _generator.HeightCurve = heightCurve;
                _generator.HeightScale = heightScale;

                switch (noiseStrategy)
                {
                    case NoiseStrategy.PerlinNoise:
                        _generator.NoiseStrategy = perlinNoiseStrategy.Model;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                switch (textureStrategy)
                {
                    case TextureStrategy.GrayScale:
                        _generator.TextureStrategy = grayScaleStrategy.Model;
                        break;
                    case TextureStrategy.Whittaker:
                        _generator.TextureStrategy = whittakerStrategy.Model;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return _generator;
            }
            set
            {
                _generator = value as TerrainGenerator;
                perlinNoiseStrategy.Model = new Factory().CreatePerlinNoiseStrategy();
                if (_generator is null) return;
                var factory = _generator.TextureStrategyFactory;
                grayScaleStrategy.Model = factory.CreateGrayScaleStrategy();
                whittakerStrategy.Model = factory.CreateWhittakerStrategy();
            }
        }

        public void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"Terrain Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            heightCurve = EditorGUILayout.CurveField("Height Curve", heightCurve ?? new AnimationCurve());
            heightScale = EditorGUILayout.Slider("Height Scale", heightScale, 0, 100);

            _noiseStrategyVisible = EditorGUILayout.Foldout(_noiseStrategyVisible, "Noise Generation");
            if (_noiseStrategyVisible)
            {
                EditorGUI.indentLevel++;
                noiseStrategy = (NoiseStrategy) EditorGUILayout.EnumPopup("Strategy", noiseStrategy);

                switch (noiseStrategy)
                {
                    case NoiseStrategy.PerlinNoise:
                        perlinNoiseStrategy.Display();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                EditorGUI.indentLevel--;
            }

            _textureStrategyVisible = EditorGUILayout.Foldout(_textureStrategyVisible, "Texture Generation");
            if (_textureStrategyVisible)
            {
                EditorGUI.indentLevel++;
                textureStrategy = (TextureStrategy) EditorGUILayout.EnumPopup("Strategy", textureStrategy);

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
            }
            
            EditorGUI.indentLevel--;
        }
    }
}