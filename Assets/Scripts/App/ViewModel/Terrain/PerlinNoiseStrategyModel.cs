using System;
using Interfaces;
using UnityEditor;
using UnityEngine;
using Factory = Terrain.Noise.Factory;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class PerlinNoiseStrategyModel : EditorStrategyView<object, float[,]>
    {
        #region UI Fields
        
        [SerializeField]
        private int _width;
        
        [SerializeField]
        private int _depth;
        
        [SerializeField]
        private int _seed;
        
        [SerializeField]
        private float _scale; 
        
        [SerializeField]
        private int _numOctaves;
        
        [SerializeField]
        private float _persistence;
        
        [SerializeField]
        private float _lacunarity;
        
        [SerializeField]
        private Vector2 _noiseOffset;

        #endregion

        public override void Display()
        {
            _width = EditorGUILayout.IntSlider("Width", _width, 2, 250);
            _depth = EditorGUILayout.IntSlider("Depth", _depth, 2, 250);
            _seed = EditorGUILayout.IntField("Seed", _seed);
            _scale = EditorGUILayout.Slider("Scale", _scale, 1, 100);
            _numOctaves = EditorGUILayout.IntSlider("Number of Octaves", _numOctaves, 1, 10);
            _persistence = EditorGUILayout.Slider("Persistence", _persistence, 0, 1);
            _lacunarity = EditorGUILayout.Slider("Lacunarity", _lacunarity, 1, 10);
            _noiseOffset = EditorGUILayout.Vector2Field("Offset", _noiseOffset);
        }

        public override float[,] Generate() => 
            Factory.CreatePerlinNoiseStrategy(
                    _width, _depth, 
                    _seed, _scale, 
                    _numOctaves, _persistence, 
                    _lacunarity, _noiseOffset)
                .Generate();
    }
}