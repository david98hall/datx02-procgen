using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// A RoadNetwork strategy for testing.
    /// </summary>
    internal class SampleStrategy : Strategy<float[,], RoadNetwork>
    {
        
        internal SampleStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
        }
        
        public override RoadNetwork Generate()
        {
            var roadNetwork = new RoadNetwork();
            // roadNetwork.AddRoads(CreateTestExample1());
            // roadNetwork.AddRoads(CreateTestExample2());
            roadNetwork.AddRoads(new HashSet<IEnumerable<Vector3>> { CreateHighway() });
            return roadNetwork;
        }

        #region Test examples
        
        // Cycles within cycle
        private static IEnumerable<IEnumerable<Vector3>> CreateTestExample1()
        {
            var roads = new HashSet<IEnumerable<Vector3>> {CreateCircleRoad(7.5f, 0.5f)};
            //roads.AddRange(CreatePyramid(5f, -2.5f, -2.5f));
            return roads;
        }
        
        // No cycles within other cycles
        private static IEnumerable<IEnumerable<Vector3>> CreateTestExample2()
        {
            var roads = new HashSet<IEnumerable<Vector3>>
            {
                CreateCircleRoad(7.5f, 0.5f, 22.5f),
                CreateCircleRoad(5, 0.5f, 22.5f),
                CreateCircleRoad(2.5f, 0.5f, 22.5f)
            };
            //roads.AddRange(CreatePyramid(5f, 35, -3.75f));
            return roads;
        }
        #endregion
        
        #region Types of roads
        private static IEnumerable<Vector3> CreateSquareRoad(float size, float offsetX = 0, float offsetZ = 0)
        {
            var squareRoad = new LinkedList<Vector3>();
            squareRoad.AddLast(new Vector3(offsetX, 0, offsetZ));
            squareRoad.AddLast(new Vector3(offsetX, 0, offsetZ + size));
            squareRoad.AddLast(new Vector3(offsetX + size, 0, offsetZ + size));
            squareRoad.AddLast(new Vector3(offsetX + size, 0, offsetZ));
            squareRoad.AddLast(new Vector3(offsetX, 0, offsetZ));

            return squareRoad;
        }
        
        private static IEnumerable<Vector3> CreateCircleRoad(float radius, float resolution, float offsetX = 0, float offsetZ = 0)
        {
            var circleRoad = new LinkedList<Vector3>();
            for (var i = 0f; i < 2 * Math.PI; i += resolution)
            {
                var x = (float) (radius * Math.Cos(i)) + offsetX;
                var z = (float) (radius * Math.Sin(i)) + offsetZ;
                circleRoad.AddLast(new Vector3(x, 0, z));
            }
            circleRoad.AddLast(new Vector3(radius + offsetX, 0, offsetZ));

            return circleRoad;
        }

        #endregion
        
        #region Example road networks

        private static IEnumerable<IEnumerable<Vector3>> CreatePyramid(
            float size, float offsetX = 0, float offsetZ = 0)
        {
            var roads = new HashSet<IEnumerable<Vector3>> {CreateSquareRoad(size, offsetX, offsetZ)};

            var diagonalRoad1 = new LinkedList<Vector3>();
            diagonalRoad1.AddLast(new Vector3(offsetX, 0, offsetZ + size));
            diagonalRoad1.AddLast(new Vector3(offsetX + size / 2, 2, offsetZ + size / 2));
            diagonalRoad1.AddLast(new Vector3(offsetX + size, 0, offsetZ));
            roads.Add(diagonalRoad1);
            
            var diagonalRoad2 = new LinkedList<Vector3>();
            diagonalRoad2.AddLast(new Vector3(offsetX, 0, offsetZ));
            diagonalRoad2.AddLast(new Vector3(offsetX + size / 2, 2, offsetZ + size / 2));
            diagonalRoad2.AddLast(new Vector3(offsetX + size, 0, offsetZ + size));
            roads.Add(diagonalRoad2);

            var straightRoad = new LinkedList<Vector3>();
            straightRoad.AddLast(new Vector3(offsetX, 0, offsetZ + size / 4));
            straightRoad.AddLast(new Vector3(offsetX + size, 0, offsetZ + size / 4));
            roads.Add(straightRoad);

            return roads;
        }


        private static IEnumerable<Vector3> CreateHighway()
        {
            var highway = new LinkedList<Vector3>();
            highway.AddLast(new Vector3(0f, 0f, 0f));
            highway.AddLast(new Vector3(7f, 0f, 4f));
            highway.AddLast(new Vector3(10f, 0f, 10f));
            highway.AddLast(new Vector3(11f, 0f, 5f));

            return highway;
        }
        #endregion
        
    }
}