using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities
{
    internal class RoadNetworkStrategySample : IGenerator<RoadNetwork>
    {
        public RoadNetwork Generate()
        {
            var roadNetwork = new RoadNetwork();
            
            // Road looking like a square
            var road1 = new LinkedList<Vector3>();
            road1.AddLast(new Vector3(0, 0, 0));
            road1.AddLast(new Vector3(0, 0, 1));
            road1.AddLast(new Vector3(1, 0, 1));
            road1.AddLast(new Vector3(1, 0, 0));
            road1.AddLast(new Vector3(0, 0, 0));
            roadNetwork.AddRoad(road1);

            return roadNetwork;
        }
    }
}