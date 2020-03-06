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
            
            if (DrawDefaultInspector() && cityObject.autoUpdate)
            {
                cityObject.GenerateCity();
            }

            if (GUILayout.Button("Generate City"))
            {
                cityObject.GenerateCity();
            }
            
        }
    }
}