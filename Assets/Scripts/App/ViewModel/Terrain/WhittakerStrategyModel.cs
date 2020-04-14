using System;
using Interfaces;
using Terrain.Textures;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class WhittakerStrategyModel : IViewAdapter<IGenerator<Texture2D>>
    {
        private WhittakerStrategy _strategy;

        [HideInInspector] 
        public float precipitationScale;

        [HideInInspector]
        public float temperatureScale;

        public IGenerator<Texture2D> Model 
        {
            get
            {
                _strategy.PrecipitationScale = precipitationScale;
                _strategy.TemperatureScale = temperatureScale;
                return _strategy;
            }
            set => _strategy = value as WhittakerStrategy; 
        }
        
        public void Display()
        {
            precipitationScale = EditorGUILayout.Slider("Precipitation scale", precipitationScale, 1, 100);
            temperatureScale = EditorGUILayout.Slider("Temperature scale", temperatureScale, 1, 100);
        }
    }
}