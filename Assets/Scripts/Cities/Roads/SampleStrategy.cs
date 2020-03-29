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
            
            const float size = 5f;
            const float radius = size * 1.5f;

            var circleRoad = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += 0.1f)
            {
                var x = (float) (radius * Math.Cos(i));
                var z = (float) (radius * Math.Sin(i));
                circleRoad.AddLast(new Vector3(x, 0, z));
            }
            circleRoad.AddLast(new Vector3(radius, 0, 0));
            roadNetwork.AddRoad(circleRoad);
            
            const float offset1 = -size / 2;
            var squareRoad = new LinkedList<Vector3>();
            squareRoad.AddLast(new Vector3(offset1, 0, offset1));
            squareRoad.AddLast(new Vector3(offset1, 0, offset1 + size));
            squareRoad.AddLast(new Vector3(offset1 + size, 0, offset1 + size));
            squareRoad.AddLast(new Vector3(offset1 + size, 0, offset1));
            squareRoad.AddLast(new Vector3(offset1, 0, offset1));
            roadNetwork.AddRoad(squareRoad);
            
            var diagonalRoad1 = new LinkedList<Vector3>();
            diagonalRoad1.AddLast(new Vector3(offset1, 0, offset1 + size));
            diagonalRoad1.AddLast(new Vector3(offset1 + size / 2, 2, offset1 + size / 2));
            diagonalRoad1.AddLast(new Vector3(offset1 + size, 0, offset1));
            roadNetwork.AddRoad(diagonalRoad1);
            
            var diagonalRoad2 = new LinkedList<Vector3>();
            diagonalRoad2.AddLast(new Vector3(offset1, 0, offset1));
            diagonalRoad2.AddLast(new Vector3(offset1 + size / 2, 2, offset1 + size / 2));
            diagonalRoad2.AddLast(new Vector3(offset1 + size, 0, offset1 + size));
            roadNetwork.AddRoad(diagonalRoad2);

            var straightRoad = new LinkedList<Vector3>();
            straightRoad.AddLast(new Vector3(offset1, 0, offset1 + size / 4));
            straightRoad.AddLast(new Vector3(offset1 + size, 0, offset1 + size / 4));
            roadNetwork.AddRoad(straightRoad);

            return roadNetwork;
        }
    }
}