using System;
using App.ViewModel.Terrain;
using Terrain;
using UnityEngine;

namespace App
{
    [Serializable]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class App : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        [SerializeField]
        [HideInInspector]
        private TerrainGeneratorModel terrainGeneratorModel;

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            terrainGeneratorModel.Model = new TerrainGenerator();
        }

        public void Update()
        {
            var (mesh, texture) = terrainGeneratorModel.Model.Generate();
            _meshFilter.sharedMesh = mesh;
            _meshRenderer.sharedMaterial.mainTexture = texture;
        }

        public void Display()
        {
            terrainGeneratorModel.Display();
        }
    }
}