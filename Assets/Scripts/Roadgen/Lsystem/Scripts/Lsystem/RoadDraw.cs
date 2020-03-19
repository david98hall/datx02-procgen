using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cities.Roads;

/// <summary>
/// Draws roads by interpreting an L-system, and utilizing one or more 'turtles' that move along the terrain.
/// </summary>
 public class RoadDraw : MonoBehaviour
{
    public Lsystem system = new Lsystem('F');
    /// <summary>
    /// Represents the current state of the turtle(s)
    /// </summary>
    static class State
    {
        static Node current;
        static Vector3 direction;
    }

    /// <summary>
    /// A point in space that connects roads
    /// </summary>
    class Node
    {
        Vector3 pos;
        Node prev;
        HashSet<Node> next = new HashSet<Node>();

        public Node(Vector3 pos, Node prev)
        {
            this.pos = pos;
            this.prev = prev;
        }

        public void Add(Node n)
        {
            next.Add(n);
        }

    }
    
    
}
