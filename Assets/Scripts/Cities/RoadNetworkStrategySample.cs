using System;
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
            
            // Circular road
            var road1 = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += 0.01f)
            {
                var dX = (float)Math.Cos(i);
                var dZ = (float)Math.Sin(i);
                road1.AddLast(new Vector3(dX, 0, dZ));
            }
            roadNetwork.AddRoad(road1);

            // Road sticking out from the square's top left corner
            var road2 = new LinkedList<Vector3>();
            road2.AddLast(new Vector3(0, 0, 1));
            road2.AddLast(new Vector3(-2.5f, 0, 3.5f));
            road2.AddLast(new Vector3(-4.5f, 0, 2));
            road2.AddLast(new Vector3(-6.5f, 0, 1));
            roadNetwork.AddRoad(road2);
            
            // Circular road 2
            var road3 = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += 0.01f)
            {
                const float offset = 3;
                var dX = (float)Math.Cos(i) + offset;
                var dZ = (float)Math.Sin(i) + offset;
                road3.AddLast(new Vector3(dX, 0, dZ));
            }
            roadNetwork.AddRoad(road3);
            
            return roadNetwork;
        }
    }
}