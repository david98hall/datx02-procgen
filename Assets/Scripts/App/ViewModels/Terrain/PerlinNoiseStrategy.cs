using System;
using UnityEditor;
using UnityEngine;
using Factory = Terrain.Noise.Factory;

namespace App.ViewModels.Terrain
{
    [Serializable]
    public class PerlinNoiseStrategy : ViewModelStrategy<object, float[,]>
    {
        #region UI Fields
        
        [SerializeField]
        private int width;
        
        [SerializeField]
        private int depth;
        
        [SerializeField]
        private int seed;
        
        [SerializeField]
        private float scale; 
        
        [SerializeField]
        private int numOctaves;
        
        [SerializeField]
        private float persistence;
        
        [SerializeField]
        private float lacunarity;
        
        [SerializeField]
        private Vector2 noiseOffset;

        #endregion

        public override void Display()
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

        public override float[,] Generate() => 
            Factory.CreatePerlinNoiseStrategy(
                    width, depth, 
                    seed, scale, 
                    numOctaves, persistence, 
                    lacunarity, noiseOffset)
                .Generate();
    }
}