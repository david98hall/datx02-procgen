using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorView : EditorWindow 
{

    [MenuItem("Window/Edit Enviroment") ]
    public static void ShowWindow ()
    {
          GetWindow<EditorView>("Edit Enviroment");
    }

    void OnGUI ()
    {
        
        GUILayout.BeginHorizontal("box");
        //window code
        if(GUILayout.Button("Terrain", GUILayout.Width(100),GUILayout.Height(100))){
            //open window for edit terrain
            TerrainWindow.ShowWindow();
        }
        
        
        if (GUILayout.Button("City", GUILayout.Width(100),GUILayout.Height(100)))
        {
            //open window for cretaing cities
            CityWindow.ShowWindow();
        }


        if(GUILayout.Button("Roads", GUILayout.Width(100),GUILayout.Height(100)))
        {
            //open window for creating roads 
            RoadWindow.ShowWindow();
        }

        GUILayout.EndHorizontal();
    }
 
}
