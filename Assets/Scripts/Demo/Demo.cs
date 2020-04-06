using System;
using UnityEngine;

namespace Demo
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Demo : MonoBehaviour
    {
        private Settings settings;

        public TerrainGenerator terrainGenerator;
        private void Update()
        {
            var (mesh, texture) = terrainGenerator.Generate();
            settings.meshFilter.sharedMesh = mesh;
            settings.meshRenderer.sharedMaterial.mainTexture = texture;
        }
        
        [Serializable]
        public struct Settings
        {
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
        }
    }
}
