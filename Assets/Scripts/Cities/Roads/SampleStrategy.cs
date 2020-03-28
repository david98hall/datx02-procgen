using System;
using System.Collections.Generic;
using Extensions;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    internal class SampleStrategy : Strategy<float[,], RoadNetwork>
    {
        
        internal SampleStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
        }
        
        public override RoadNetwork Generate()
        {
            var roadNetwork = new RoadNetwork();
            
            const float offset1 = 0;
            const float size = 5;

            /*
            var road1 = new LinkedList<Vector3>();
            road1.AddLast(new Vector3(offset1, 0, offset1 + 1));
            road1.AddLast(new Vector3(offset1 + size, 0, offset1 + 1));
            roadNetwork.AddRoad(road1);
            
            var road2 = new LinkedList<Vector3>();
            road2.AddLast(new Vector3(offset1 + 1, 0, offset1));
            road2.AddLast(new Vector3(offset1 + 1, 0, offset1 + 2));
            roadNetwork.AddRoad(road2);

            var road3 = new LinkedList<Vector3>();
            road3.AddLast(new Vector3(offset1 + 4, 0, offset1 + 2));
            road3.AddLast(new Vector3(offset1 + 3, 1, offset1 + 1));
            road3.AddLast(new Vector3(offset1 + 2, 0, offset1));
            roadNetwork.AddRoad(road3);

            var road4 = new LinkedList<Vector3>();
            road4.AddLast(new Vector3(offset1 + 7, 0, offset1 + 2));
            road4.AddLast(new Vector3(offset1 + 6, 1, offset1 + 1));
            road4.AddLast(new Vector3(offset1 + 5, 0, offset1));
            roadNetwork.AddRoad(road4);
            */

            var road1 = new LinkedList<Vector3>();
            road1.AddLast(new Vector3(offset1, 0, offset1));
            road1.AddLast(new Vector3(offset1, 0, offset1 + size));
            road1.AddLast(new Vector3(offset1 + size, 0, offset1 + size));
            road1.AddLast(new Vector3(offset1 + size, 0, offset1));
            road1.AddLast(new Vector3(offset1, 0, offset1));
            roadNetwork.AddRoad(road1);
            
            var road2 = new LinkedList<Vector3>();
            road2.AddLast(new Vector3(offset1, 0, offset1 + size));
            road2.AddLast(new Vector3(offset1 + size / 2, 2, offset1 + size / 2));
            road2.AddLast(new Vector3(offset1 + size, 0, offset1));
            roadNetwork.AddRoad(road2);
            
            var road3 = new LinkedList<Vector3>();
            road3.AddLast(new Vector3(offset1, 0, offset1));
            road3.AddLast(new Vector3(offset1 + size / 2, 2, offset1 + size / 2));
            road3.AddLast(new Vector3(offset1 + size, 0, offset1 + size));
            roadNetwork.AddRoad(road3);

            var road4 = new LinkedList<Vector3>();
            road4.AddLast(new Vector3(offset1, 0, offset1 + size / 4));
            road4.AddLast(new Vector3(offset1 + size, 0, offset1 + size / 4));
            roadNetwork.AddRoad(road4);
            
            return roadNetwork;
        }
    }
}