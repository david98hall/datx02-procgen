using UnityEditor;
using UnityEngine;

namespace Cities.Testing
{
    [CustomEditor(typeof(CityDisplay))]
    public class CityGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var cityObject = (CityDisplay) target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate City"))
            {
                cityObject.GenerateCity();
            }
            
            if (GUILayout.Button("Clear"))
            {
                cityObject.Clear();;
            }
        }
    }
}