using UnityEditor;
using UnityEngine;

namespace Terrain.Testing
{
    [CustomEditor(typeof(TerrainDisplay))]
    public class TerrainDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var terrainDisplay = (TerrainDisplay) target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Terrain"))
            {
                terrainDisplay.GenerateTerrainMesh();
            }
        }
    }
}