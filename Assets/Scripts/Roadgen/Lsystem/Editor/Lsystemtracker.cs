using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadDraw))]
public class Lsystemtracker : Editor
{
    public override void OnInspectorGUI()
    {
        RoadDraw roadDraw = (RoadDraw) target;
        EditorGUILayout.TextField("Tree", roadDraw.system.ToString());
        if(GUILayout.Button("Rewrite L-System"))
        {
            roadDraw.system.Rewrite();
        }
        if(GUILayout.Button("Reset L-system"))
        {
            roadDraw.system = new Lsystem(roadDraw.system.axiom);
        }
    }
}
