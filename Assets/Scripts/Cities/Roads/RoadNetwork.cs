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
            var firstIteration = true;
            var previousVertex = Vector3.negativeInfinity;
            var roadParts = GetRoadParts().GetEnumerator();
            var i = 1;
            while (roadVertices.MoveNext())
            {
                // Adds the current vertex to the road network
                var currentVertex = roadVertices.Current;                
                AddRoadVertex(currentVertex);
                
                if (!firstIteration)
                {
                    var intersectionPoints = 
                        GetIntersectionPoints(previousVertex, currentVertex, roadParts);
                    if (intersectionPoints.Any())
                    {
                        // Add all intersection points on other road parts if there are any
                        foreach (var (start, intersection, end) in intersectionPoints)
                        {
                            _roadNetwork[start].Remove(end);
                            _roadNetwork[start].Add(intersection);
                            _roadNetwork[previousVertex].Add(intersection);
                            AddRoadVertex(intersection);
                            _roadNetwork[intersection].Add(end);
                            _roadNetwork[intersection].Add(currentVertex);
                        }
                    }
                    else
                    {
                        // Add an edge straight from the previous vertex to the current one
                        _roadNetwork[previousVertex].Add(currentVertex);
                    }
                }
                else
                {
                    firstIteration = false;
                }

                i++;
                
                // Update the previous vertex to the current one
                previousVertex = currentVertex;
            }
        }

        private static ICollection<(Vector3 start, Vector3 intersection, Vector3 end)> GetIntersectionPoints(
            Vector3 linePoint1, Vector3 linePoint2, IEnumerator<(Vector3, Vector3)> roadParts)
        {
            var intersectionPoints = new HashSet<(Vector3 start, Vector3 intersection, Vector3 end)>();
            
            while (roadParts.MoveNext())
            {
                var (partStart, partEnd) = roadParts.Current;
                
                // If the argument line intersects the road part line
                if (Maths3D.Intersection(
                    out var intersectionPoint, 
                    linePoint1, linePoint2, 
                    partStart, partEnd))
                {
                    intersectionPoints.Add((partStart, intersectionPoint, partEnd));
                }
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

            // Traverse all neighbours to the start node
            foreach (var neighbour in _roadNetwork[start])
            {
                // Look roads by searching from the neighbour
                var neighbourRoads = GetRoads(neighbour, visited);

                // Create a new road
                var road = new LinkedList<Vector3>();
                road.AddLast(start);
                roads.Add(road);
                
                switch (neighbourRoads?.Count ?? 0)
                {
                    // No neighbour roads
                    case 0:
                        // If the neighbour roads is null, the neighbour had already been visited; skip its neighbours.
                        // Make a road from start to neighbour
                        road.AddLast(neighbour);
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
        
        #endregion

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
                    valuesCopy.Add(new Vector3(value.x, value.y, value.z));
                }
                
                copy.Add(key, valuesCopy);
            }
            
            return copy;
        }
        
        #endregion
        
    }
}