using System;
using Terrain;
using UnityEditor;
using UnityEngine;

namespace App.ViewModels.Terrain
{
    [Serializable]
    public class TerrainViewModel : ViewModelStrategy<object, (Mesh, Texture2D)>
    {
        private TerrainGenerator _generator;
        private bool _visible;
        private bool _noiseStrategyVisible;
        private bool _textureStrategyVisible;

        public enum NoiseStrategy
        {
            PerlinNoise
        }

        public enum TextureStrategy
        {
            GrayScale, Whittaker
        }

        #region UI Fields

        [SerializeField]
        private PerlinNoiseStrategy perlinNoiseStrategy;

        [SerializeField] 
        private WhittakerStrategy whittakerStrategy;

        [SerializeField] 
        private GrayScaleStrategy grayScaleStrategy;

        [SerializeField] 
        private AnimationCurve heightCurve;
        
        [SerializeField]
        private float heightScale;

        [SerializeField]
        private NoiseStrategy noiseStrategy;

        [SerializeField]
        private TextureStrategy textureStrategy;

        #endregion

        public override void Initialize()
        {
            _generator = new TerrainGenerator();
            perlinNoiseStrategy.Injector = _generator;
            whittakerStrategy.Injector = _generator;
            grayScaleStrategy.Injector = _generator;
            
            perlinNoiseStrategy.Initialize();
            whittakerStrategy.Initialize();
            grayScaleStrategy.Initialize();
        }

        public override void Display()
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

            _textureStrategyVisible = EditorGUILayout.Foldout(_textureStrategyVisible, "Texture Generation");
            if (_textureStrategyVisible)
            {
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
            
            EditorGUI.indentLevel--;
        }

        public override (Mesh, Texture2D) Generate()
        {
            _generator.HeightCurve = heightCurve;
            _generator.HeightScale = heightScale;
            
            // Set the noise strategy
            switch (noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    _generator.NoiseStrategy = perlinNoiseStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            // Set texture strategy
            switch (textureStrategy)
            {
                case TextureStrategy.GrayScale:
                    _generator.TextureStrategy = grayScaleStrategy;
                    break;
                case TextureStrategy.Whittaker:
                    _generator.TextureStrategy = whittakerStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return _generator.Generate();
        }
    }
}