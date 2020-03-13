using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Draws roads by interpreting an L-system, and utilizing one or more 'turtles' that move along the terrain.
/// </summary>
public class RoadDraw : MonoBehaviour
{
    public Lsystem system = new Lsystem('F');
    /// <summary>
    /// Represents the current state of the turtle(s)
    /// </summary>
    class State
    {
        Vector3 pos;
        Vector3 direction;
    }

    /// <summary>
    /// A point in space that connects roads
    /// </summary>
    class Node
    {
        Vector3 pos;
        Node prev;
        HashSet<Node> next = new HashSet<Node>();
    }

    /*public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Rewrite L-system"))
        {
            system.Rewrite();
            Debug.Log(system.ToString());
        }
    }*/
}
