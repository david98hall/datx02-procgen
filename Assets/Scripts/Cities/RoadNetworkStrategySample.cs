﻿using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities
{
    internal class RoadNetworkStrategySample : RoadNetworkStrategy
    {
        
        internal RoadNetworkStrategySample(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
        }
        
        public override RoadNetwork Generate()
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

            // Road sticking out from the square's top left corner
            var road2 = new LinkedList<Vector3>();
            road2.AddLast(new Vector3(0.5f, 0, 0.5f));
            road2.AddLast(new Vector3(0, 0, 1));
            road2.AddLast(new Vector3(-2.5f, 0, 3.5f));
            road2.AddLast(new Vector3(-4.5f, 0, 2));
            road2.AddLast(new Vector3(-6.5f, 0, 1));
            roadNetwork.AddRoad(road2);
            
            // Circular road 2
            var road3 = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += 0.01f)
            {
                float offset = 3;
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
            
            var road5 = new LinkedList<Vector3>();
            road5.AddLast(new Vector3(offset1, 0, offset1));
            road5.AddLast(new Vector3(offset1 + sqWidth / 2, 0, offset1 + sqWidth / 2));
            road5.AddLast(new Vector3(offset1 + sqWidth, 0, offset1 + sqWidth));
            roadNetwork.AddRoad(road5);
            
            var road6 = new LinkedList<Vector3>();
            road6.AddLast(new Vector3(offset1, 0, offset1 + sqWidth));
            road6.AddLast(new Vector3(offset1 + sqWidth / 2, 0, offset1 + sqWidth / 2));
            road6.AddLast(new Vector3(offset1 + sqWidth, 0, offset1));
            roadNetwork.AddRoad(road6);
            
            return roadNetwork;
        }
    }
}