using System;
using Interfaces;
using Terrain.Textures;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class WhittakerStrategyModel : EditorStrategyView<float[,], Texture2D>
    {
        private Factory _textureStrategyFactory;
        
        [SerializeField] 
        private float _precipitationScale;

        [SerializeField]
        private float temperatureScale;

        public override void Initialize()
        {
            _textureStrategyFactory = new Factory(Injector);
        }

        public override void Display()
        {
            _precipitationScale = EditorGUILayout.Slider("Precipitation scale", _precipitationScale, 1, 100);
            temperatureScale = EditorGUILayout.Slider("Temperature scale", temperatureScale, 1, 100);
        }

        public override Texture2D Generate()
        {
            return _textureStrategyFactory?.CreateWhittakerStrategy().Generate();
        }
    }
}