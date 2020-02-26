using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Terrain;
using Interfaces;

 
public class TerrainWindow : EditorWindow
{
    public enum NoiseMapStrategy
        {
            PerlinNoise
        }
      public NoiseMapStrategy noiseMapStrategy;
        
        public int width;
        public int height;
        public float noiseScale;
        
        public float heightScale;
        public AnimationCurve heightCurve;

        private readonly TerrainGenerator terrainGenerator;
        
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public bool autoUpdate;
         public TerrainWindow()
        {
            noiseMapStrategy = NoiseMapStrategy.PerlinNoise;
            terrainGenerator = new TerrainGenerator(null);
        }
        public void GenerateTerrainMesh()
        {
            terrainGenerator.HeightScale = heightScale;
            terrainGenerator.HeightCurve = heightCurve;
            terrainGenerator.Strategy = GetNoiseStrategy();
            var (mesh, texture) = terrainGenerator.Generate();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial.mainTexture = texture;
        }

        private IGenerator<float[,]> GetNoiseStrategy()
        {
            return new PerlinNoiseStrategy(width, height, noiseScale);
        }
          
        private void OnValidate()
        {
            if (width < 1) 
                width = 1;
            if (height < 1) 
                height = 1;
        }
     public static void ShowWindow ()
    {
         GetWindow<TerrainWindow>("Terrain Editor");
    }

    void OnGUI()
    {
            //if (Editor.DrawDefaultInspector() && autoUpdate)
            //{
                //GenerateTerrainMesh();
            //}
            

            heightCurve = EditorGUILayout.CurveField("Height Curve",heightCurve) as AnimationCurve;

            meshFilter = EditorGUILayout.ObjectField("Mesh Filter",meshFilter,typeof(MeshFilter),true) as MeshFilter;

            meshRenderer = EditorGUILayout.ObjectField("Mesh Renderer",meshRenderer,typeof(MeshRenderer),true) as MeshRenderer;

            autoUpdate= EditorGUILayout.Toggle("Auto Update",autoUpdate);

            if (GUILayout.Button("Generate Terrain"))
            {
                GenerateTerrainMesh();
            }
    }
   
}
