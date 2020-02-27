using UnityEditor;
using UnityEngine;

namespace Terrain.Testing
{
    [CustomEditor(typeof(TextureDisplay))]
    public class TextureDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (TextureDisplay) target;
            script.strategy = (TextureDisplay.Strategy) EditorGUILayout.EnumPopup("Strategy", script.strategy);
            script.width = EditorGUILayout.IntField("Width", script.width);
            script.depth = EditorGUILayout.IntField("Depth", script.depth);
            script.heightScale = EditorGUILayout.FloatField("Height scale", script.heightScale);
            
            switch (script.strategy)
            {
                case TextureDisplay.Strategy.Whittaker: 
                    script.temperatureScale = 
                        EditorGUILayout.FloatField("Temperature scale", script.temperatureScale);
                    script.precipitationScale =
                        EditorGUILayout.FloatField("Precipitation scale", script.precipitationScale);
                    break;
            }
            
            if (GUILayout.Button("Refresh"))
            {
                script.Refresh();
            }
        }
    }
}