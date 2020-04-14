using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace App
{
    [CustomEditor(typeof(App))]
    public class AppEditor : Editor
    {
        private bool _terrainGenerator;
        private bool _noiseGenerator;
        private bool _textureGenerator;
        
        public override void OnInspectorGUI()
        {
            if (!(target is App app)) return;
            app.DisplayEditor();

            if (GUILayout.Button("Update"))
            {
                app.Generate();
            }

            if (!GUI.changed) return;
            EditorUtility.SetDirty(app);
            EditorSceneManager.MarkSceneDirty(app.gameObject.scene);
        }
    }
}