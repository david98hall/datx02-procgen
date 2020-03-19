using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Cities.Roads;


/// <summary>
/// A string rewriting system. Used in RoadDraw to generate roads.
/// </summary>
public class Lsystem
{
    // F -> Road goes forward
    // S -> The road splits
    // + -> Turn left by some amount
    // - -> Turn right by some amount
    // [ -> Save state
    // ] -> Restore state

    public IDictionary<char, string> ruleset = new Dictionary<char, string>();
    StringBuilder tree;
    public char axiom;
    RoadNetwork network = new RoadNetwork();
    Vector3 pos = Vector3.zero;

    public Lsystem(char c){
        
        axiom = c;
        ruleset.Add('F',"-GGFG");
        ruleset.Add('G',"FFGF+");
        tree = new StringBuilder(c.ToString());
    }
    public override string ToString(){
        return tree.ToString();
    }

    public void Rewrite()
    {
        int angle;
        Vector3 direction;
        int length;
        StringBuilder newTree = new StringBuilder();
        foreach (char c in tree.ToString())
        {
            try
            {
                newTree.Append(ruleset[c]);
                //Draw a Road, or move forward.
            }
            catch(KeyNotFoundException)
            {
                //Rotate an angle, depending on + or -.
            }
        }
        tree = newTree;
    }
}
