using UnityEditor;
using UnityEngine;

namespace Demo
{
    [CustomEditor(typeof(Demo2))]
    [CanEditMultipleObjects]
    public class DemoEditor2 : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!(target is Demo2 demo)) return;

            
            //demo.noiseStrategyIndex =
            //    EditorGUILayout.Popup("Noise Strategy", demo.noiseStrategyIndex, demo.NoiseStrategies);
            demo.textureStrategyIndex =
                EditorGUILayout.Popup("Noise Strategy", demo.textureStrategyIndex, demo.TextureStrategies);
           
        }
    }
}