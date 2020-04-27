using System;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Utils.Concurrency;

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
        private string _progressInfo;
        private float _progress;
        
        private void Reset()
        {
            _progressInfo = "";
            
            // Reset cancellation token source
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnGenerationStart(string info)
        {
            _progressInfo = info;
            Repaint();
        }

        private void OnGenerationEnd(string info, float progress)
        {
            _progress = progress;
            Repaint();
        }
        
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
                controller.GenerateAsync(
                    Reset, OnGenerationStart, OnGenerationEnd, cancellationToken);
            }
            else if (_cancellationTokenSource != null)
            {
                EditorGUILayout.LabelField($"Progress: {_progress * 100}% ({_progressInfo})");

                if (GUILayout.Button("Cancel Generation"))
                {
                    Reset();
                    controller.Reset();   
                }
            }

            if (!GUI.changed) return;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
        
    }
}