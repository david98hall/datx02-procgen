using System;
using Terrain;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class TerrainGeneratorModel : EditorStrategyView<object, (Mesh, Texture2D)>
    {
        private TerrainGenerator _generator;
        private bool _visible;
        private bool _noiseStrategyVisible;
        private bool _textureStrategyVisible;

        [SerializeField]
        private PerlinNoiseStrategyModel _perlinNoiseStrategy;

        [SerializeField] 
        private WhittakerStrategyModel _whittakerStrategy;

        [SerializeField] 
        private GrayScaleModel _grayScaleStrategy;

        public enum NoiseStrategy
        {
            PerlinNoise
        }

        public enum TextureStrategy
        {
            GrayScale, Whittaker
        }

        [SerializeField] 
        private AnimationCurve _heightCurve;
        
        [SerializeField]
        private float _heightScale;

        [SerializeField]
        private NoiseStrategy _noiseStrategy;

        [SerializeField]
        private TextureStrategy _textureStrategy;

        public override void Initialize()
        {
            _generator = new TerrainGenerator();
            _perlinNoiseStrategy.Injector = _generator;
            _whittakerStrategy.Injector = _generator;
            _grayScaleStrategy.Injector = _generator;
            
            _perlinNoiseStrategy.Initialize();
            _whittakerStrategy.Initialize();
            _grayScaleStrategy.Initialize();
        }

        public override void Display()
        {
            _visible = EditorGUILayout.Foldout(_visible,"Terrain Generation");
            if (!_visible) return;

            EditorGUI.indentLevel++;
            _heightCurve = EditorGUILayout.CurveField("Height Curve", _heightCurve ?? new AnimationCurve());
            _heightScale = EditorGUILayout.Slider("Height Scale", _heightScale, 0, 100);

            _noiseStrategyVisible = EditorGUILayout.Foldout(_noiseStrategyVisible, "Noise Generation");
            if (_noiseStrategyVisible)
            {
                EditorGUI.indentLevel++;
                _noiseStrategy = (NoiseStrategy) EditorGUILayout.EnumPopup("Strategy", _noiseStrategy);

                EditorGUI.indentLevel++;
                switch (_noiseStrategy)
                {
                    case NoiseStrategy.PerlinNoise:
                        _perlinNoiseStrategy.Display();
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
                _textureStrategy = (TextureStrategy) EditorGUILayout.EnumPopup("Strategy", _textureStrategy);

                EditorGUI.indentLevel++;
                switch (_textureStrategy)
                {
                    case TextureStrategy.GrayScale:
                        _grayScaleStrategy.Display();
                        break;
                    case TextureStrategy.Whittaker:
                        _whittakerStrategy.Display();
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
            _generator.HeightCurve = _heightCurve;
            _generator.HeightScale = _heightScale;
            
            // Set the noise strategy
            switch (_noiseStrategy)
            {
                case NoiseStrategy.PerlinNoise:
                    _generator.NoiseStrategy = _perlinNoiseStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            // Set texture strategy
            switch (_textureStrategy)
            {
                case TextureStrategy.GrayScale:
                    _generator.TextureStrategy = _grayScaleStrategy;
                    break;
                case TextureStrategy.Whittaker:
                    _generator.TextureStrategy = _whittakerStrategy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return _generator.Generate();
        }
    }
}