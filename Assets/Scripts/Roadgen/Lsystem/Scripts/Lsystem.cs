using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


/// <summary>
/// A string rewriting system. Used in RoadDraw to generate roads.
/// </summary>
public class Lsystem
{
        // F -> Draw a line
    // G -> Move forward
    // + -> Turn left
    // - -> Turn Right
    // [ -> Save state
    // ] -> Restore state

    public IDictionary<char, string> ruleset = new Dictionary<char, string>();
    StringBuilder tree;
    public char axiom;

    public Lsystem(char c){
        axiom = c;
        ruleset.Add('F',"+FG-");
        ruleset.Add('G',"GF+F");
        tree = new StringBuilder(c.ToString());
    }
    public override string ToString(){
        return tree.ToString();
    }

    public void Rewrite()
    {
        StringBuilder newTree = new StringBuilder();
        foreach (char c in tree.ToString())
        {
            try
            {
                newTree.Append(ruleset[c]);
            }
            catch(KeyNotFoundException)
            {

            }
        }
        tree = newTree;
    }
}
