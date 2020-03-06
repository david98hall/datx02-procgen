using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A model for generating roads through the use of one or more 'turtles'
/// that move along the terrain.
/// </summary>
public class Lsystem : MonoBehaviour
{
    // F -> Draw a line
    // G -> Move forward
    // + -> Turn left
    // - -> Turn Right
    // [ -> Save state
    // ] -> Restore state

    /// <summary>
    /// The rules dictate the rewriting process.
    /// </summary>
    class Rule{
        char input;
        string output;
        
        Rule(char c, string s){
            input = c;
            output = s;
        }
    }
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

    public char axiom = 'F';
    public Set<String> ruleset = new HashSet<Rule>();
    StringBuffer tree = new StringBuffer();
}
