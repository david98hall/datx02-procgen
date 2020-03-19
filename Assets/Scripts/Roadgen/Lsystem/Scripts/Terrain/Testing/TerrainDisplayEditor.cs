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

            if (DrawDefaultInspector() && terrainDisplay.autoUpdate)
            {
                terrainDisplay.GenerateTerrainMesh();
            }

            if (GUILayout.Button("Generate Terrain"))
            {
                terrainDisplay.GenerateTerrainMesh();
            }
        }
    }
}