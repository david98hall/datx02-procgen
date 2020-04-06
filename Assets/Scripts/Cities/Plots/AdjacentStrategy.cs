using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cities.Roads;
using Interfaces;
using Utils.Geometry;

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
                var v = Maths3D.PerpendicularClockwise(roadVector);
                vertices.AddLast(end - v.normalized * plotLength);
                vertices.AddLast(start - v.normalized * plotLength);
                
                vertices.AddLast(start);
                vertices.AddLast(end);
                plots.Add(new Plot(vertices));
            }

            return plots;
        }
    }
}
