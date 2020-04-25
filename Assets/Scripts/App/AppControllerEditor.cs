using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace App
{
    /// <summary>
    /// Custom editor for the application controller.
    /// Is required for displaying the custom views instead of the default editor.
    /// </summary>
    [CustomEditor(typeof(AppController))]
    public class AppControllerEditor : Editor
    {
        /// <summary>
        /// Overrides the default editor inspector and displays the custom editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (!(target is AppController controller)) return;
            controller.Display();

            if (GUILayout.Button("Update"))
            {
                controller.GenerateAsync();
            }

            if (!GUI.changed) return;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}