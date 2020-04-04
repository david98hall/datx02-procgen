using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cities.Roads;
using Interfaces;

namespace Cities.Plots 
{
    internal class AdjacentStrategy : Strategy<RoadNetwork, IEnumerable<Plot>>
    {
        public AdjacentStrategy(IInjector<RoadNetwork> injector) : base(injector)
        {
        }

        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            var roadNetwork = Injector.Get();

            foreach (var (start, end) in roadNetwork.GetRoadParts())
            {
                var vertices = new LinkedList<Vector3>();
                vertices.AddLast(end);

                var roadVector = end - start;
                var plotLength = Vector3.Magnitude(roadVector);
                var v = PerpendicularClockwise(roadVector);
                vertices.AddLast(end - v.normalized * plotLength);
                vertices.AddLast(start - v.normalized * plotLength);
                
                vertices.AddLast(start);
                vertices.AddLast(end);
                plots.Add(new Plot(vertices));
            }

            return plots;
        }
        
        /// <summary>
        /// Finds the perpendicular clockwise vector in the x-z plane.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>The perpendicular clockwise vector to </returns>
        private static Vector3 PerpendicularClockwise(Vector3 vector)
        {
            return new Vector3(vector.z, vector.y, -vector.x);
        }
    }
}
