using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using Utils.Geometry;

namespace Cities.Roads
{
    /// <summary>
    /// Represents a network of roads.
    /// </summary>
    public class RoadNetwork : ICloneable
    {
        
        // Adjacency set for road network vectors
        private readonly IDictionary<Vector3, ICollection<Vector3>> _roadNetwork;

        public IEnumerable<Vector3> RoadVertices => _roadNetwork.Keys.Select(v => v.Clone());
        
        public IEnumerable<Vector3> Intersections => 
            _roadNetwork.Keys.Where(v => _roadNetwork[v].Count > 1).Select(v => v.Clone());

        #region Constructors
        
        public RoadNetwork()
        {
            _roadNetwork = new Dictionary<Vector3, ICollection<Vector3>>();
        }

        public RoadNetwork(RoadNetwork roadNetwork) : this(roadNetwork._roadNetwork)
        {
        }
        
        private RoadNetwork(IDictionary<Vector3, ICollection<Vector3>> roadNetwork)
        {
            _roadNetwork = CloneRoadNetwork(roadNetwork);
        }

        #endregion
        
        #region Adding roads

        /// <summary>
        /// Adds roads to the city's road network.
        /// </summary>
        /// <param name="roads">The roads to add.</param>
        /// <typeparam name="T">An IEnumerable type of Vector3s.</typeparam>
        public void AddRoads<T>(IEnumerator<T> roads) where T : IEnumerable<Vector3>
        {
            while (roads.MoveNext())
            {
                AddRoad(roads.Current);
            }
        }
        
        /// <summary>
        /// Adds roads to the city's road network.
        /// </summary>
        /// <param name="roads">The roads to add.</param>
        /// <typeparam name="T">An IEnumerable type of Vector3s.</typeparam>
        public void AddRoads<T>(IEnumerable<T> roads) where T : IEnumerable<Vector3>
        {
            AddRoads(roads.GetEnumerator());
        }
        
        /// <summary>
        /// Adds roads to the city's road network.
        /// </summary>
        /// <param name="roads">The roads to add.</param>
        /// <typeparam name="T">An IEnumerable type of Vector3s.</typeparam>
        public void AddRoads<T>(params T[] roads) where T : IEnumerable<Vector3>
        {
            AddRoads((IEnumerator<IEnumerable<Vector3>>)roads.GetEnumerator());
        }
        
        /// <summary>
        /// Creates a road by creating edges between vertices.
        /// </summary>
        /// <param name="roadVertices">The vertices of the road.</param>
        public void AddRoad(IEnumerator<Vector3> roadVertices)
        {
            // Check that there is at least one vertex
            if (!roadVertices.MoveNext()) return;
            var previousVertex = roadVertices.Current;
            
            // Check that there is at least two vertices
            if (!roadVertices.MoveNext()) return;

            // The road vertices are at least two, add a road between the first two
            AddRoad(previousVertex, roadVertices.Current, GetRoadParts());
            previousVertex = roadVertices.Current;
            
            // Add roads between the rest of the vertices along the full road
            while (roadVertices.MoveNext())
            {
                AddRoad(previousVertex, roadVertices.Current, GetRoadParts());
                previousVertex = roadVertices.Current;
            }
            
            // At the last the vertex of the road
            AddRoadVertex(roadVertices.Current);
        }

        private void AddRoad(Vector3 start, Vector3 end, IEnumerable<(Vector3, Vector3)> roadParts)
        {
            // Adds the start to the road network
            AddRoadVertex(start);
            
            if (!SplitAtIntersections(start, end, roadParts)) 
            {
                // Add an edge straight from the previous vertex to the current one
                _roadNetwork[start].Add(end);
            }
        }

        // Returns true if a split occurred
        private bool SplitAtIntersections(Vector3 lineStart, Vector3 lineEnd, IEnumerable<(Vector3, Vector3)> roadParts)
        {
            var intersections = GetIntersectionPoints(
                lineStart, lineEnd, roadParts.GetEnumerator());

            // No intersections; return false
            if (!intersections.Any()) return false;
            
            // Add all intersection points on other road parts if there are any
            foreach (var (start, intersection, end) in intersections)
            {
                AddRoadVertex(intersection);
                
                // Remove full roads since they now intersect
                _roadNetwork[lineStart].Remove(lineEnd);
                _roadNetwork[start].Remove(end);

                // Add road from one of the start points to the intersection
                if (!_roadNetwork[intersection].Contains(start) && !intersection.Equals(start))
                    _roadNetwork[start].Add(intersection);

                // Add road from the other start point to the intersection
                if (!_roadNetwork[intersection].Contains(lineStart)&& !intersection.Equals(lineStart)) 
                    _roadNetwork[lineStart].Add(intersection);

                // Add road from the intersection to one of the end points
                if (!_roadNetwork[end].Contains(intersection) && !intersection.Equals(end)) 
                    _roadNetwork[intersection].Add(end);

                // Add road from the intersection to the other end point
                if ((!_roadNetwork.ContainsKey(lineEnd) || !_roadNetwork[lineEnd].Contains(intersection)) &&
                    !intersection.Equals(lineEnd))
                {
                    _roadNetwork[intersection].Add(lineEnd);      
                }

                // Update the start point to the intersection, in case there
                // are more intersections along the rest of the road
                lineStart = intersection;
            }

            // Intersections found, return true
            return true;
        }
        
        private static ICollection<(Vector3 start, Vector3 intersection, Vector3 end)> GetIntersectionPoints(
            Vector3 linePoint1, Vector3 linePoint2, IEnumerator<(Vector3, Vector3)> roadParts)
        {
            var intersectionPoints = new HashSet<(Vector3, Vector3, Vector3)>();
            
            while (roadParts.MoveNext())
            {
                var (partStart, partEnd) = roadParts.Current;
                
                // If the argument line intersects the road part line
                if (!Maths3D.LineSegmentIntersection(
                    out var intersectionPoint,
                    linePoint1, linePoint2,
                    partStart, partEnd)) continue;
                
                // Register the intersection point
                intersectionPoints.Add((partStart, intersectionPoint, partEnd));
            }
            
            return intersectionPoints;
        }
        
        /// <summary>
        /// Creates a road by creating edges between vertices.
        /// </summary>
        /// <param name="roadVertices">The vertices of the road.</param>
        public void AddRoad(IEnumerable<Vector3> roadVertices)
        {
            AddRoad(roadVertices.GetEnumerator());
        }

        /// <summary>
        /// Creates a road by creating edges between vertices.
        /// </summary>
        /// <param name="roadVertices">The vertices of the road.</param>
        public void AddRoad(params Vector3[] roadVertices)
        {
            AddRoad((IEnumerator<Vector3>)roadVertices.GetEnumerator());
        }
        
        private void AddRoadVertex(Vector3 vertex)
        {
            if (!_roadNetwork.ContainsKey(vertex))
            {
                _roadNetwork.Add(vertex, new HashSet<Vector3>());
            }
        }
        
        #endregion

        #region Get road parts

        /// <summary>
        /// Gets all parts of the road network in this city.
        /// </summary>
        /// <returns>All road parts.</returns>
        public IEnumerable<(Vector3, Vector3)> GetRoadParts()
        {
            return _roadNetwork.Keys.SelectMany(GetRoadParts);
        }

        private IEnumerable<(Vector3, Vector3)> GetRoadParts(Vector3 startVertex)
        {
            return _roadNetwork[startVertex].Select(endVertex => (startVertex, endVertex));
        }

        #endregion

        #region Get roads
        
        /// <summary>
        /// Returns all roads in this network.
        /// </summary>
        /// <returns>All roads.</returns>
        public IEnumerable<IEnumerable<Vector3>> GetRoads()
        {
            var roads = new HashSet<LinkedList<Vector3>>();
            var visited = new HashSet<Vector3>();

            // Go through all vertices in the road network and look for roads
            foreach (var current in _roadNetwork.Keys)
            {
                var foundRoads = GetRoads(current, visited);
                // If the found roads is null, the current vertex had already been visited; skip it.
                if (foundRoads == null) continue;

                // Add all roads found when starting the search from the current vertex
                foreach (var road in foundRoads)
                {
                    roads.Add(road);
                }
            }
            
            return roads;
        }

        private ISet<LinkedList<Vector3>> GetRoads(Vector3 start, ISet<Vector3> visited)
        {
            // If the vertex has already been visited, abort
            if (visited.Contains(start))
                return null;

            var roads = new HashSet<LinkedList<Vector3>>();
            
            // Mark the start vertex as visited
            visited.Add(start);

            // Traverse all of the start node's neighbours
            foreach (var neighbour in _roadNetwork[start])
            {
                // Look up roads by searching from the neighbour
                var neighbourRoads = GetRoads(neighbour, visited);

                // Create a new road
                var road = new LinkedList<Vector3>();
                road.AddLast(start.Clone());
                roads.Add(road);
                
                switch (neighbourRoads?.Count ?? 0)
                {
                    // No neighbour roads
                    case 0:
                        // If the neighbour roads is null, the neighbour had already been visited; skip its neighbours.
                        // Make a road from start to neighbour
                        road.AddLast(neighbour.Clone());
                        break;
                    
                    // One neighbour road was found
                    case 1:
                        // Append the only found road going out from the neighbour to the start vertex
                        // to extend the road.
                        road.AddRange(neighbourRoads.First());
                        break;
                    
                    // At least two neighbour roads were found
                    default:
                    {
                        // Add roads found when searching from the neighbour vertex
                        var neighbourRoadsEnumerator = neighbourRoads.GetEnumerator();
                        while (neighbourRoadsEnumerator.MoveNext())
                        {
                            if (road.Count == 1)
                            {
                                // Only the start vertex has been added to the road starting from it.
                                // Add the first of the found neighbour roads to the start vertex road.
                                road.AddRange(neighbourRoadsEnumerator.Current);
                            }
                            else
                            {
                                // A road has already been added from the start vertex.
                                // Add the roads as they were created when searching from the neighbour vertex.
                                roads.Add(neighbourRoadsEnumerator.Current);
                            }
                        }

                        break;
                    }
                }
            }

            return roads;
        }

        public IEnumerable<Vector3> GetAdjacentVertices(Vector3 vector)
        {
            return !_roadNetwork.ContainsKey(vector) ? null : _roadNetwork[vector].Select(v => v.Clone());
        }

        public int GetNumberOfAdjacentVectors(Vector3 vector)
        {
            return !_roadNetwork.ContainsKey(vector) ? -1 : _roadNetwork[vector].Count;
        }

        public bool IsAdjacent(Vector3 v1, Vector3 v2)
        {
            return _roadNetwork.ContainsKey(v1) && _roadNetwork[v1].Contains(v2);
        }
        
        #endregion

        public IReadOnlyDictionary<Vector3, ICollection<Vector3>> ConvertToUndirectedGraph()
        {
            var undirected = new Dictionary<Vector3, ICollection<Vector3>>();
            
            // Helper method
            void AddVertex(Vector3 vertex)
            {
                if (!undirected.ContainsKey(vertex)) 
                    undirected.Add(vertex, new HashSet<Vector3>());
            }
            
            // If A -> B is an edge in _roadNetwork, add B -> A as an edge
            // and do this for all roads to make the network undirected.
            foreach (var vertex in _roadNetwork.Keys)
            {
                var vertexClone = vertex.Clone();
                AddVertex(vertexClone);
                foreach (var neighbour in _roadNetwork[vertex])
                {
                    var neighbourClone = neighbour.Clone();
                    undirected[vertexClone].Add(neighbour);
                    AddVertex(neighbourClone);
                    undirected[neighbourClone].Add(vertexClone);
                }
            }

            return undirected;
        }

        #region Cloning
        
        public object Clone() => new RoadNetwork(this);
        
        private static IDictionary<Vector3, ICollection<Vector3>> CloneRoadNetwork(
            IDictionary<Vector3, ICollection<Vector3>> roadNetwork)
        {
            var copy = new Dictionary<Vector3, ICollection<Vector3>>();

            // Clone the adjacency set
            foreach (var key in roadNetwork.Keys)
            {
                var valuesCopy = new HashSet<Vector3>();

                foreach (var value in roadNetwork[key])
                {
                    valuesCopy.Add(value.Clone());
                }
                
                copy.Add(key.Clone(), valuesCopy);
            }
            
            return copy;
        }

        #endregion
        
    }
}