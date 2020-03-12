using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    internal class RoadNetworkStrategySample : RoadNetworkStrategy
    {
        
        internal RoadNetworkStrategySample(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
        }
        
        public override RoadNetwork Generate()
        {
            var roadNetwork = new RoadNetwork();
            
            var road2 = new LinkedList<Vector3>();
            road2.AddLast(new Vector3(0.5f, 0, 0.5f));
            road2.AddLast(new Vector3(0, 0, 1));
            road2.AddLast(new Vector3(-2.5f, 0, 3.5f));
            road2.AddLast(new Vector3(-4.5f, 0, 2));
            road2.AddLast(new Vector3(-6.5f, 0, 1));
            roadNetwork.AddRoad(road2);
            
            // Circular road
            var road3 = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += 0.01f)
            {
                const float offset = 3;
                var dX = (float)Math.Cos(i) + offset;
                var dZ = (float)Math.Sin(i) + offset;
                road3.AddLast(new Vector3(dX, 0, dZ));
            }
            roadNetwork.AddRoad(road3);            
            
            // Road looking like a square
            var road4 = new LinkedList<Vector3>();
            const float offset1 = -6;
            const float sqWidth = 5;
            road4.AddLast(new Vector3(offset1, 0, offset1));
            road4.AddLast(new Vector3(offset1, 0, offset1 + sqWidth));
            road4.AddLast(new Vector3(offset1 + sqWidth, 0, offset1 + sqWidth));
            road4.AddLast(new Vector3(offset1 + sqWidth, 0, offset1));
            road4.AddLast(new Vector3(offset1, 0, offset1));
            roadNetwork.AddRoad(road4);

            var road51 = new LinkedList<Vector3>();
            road51.AddLast(new Vector3(offset1, 0, offset1));
            road51.AddLast(new Vector3(offset1 + sqWidth / 2, 2, offset1 + sqWidth / 2));
            road51.AddLast(new Vector3(offset1 + sqWidth, 0, offset1 + sqWidth));
            roadNetwork.AddRoad(road51);
            
            var road61 = new LinkedList<Vector3>();
            road61.AddLast(new Vector3(offset1, 0, offset1 + sqWidth));
            road61.AddLast(new Vector3(offset1 + sqWidth / 2, 2, offset1 + sqWidth / 2));
            road61.AddLast(new Vector3(offset1 + sqWidth, 0, offset1));
            roadNetwork.AddRoad(road61);
            
            var road7 = new LinkedList<Vector3>();
            road7.AddLast(new Vector3(offset1, 0, offset1 + 1));
            road7.AddLast(new Vector3(offset1 + sqWidth, 0, offset1 + 1));
            roadNetwork.AddRoad(road7);

            return roadNetwork;
        }
    }
}