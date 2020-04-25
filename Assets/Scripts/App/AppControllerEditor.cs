using System.Threading;
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

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Overrides the default editor inspector and displays the custom editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (!(target is AppController controller)) return;
            controller.Display();

            if (_cancellationTokenSource == null && GUILayout.Button("Generate"))
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;
                controller.GenerateAsync(cancellationToken);
            } 
            else if (_cancellationTokenSource != null && GUILayout.Button("Cancel Generation"))
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                controller.Reset();
            }

            if (!GUI.changed) return;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}