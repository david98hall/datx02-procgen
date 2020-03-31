using System.Collections.Generic;
using System.Text;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// A way of generating roads using a non-deterministic L-system.
    /// </summary>
    public class LSystem
    {
        public class State
        {
            public Vector3 pos;
            public double angle;

            public State(Vector3 pos, double angle)
            {
                this.pos = pos;
                this.angle = angle;
            }
        }


        private readonly IInjector<float[,]> _noiseMapInjector;
        // F -> The Road goes forward
        // S -> The Road splits
        // B -> The Road branches off in a particular shape
        // + -> Turn left by some amount
        // - -> Turn right by some amount
        // [ -> Start working on the next state in the queue, saving the current one
        // ] -> Mark the current state as finished, and proceed to the next one in the queue
        public IDictionary<char, string> ruleset = new Dictionary<char, string>();
        StringBuilder tree;
        public char axiom;
        public RoadNetwork network = new RoadNetwork();
        State state;
        Queue<State> states = new Queue<State>();
        private float toRad = Mathf.Deg2Rad;
        private float pi = Mathf.PI;

        public LSystem(IInjector<float[,]> noiseMapInjector, char c)
        {
            _noiseMapInjector = noiseMapInjector;
            axiom = c;
            state = new State(Vector3.zero, 0);
            ruleset.Add('F',"F+FB]");
            ruleset.Add('S', "B-FB[");
            ruleset.Add('B',"FS[F+]");
            tree = new StringBuilder(c.ToString());
        }

        /*public Lsystem(char c, State state){
        axiom = c;
        this.state = state;
        ruleset.Add('F',"F+B-S-");
        ruleset.Add('S', "-F+B");
        ruleset.Add('B',"FF+");
        tree = new StringBuilder(c.ToString());
    }*/

        public override string ToString(){
            return tree.ToString();
        }
        /// <summary>
        /// Rewrites the String within the L-system according to the ruleset, and generates roads.
        /// </summary>
        public void Rewrite()
        {
            var noiseMap = _noiseMapInjector.Get();
            System.Random rdm = new System.Random();
            StringBuilder newTree = new StringBuilder();
            foreach (char c in tree.ToString())
            {
                try
                {
                    string r = ruleset[c];
                    newTree.Append(r);
                    LinkedList<Vector3> road = new LinkedList<Vector3>();
                    Vector3 direction = new Vector3(Mathf.Cos((float) state.angle), 0, Mathf.Sin((float) state.angle));
                    road.AddLast(state.pos);
                    float length = Random.Range(1,3);
                    switch(c)
                    {
                        case 'S':   //Create split-off road
                        case 'B':{  //Create a branch with an F-like shape
                            LinkedList<Vector3> splitRoad = new LinkedList<Vector3>();
                            splitRoad.AddLast(state.pos);
                            double splitAngle;
                            double condition = rdm.NextDouble();
                            if(condition >= 0.5)
                            {
                                splitAngle = state.angle + Random.Range(45*toRad,75*toRad);
                            }else
                            {
                                splitAngle = state.angle - Random.Range(45*toRad,75*toRad);
                            }
                            Vector3 splitDir = new Vector3(Mathf.Cos((float) splitAngle), 0, Mathf.Sin((float) splitAngle));
                            State splitState;
                            switch(c)
                            {
                                case 'S':{
                                    splitState = new State(state.pos + length * splitDir, splitAngle);
                                    states.Enqueue(splitState);
                                    splitRoad.AddLast(splitState.pos);
                                    break;
                                }
                                case 'B':{
                                    Vector3 rotate90;
                                    if(condition >= 0.5)
                                    {
                                        rotate90 = new Vector3(Mathf.Cos((float) splitAngle + 90*toRad), 0, Mathf.Sin((float) splitAngle + 90*toRad));
                                    }else
                                    {
                                        rotate90 = new Vector3(Mathf.Cos((float) splitAngle - 90*toRad), 0, Mathf.Sin((float) splitAngle - 90*toRad));
                                    }
                                    splitRoad.AddLast(state.pos + 2 * splitDir);
                                    splitState = new State(state.pos + 2*splitDir + rotate90, splitAngle);
                                    splitRoad.AddLast(splitState.pos);
                                    states.Enqueue(splitState);
                                    LinkedList<Vector3> row2 = new LinkedList<Vector3>();
                                    row2.AddLast(state.pos + 1 * splitDir);
                                    row2.AddLast(state.pos + 1 * splitDir + rotate90);
                                    network.AddRoad(row2);
                                    break;
                                }
                            }
                            network.AddRoad(splitRoad);
                            //Debug.Log("Intersections: "+ network.Intersections.ToString());
                            break;
                        }
                    }
                    road.AddLast(state.pos + length * direction);
                    network.AddRoad(road);
                    state.pos += length * direction;
                }
                catch(KeyNotFoundException)
                {
                    switch(c)
                    {
                        case '+':
                            state.angle += Random.Range(30*toRad,55*toRad);
                            break;
                        case '-':
                            state.angle -= Random.Range(30*toRad,55*toRad);
                            break;
                        case '[':
                            states.Enqueue(state);
                            state = states.Dequeue();
                            break;
                        case ']':
                            state = states.Dequeue();
                            break;

                    }
                    state.angle %= 2*pi;
                }
            }
            tree = newTree;
        }
    }
}
