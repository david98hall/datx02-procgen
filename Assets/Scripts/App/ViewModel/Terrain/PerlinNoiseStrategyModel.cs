using System;
using Interfaces;
using Terrain.Noise;
using UnityEditor;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class PerlinNoiseStrategyModel : IViewAdapter<IGenerator<float[,]>>
    {
        private PerlinNoiseStrategy _strategy;
        
        [HideInInspector]
        public int width;
        
        [HideInInspector]
        public int depth;
        
        [HideInInspector] 
        public int seed;

        [HideInInspector]
        public float scale;

        [HideInInspector] 
        public int numOctaves;
        
        [HideInInspector] 
        public float persistence;
        
        [HideInInspector] 
        public float lacunarity;
        
        [HideInInspector] 
        public Vector2 noiseOffset;

        public IGenerator<float[,]> Model {
            get
            {
                _strategy.Width = width;
                _strategy.Depth = depth;
                _strategy.Seed = seed;
                _strategy.Scale = scale;
                _strategy.NumOctaves = numOctaves;
                _strategy.Lacunarity = lacunarity;
                _strategy.NoiseOffset = noiseOffset;
                return _strategy;
            }
            set => _strategy = value as PerlinNoiseStrategy;
        }

        public void Display()
        {
            width = EditorGUILayout.IntSlider("Width", width, 2, 250);
            depth = EditorGUILayout.IntSlider("Depth", depth, 2, 250);
            seed = EditorGUILayout.IntField("Seed", seed);
            scale = EditorGUILayout.Slider("Scale", scale, 1, 100);
            numOctaves = EditorGUILayout.IntSlider("Number of Octaves", numOctaves, 1, 10);
            persistence = EditorGUILayout.Slider("Persistence", persistence, 0, 1);
            lacunarity = EditorGUILayout.Slider("Lacunarity", lacunarity, 1, 10);
            noiseOffset = EditorGUILayout.Vector2Field("Offset", noiseOffset);
        }
    }
}