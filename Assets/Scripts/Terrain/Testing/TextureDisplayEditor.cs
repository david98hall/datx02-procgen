using System;
using UnityEditor;
using UnityEngine;

namespace Terrain.Testing
{
    /// <summary>
    /// A custom editor class for  <see cref = "TextureDisplay"/>
    /// </summary>
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
                case TextureDisplay.Strategy.GrayScale:
                    break;
                case TextureDisplay.Strategy.Whittaker: 
                    script.whittakerMap = 
                        (TextureDisplay.WhittakerMap) EditorGUILayout.EnumPopup("Whittaker map", script.whittakerMap);
                    script.precipitationScale =
                        EditorGUILayout.FloatField("Precipitation scale", script.precipitationScale);
                    script.temperatureScale = 
                        EditorGUILayout.FloatField("Temperature scale", script.temperatureScale);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (GUILayout.Button("Refresh"))
            {
                script.Refresh();
            }
        }
    }
}