using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// A way of generating roads using a non-deterministic L-system.
    /// </summary>
    public class Lsystem
    {
        public class State{
            public Vector3 pos;
            public double angle;

            public State(Vector3 pos, double angle)
            {
                this.pos = pos;
                this.angle = angle;
            }
            public State(){
                pos = Vector3.zero;
                angle = 0;
            }
        }
        
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

        public Lsystem(char c, Vector2 origin, IInjector<float[,]> noiseMapInjector)
        {
            _noiseMapInjector = noiseMapInjector;
            axiom = c;
            state = new State(new Vector3(origin.x, 0, origin.y), 0);
            ruleset.Add('F',"F+FB-]");
            ruleset.Add('S', "B-FB[");
            ruleset.Add('B',"FS[F+]");
            tree = new StringBuilder(c.ToString());
        }
        
        #region Noise map

        private readonly IInjector<float[,]> _noiseMapInjector;
        
        /// <summary>
        /// Applies the injected noise map to the road network and returns the result.
        /// </summary>
        internal RoadNetwork NoiseMappedNetwork => ApplyNoiseMap();

        // Applies the injected noise map on the road network.
        private RoadNetwork ApplyNoiseMap()
        {
            var newNetwork = new RoadNetwork();
            var noiseMap = _noiseMapInjector.Get();
            foreach (var (roadStart, roadEnd) in network.GetRoadParts())
            {
                var roadStartY = noiseMap[(int) roadStart.x, (int) roadStart.z];
                var roadEndY = noiseMap[(int) roadEnd.x, (int) roadEnd.z];
                const float yOffset = 0;
                newNetwork.AddRoad(
                    new Vector3(roadStart.x, roadStartY + yOffset, roadStart.z),
                    new Vector3(roadEnd.x, roadEndY + yOffset, roadEnd.z)
                );
            }
            
            return newNetwork;
        }

        #endregion

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
                    float length = UnityEngine.Random.Range(1,3);
                    int intersects = 0;
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
                                splitAngle = state.angle + UnityEngine.Random.Range(45*toRad,75*toRad);
                            }else
                            {
                                splitAngle = state.angle - UnityEngine.Random.Range(45*toRad,75*toRad);
                            }
                            Vector3 splitDir = new Vector3(Mathf.Cos((float) splitAngle), 0, Mathf.Sin((float) splitAngle));
                            State splitState = new State();
                            switch(c)
                            {
                                case 'S':{
                                    splitState = new State(state.pos + length * splitDir, splitAngle);
                                    intersects = noIntersects(splitState.pos, 2.0f);
                                    if(intersects <= 1){
                                        states.Enqueue(splitState);
                                        splitRoad.AddLast(splitState.pos);
                                    }
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
                                    splitState = new State(state.pos + 2*splitDir + rotate90, splitAngle);
                                    intersects = noIntersects(splitState.pos, 2.0f);
                                    if(intersects <= 1){
                                        splitRoad.AddLast(state.pos + 2 * splitDir);
                                        splitRoad.AddLast(splitState.pos);
                                        states.Enqueue(splitState);
                                        LinkedList<Vector3> row2 = new LinkedList<Vector3>();
                                        row2.AddLast(state.pos + 1 * splitDir);
                                        row2.AddLast(state.pos + 1 * splitDir + rotate90);
                                        network.AddRoad(row2);
                                    }
                                    break;
                                }
                            }
                            if(intersects < 4)
                                network.AddRoad(splitRoad);
                            break;
                        }
                    }
                    Vector3 newPos = state.pos + length * direction;
                    if(noIntersects(newPos, 2.0f) <= 1){
                        road.AddLast(newPos);
                        network.AddRoad(road);
                        state.pos = newPos;
                    }
                }
                catch(KeyNotFoundException)
                {
                    switch(c)
                    {
                        case '+':
                            state.angle += UnityEngine.Random.Range(30*toRad,55*toRad);
                            break;
                        case '-':
                            state.angle -= UnityEngine.Random.Range(30*toRad,55*toRad);
                            break;
                        case '[':
                            states.Enqueue(state);
                            state = states.Dequeue();
                            break;
                        case ']':
                            try{state = states.Dequeue();}
                            catch(InvalidOperationException){break;}
                            break;
                    }
                    state.angle %= 2*pi;
                }
            }
            tree = newTree;
        }
        /// <summary>
        /// Generates a grid with varying size.
        /// </summary>
        /// <param name="state">The initial state of the grid, containing position and direction.</param>
        /// <returns></returns>
        private State Grid(State state){

            int grids = UnityEngine.Random.Range(3,10);
            Queue<State> workSites = new Queue<State>();
            LinkedList<Vector3> road = new LinkedList<Vector3>();
            road.AddLast(state.pos);
            workSites.Enqueue(state);
            State workSite = new State();
            for (int i = 0; i < grids; i++)
            {
                try{workSite = workSites.Dequeue();
                }catch(InvalidOperationException){
                    return workSite;
                }
                int type = UnityEngine.Random.Range(0,4);
                float size;
                switch(type)
                { //Some randomness in square size
                    case 1:
                        size = 1.5f;
                        break;
                    case 2: 
                        size = 2;
                        break;
                    case 3:
                        size = 2.5f;
                        break;
                    default:
                        size = 1;
                        break;
                }
                for (int j = 0; j < 4; j++) // A square road is created
                {
                    workSite.pos += size * new Vector3(Mathf.Cos((float) workSite.angle), 0, Mathf.Sin((float) workSite.angle));
                    if(noIntersects(workSite.pos, 2.0f) <= 1){
                        road.AddLast(workSite.pos);
                        workSite.angle += 90*toRad;
                    }else{
                        break;
                    }
                }
                Vector3 newPos = workSite.pos + size * new Vector3(Mathf.Cos((float) workSite.angle), 0, Mathf.Sin((float) workSite.angle));
                if(UnityEngine.Random.Range(0,1) < 0.5f){ //Some randomness to mix up the order of which worksites are added to the queue
                    if(noIntersects(workSite.pos, 2.0f) <= 1)
                        workSites.Enqueue(new State(workSite.pos, workSite.angle - 90*toRad));
                    if(noIntersects(newPos,2.0f) <= 1)
                        workSites.Enqueue(new State(newPos, workSite.angle));

                }else{
                    if(noIntersects(newPos,2.0f) <= 1)
                        workSites.Enqueue(new State(newPos, workSite.angle));
                    if(noIntersects(workSite.pos, 2.0f) <= 1)
                        workSites.Enqueue(new State(workSite.pos, workSite.angle - 90*toRad));
                }
            }
            return workSite;

        }
        /// <summary>
        /// Given a position and a radius, returns the number of intersections that can be found within.
        /// </summary>
        /// <param name="pos">The point from which the radius is defined.</param>
        /// <param name="radius">the radius within which an intersection is considered</param>
        /// <returns>the number of intersections within the radius</returns>
        private int noIntersects(Vector3 pos, float radius){
            int n = 0;
            foreach (Vector3 sect in network.Intersections)
            {
                if(Vector3.Distance(pos, sect) <= radius)
                    n++;
            }
            return n;
        }
    }
}