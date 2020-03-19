using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cities.Roads;
using Extensions;
using Interfaces;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Plots
{
    internal class PlotStrategySample : PlotStrategy
    {

        public PlotStrategySample(IInjector<RoadNetwork> roadNetworkInjector) : base(roadNetworkInjector)
        {
        }
        
        public override IEnumerable<Plot> Generate()
        {
            var plots = new HashSet<Plot>();
            
            var roadParts = RoadNetwork.GetRoadParts().ToList();

            plots.AddRange(GenerateTrianglePlots(roadParts));

            var plotStrings = plots.Select(plot =>
            {
                const string arrow = " -> ";
                var plotString = plot.Vertices.Aggregate("Plot: ", (current, vector) => current + (vector + arrow));
                return plotString.Substring(0, plotString.Length - arrow.Length);
            });

            Debug.Log("Number of plots: " + plots.Count);
            
            foreach (var plotString in plotStrings)
            {
                Debug.Log(plotString);
            }
            
            return plots;
        }

        private IEnumerable<Plot> GenerateTrianglePlots(IReadOnlyCollection<(Vector3 Start, Vector3 End)> roadParts)
        {
            var plots = new HashSet<Plot>();
            var vertexPairs = 
                RoadNetwork.RoadVertices.Zip(RoadNetwork.RoadVertices, (v1, v2) => (v1, v2));

            var potentialTriangleEdges = new HashSet<(Vector3 Start, Vector3 End)>();

            foreach (var (Start1, End1) in vertexPairs)
            {
                var saveSegment = true;
                
                foreach (var (Start2, End2) in roadParts)
                {
                    var isIntersection = Maths3D.LineSegmentIntersection(
                            out var intersection, Start1, End1, Start2, End2);
                    saveSegment &= !isIntersection || intersection.Equals(Start2) || intersection.Equals(End2);
                }

                if (saveSegment)
                {
                    potentialTriangleEdges.Add((Start1, End1));
                }
            }
            
            foreach (var (Start, End) in potentialTriangleEdges)
            {
                foreach (var endNeighbour in RoadNetwork.GetAdjacentVertices(End))
                {
                    foreach (var endNeighbourNeighbour in RoadNetwork.GetAdjacentVertices(endNeighbour))
                    {
                        if (!RoadNetwork.IsAdjacent(endNeighbourNeighbour, Start)) continue;
                        // Triangle found
                        var trianglePlot = new Plot();
                        trianglePlot.SetShapeVertices(new []{Start, End, endNeighbour, Start});
                        plots.Add(trianglePlot);
                    }
                }
            }
            
            return plots;
        }
        
        /*
            var cycleStrings = stronglyConnectedRoads.Select(road =>
            {
                const string arrow = " -> ";
                var cycle = road.Aggregate("Cycle: ", (current, vector) => current + (vector + arrow));
                return cycle.Substring(0, cycle.Length - arrow.Length);
            });

            foreach (var cycle in cycleStrings)
            {
                Debug.Log(cycle);
            }
            */
        
    }
}