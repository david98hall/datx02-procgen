using System;
using System.Collections.Generic;
using System.Text;
using BIAS.Utils.Interfaces;
using BIAS.PCG.Terrain;
using UnityEngine;
using BIAS.Utils;
using Random = System.Random;

namespace BIAS.PCG.Cities.Roads
{
    /// <summary>
    /// A way of generating roads using a non-deterministic L-system.
    /// </summary>
    internal class LSystem
    {
        /// <summary>
        /// Describes the position and direction of the L-system
        /// </summary>
        private class State{
            internal Vector3 pos;
            internal double angle;

            internal State(Vector3 pos, double angle)
            {
                this.pos = pos;
                this.angle = angle;
            }
            internal State(){
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

        private readonly IDictionary<char, string> ruleset = new Dictionary<char, string>();
        private StringBuilder tree;
        private readonly float minX;
        private readonly float minZ;
        private readonly float maxX;
        private readonly float maxZ;
        public readonly RoadNetwork network = new RoadNetwork();
        private State state;
        private readonly Queue<State> states = new Queue<State>();
        private const float toRad = Mathf.Deg2Rad;
        private const float pi = Mathf.PI;

        private readonly IInjector<TerrainInfo> _terrainInjector;
        private TerrainInfo TerrainInfo => _terrainInjector.Get();

        internal LSystem(char c, Vector2 origin, IInjector<TerrainInfo> terrainInjector)
        {
            _terrainInjector = terrainInjector;
            var heightMap = TerrainInfo.HeightMap;
            minX = TerrainInfo.Offset.x;
            minZ = TerrainInfo.Offset.z;
            maxX = heightMap.GetLength(0)-1 + TerrainInfo.Offset.x;
            maxZ = heightMap.GetLength(1)-1 + TerrainInfo.Offset.z;
            state = new State(new Vector3(origin.x, 0, origin.y), 0);
            ruleset.Add('F',"F+FB-");
            ruleset.Add('S', "GB-FB[");
            ruleset.Add('B',"FS[F+");
            ruleset.Add('G',"GF-");
            tree = new StringBuilder(c.ToString());
        }

        public override string ToString(){
            return tree.ToString();
        }
        
        /// <summary>
        /// Rewrites the String within the L-system according to the ruleset, and generates roads.
        /// </summary>
        internal void Rewrite()
        {
            Random rdm = new Random();
            int tries;
            StringBuilder newTree = new StringBuilder();
            float range = 4.0f;
            foreach (char c in tree.ToString())
            {
                try
                {
                    string r = ruleset[c];
                    newTree.Append(r);
                    LinkedList<Vector3> road = new LinkedList<Vector3>();
                    Vector3 direction = new Vector3(Mathf.Cos((float) state.angle), 0, Mathf.Sin((float) state.angle));
                    road.AddLast(state.pos);
                    float length = rdm.Next(2,4);
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
                                splitAngle = state.angle + MathUtils.RandomInclusiveFloat(45*toRad,75*toRad);
                            }else
                            {
                                splitAngle = state.angle - MathUtils.RandomInclusiveFloat(45*toRad,75*toRad);
                            }
                            Vector3 splitDir = new Vector3(Mathf.Cos((float) splitAngle), 0, Mathf.Sin((float) splitAngle));
                            State splitState = new State();
                            switch(c)
                            {
                                case 'S':{
                                    splitState = new State(state.pos + length * splitDir, splitAngle);
                                    tries = 0;
                                    while (tries < 10 && !isWithinMesh(splitState.pos))
                                    {
                                        if(condition >= 0.5)
                                        {
                                            splitAngle += MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
                                        }else
                                        {
                                            splitAngle -= MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
                                        }
                                        splitDir = new Vector3(Mathf.Cos((float) splitAngle), 0, Mathf.Sin((float) splitAngle));
                                        splitState = new State(state.pos + length * splitDir, splitAngle);
                                        tries++;
                                    }
                                    intersects = noIntersects(splitState.pos, range);
                                    if(intersects <= 1 && tries < 10){
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
                                    splitState = new State(state.pos + 2*length*splitDir + length*rotate90, splitAngle);
                                    if(!isWithinMesh(splitState.pos)){
                                        break;
                                    }
                                    intersects = noIntersects(splitState.pos, range);
                                    if(intersects < 1){
                                        Vector3 corner = state.pos + 2 * length * splitDir;
                                        if(isWithinMesh(corner)){
                                            splitRoad.AddLast(corner);
                                            splitRoad.AddLast(splitState.pos);
                                            states.Enqueue(splitState);
                                            LinkedList<Vector3> row2 = new LinkedList<Vector3>();
                                            Vector3 row2End = state.pos + 1 * length * (splitDir + rotate90);
                                            if(isWithinMesh(row2End)){
                                                row2.AddLast(state.pos + 1 * length * splitDir);
                                                row2.AddLast(row2End);
                                                network.AddRoad(row2);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                            if(intersects <= 1 && splitRoad.Count > 1){
                                bool withinMesh = true;
                                foreach (Vector3 node in splitRoad)
                                {
                                    if(!isWithinMesh(node)){
                                        withinMesh = false;
                                        break;
                                    }
                                }
                                if(withinMesh){
                                    network.AddRoad(splitRoad);
                                }
                            }
                            break;
                        }
                        case 'G': //Create a grid
                            state = Grid(state);
                            break;
                        
                    }
                    Vector3 newPos = state.pos + length * direction;
                    tries = 0;
                    while(tries < 10 && !isWithinMesh(newPos)){
                    if(state.angle > 0){
                        state.angle += MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
                    }else{
                        state.angle -= MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
                    }
                    direction = new Vector3(Mathf.Cos((float) state.angle), 0, Mathf.Sin((float) state.angle));
                    newPos = state.pos + length * direction;
                    tries++;
                    }
                    if(noIntersects(newPos, range) <= 1 && tries < 10){
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
                            state.angle += MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
                            break;
                        case '-':
                            state.angle -= MathUtils.RandomInclusiveFloat(30*toRad,55*toRad);
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
        private State Grid(State state)
        {
            var rdm = new Random();
            int grids = rdm.Next(3,10);
            Queue<State> workSites = new Queue<State>();
            workSites.Enqueue(state);
            float range = 3.0f;
            State workSite = new State();
            for (int i = 0; i < grids; i++)
            {
                try{workSite = workSites.Dequeue();
                }catch(InvalidOperationException){
                    return workSite;
                }
                int type = rdm.Next(0,4);
                float size;
                switch(type)
                { //Some randomness in square size
                    case 1:
                        size = 3f;
                        break;
                    case 2: 
                        size = 4;
                        break;
                    case 3:
                        size = 5f;
                        break;
                    default:
                        size = 2;
                        break;
                }
                LinkedList<Vector3> road = new LinkedList<Vector3>();
                road.AddLast(state.pos);
                for (int j = 0; j < 4; j++) // A square road is created
                {
                    workSite.pos += size * new Vector3(Mathf.Cos((float) workSite.angle), 0, Mathf.Sin((float) workSite.angle));
                    if(isWithinMesh(workSite.pos)){
                        if(noIntersects(workSite.pos, range) < 2){
                        road.AddLast(workSite.pos);
                        workSite.angle += 90*toRad;
                        }else{break;}
                    }else{break;}
                }
                if(road.Count > 1){
                    bool withinMesh = true;
                    foreach (Vector3 node in road)
                    {
                        if(!isWithinMesh(node)){
                            withinMesh = false;
                            break;
                        }
                    }
                    if(withinMesh){
                        network.AddRoad(road);
                        if(road.Count > 4){
                            Vector3 newPos = workSite.pos + size * new Vector3(Mathf.Cos((float) workSite.angle), 0, Mathf.Sin((float) workSite.angle));
                            if(rdm.Next(0,2) < 0.5f){ //Some randomness to mix up the order in which worksites are added to the queue
                                if(noIntersects(workSite.pos, range) <= 1)
                                    workSites.Enqueue(new State(workSite.pos, workSite.angle - 90*toRad));
                                if(noIntersects(newPos, range) <= 1)
                                    workSites.Enqueue(new State(newPos, workSite.angle));

                            }else{
                                if(noIntersects(newPos, range) <= 1)
                                    workSites.Enqueue(new State(newPos, workSite.angle));
                                if(noIntersects(workSite.pos, range) <= 1)
                                    workSites.Enqueue(new State(workSite.pos, workSite.angle - 90*toRad));
                            }
                        }
                    }
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
        private bool isWithinMesh(Vector3 pos){
            return (pos.x < maxX && pos.x > minX && pos.z < maxZ && pos.z > minZ);
        }
    }
}