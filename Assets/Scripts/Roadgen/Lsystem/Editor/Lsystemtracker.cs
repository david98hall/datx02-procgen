using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadDraw))]
public class Lsystemtracker : Editor
{
    public void Draw()
    {
        Handles.RectangleHandleCap(0,Vector3.zero,Quaternion.LookRotation(Vector3.forward,Vector3.up),2,EventType.Repaint);
        HandleUtility.Repaint();
    }
    void OnSceneGUI()
    {
        if(true)
        {
            HandleUtility.Repaint();
        }
    }
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
        if(GUILayout.Button("Draw a Line"))
        {
            Draw();
        }
    }
}
