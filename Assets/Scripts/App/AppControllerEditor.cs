using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace App
{
    [CustomEditor(typeof(AppController))]
    public class AppControllerEditor : Editor
    {
        private bool _terrainGenerator;
        private bool _noiseGenerator;
        private bool _textureGenerator;
        
        public override void OnInspectorGUI()
        {
            if (!(target is AppController controller)) return;
            controller.DisplayEditor();

            if (GUILayout.Button("Update")) controller.Generate();

            if (!GUI.changed) return;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}