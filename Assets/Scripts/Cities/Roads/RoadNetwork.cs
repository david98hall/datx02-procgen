using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using static Utils.Geometry.Maths3D;

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

            // Add roads between the rest of the vertices along the full road
            do
            {
                AddAndSplitRoads(previousVertex, roadVertices.Current);
                previousVertex = roadVertices.Current;
            } while (roadVertices.MoveNext());
            
            // At the last the vertex of the road
            AddRoadVertex(previousVertex);
        }

        // Returns true if a road from lineStart to lineEnd should be added
        private void AddAndSplitRoads(Vector3 lineStart, Vector3 lineEnd)
        {
            foreach (var (partStart, partEnd) in GetRoadParts())
            {
                // If the line segment is not even on the part line, it couldn't possibly overlap in parallel
                var onPartLine = OnLine(lineStart, partStart, partEnd) 
                                 && OnLine(lineEnd, partStart, partEnd);
                if (!onPartLine)
                    continue;
                
                var lineStartOnPart = OnLineSegment(lineStart, partStart, partEnd);
                var lineEndOnPart = OnLineSegment(lineEnd, partStart, partEnd);
                if (lineStartOnPart && lineEndOnPart)
                {
                    // The line being added is parallel to the current road part
                    // and is going to be placed on top of it. The line is going to be
                    // placed between the start and end of the already existing road part.
                    // Do nothing since the road being added is already contained.
                    return;
                }

                if (lineStartOnPart && !partStart.Equals(lineEnd))
                {
                    // The line being added is parallel to the current road part
                    // and is going to be placed on top of it. The end of the
                    // line is not on the road part but the start is.
                    
                    // Extend the road from partStart to lineEnd
                    _roadNetwork[partStart].Remove(partEnd);
                    AddAndSplitRoadsAtIntersections(partStart, lineEnd);
                    return;
                }
                
                if (lineEndOnPart && !lineStart.Equals(partEnd))
                {
                    // The line being added is parallel to the current road part
                    // and is going to be placed on top of it. The start of the
                    // line is not on the road part but the end is.
                    
                    // Extend the road from lineStart to partEnd
                    _roadNetwork[partStart].Remove(partEnd);
                    AddAndSplitRoadsAtIntersections(lineStart, partEnd);
                    return;
                }

                if (OnLineSegment(partStart, lineStart, lineEnd)
                    && OnLineSegment(partEnd, lineStart, lineEnd))
                {
                    // The line being added is parallel to the current road part
                    // and is going to be placed on top of it. The start and end of
                    // the line is not on the existing road part.
                    _roadNetwork[partStart].Remove(partEnd);
                    break;
                }
                
            }

            AddAndSplitRoadsAtIntersections(lineStart, lineEnd);
        }

        private void AddAndSplitRoadsAtIntersections(Vector3 lineStart, Vector3 lineEnd)
        {
            AddRoadVertex(lineStart);
            
            var intersections = GetIntersectionPoints(
                lineStart, lineEnd, GetRoadParts().GetEnumerator());

            var intersectionsOtherThanOnLineEndings = false;
            
            // Add all intersection points on other road parts if there are any
            foreach (var (start, intersection, end) in intersections)
            {
                if (lineStart.Equals(start) || lineStart.Equals(end) || lineEnd.Equals(start) || lineEnd.Equals(end)) 
                    continue;

                intersectionsOtherThanOnLineEndings = true;
                
                AddRoadVertex(intersection);
                
                // Remove full roads since they now intersect
                _roadNetwork[lineStart].Remove(lineEnd);
                _roadNetwork[start].Remove(end);

                // Add road from one of the start points to the intersection
                if (!_roadNetwork[intersection].Contains(start) && !intersection.Equals(start))
                    _roadNetwork[start].Add(intersection);

                // Add road from the other start point to the intersection
                if (!_roadNetwork[intersection].Contains(lineStart) && !intersection.Equals(lineStart)) 
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

                if (!lineStart.Equals(intersection))
                {
                    // Potentially split to the "left" of the intersection
                    AddAndSplitRoadsAtIntersections(lineStart, intersection);
                }
                
                if (!lineEnd.Equals(intersection))
                {
                    // Potentially split to the "right" of the intersection
                    AddAndSplitRoadsAtIntersections(intersection, lineEnd);
                }
            }
            
            if (!intersectionsOtherThanOnLineEndings)
            {
                // No intersections (except for perhaps intersections at line endings); add the road as is
                _roadNetwork[lineStart].Add(lineEnd);
            }
            
        }
        
        private static ICollection<(Vector3 start, Vector3 intersection, Vector3 end)> GetIntersectionPoints(
            Vector3 linePoint1, Vector3 linePoint2, IEnumerator<(Vector3, Vector3)> roadParts)
        {
            var intersectionPoints = new HashSet<(Vector3, Vector3, Vector3)>();
            
            while (roadParts.MoveNext())
            {
                var (partStart, partEnd) = roadParts.Current;
                
                // If the argument line intersects the road part line, register the intersection point
                if (LineSegmentIntersection(
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
            AddRoad((IEnumerable<Vector3>) roadVertices);
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
        public IEnumerable<(Vector3, Vector3)> GetRoadParts() => GetRoadParts(_roadNetwork);

        private IEnumerable<(Vector3, Vector3)> GetRoadParts(IDictionary<Vector3, ICollection<Vector3>> roadNetwork)
        {
            return roadNetwork.Keys.SelectMany(GetRoadParts);
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
                        road.AddRange(neighbourRoads?.First());
                        break;
                    
                    // At least two neighbour roads were found
                    default:
                    {
                        // Add roads found when searching from the neighbour vertex
                        var neighbourRoadsEnumerator = neighbourRoads?.GetEnumerator();
                        while (neighbourRoadsEnumerator != null && neighbourRoadsEnumerator.MoveNext())
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

        #region Vertex info

        /// <summary>
        /// Gets the adjacent vertices.
        /// </summary>
        /// <param name="vertex">The vector who's adjacent vertices will be returned.</param>
        /// <returns>The adjacent vertices of the passed vertex.</returns>
        public IEnumerable<Vector3> GetAdjacentVertices(Vector3 vertex)
        {
            return !_roadNetwork.ContainsKey(vertex) ? null : _roadNetwork[vertex].Select(v => v.Clone());
        }

        /// <summary>
        /// Gets the number of adjacent vertices.
        /// </summary>
        /// <param name="vertex">The vector who's adjacent vertices will be returned.</param>
        /// <returns>The number of adjacent vertices.</returns>
        public int GetNumberOfAdjacentVertices(Vector3 vertex)
        {
            return !_roadNetwork.ContainsKey(vertex) ? -1 : _roadNetwork[vertex].Count;
        }

        /// <summary>
        /// Returns true if v2 is adjacent to v1.
        /// </summary>
        /// <param name="v1">The vertex to check adjacency to.</param>
        /// <param name="v2">The vertex to check whether it's adjacent to v1 or not.</param>
        /// <returns>true if v2 is adjacent to v1.</returns>
        public bool IsAdjacent(Vector3 v1, Vector3 v2)
        {
            return _roadNetwork.ContainsKey(v1) && _roadNetwork[v1].Contains(v2);
        }

        #endregion
        
        #region Conversion

        /// <summary>
        /// Convert this road network into an undirected graph.
        /// </summary>
        /// <returns>The undirected graph.</returns>
        public IDictionary<Vector3, ICollection<Vector3>> ConvertToUndirectedGraph()
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
        
        /// <summary>
        /// Creates an undirected clone of this road network.
        /// </summary>
        /// <returns>The undirected road network.</returns>
        public RoadNetwork GetAsUndirected() => new RoadNetwork(ConvertToUndirectedGraph());

        /// <summary>
        /// Projects this road network to the xz plane at a specified y-value.
        /// </summary>
        /// <param name="y">The y-value of the plane to project to.</param>
        /// <returns>The projected road network.</returns>
        public RoadNetwork GetXZProjection(float y = 0)
        {
            var projectionNetwork = new RoadNetwork();
            foreach (var (start, end) in GetRoadParts())
            {
                projectionNetwork.AddRoad(new Vector3(start.x, y, start.z), new Vector3(end.x, y, end.z));
            }
            return projectionNetwork;
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
                    valuesCopy.Add(value.Clone());
                }
                
                copy.Add(key.Clone(), valuesCopy);
            }
            
            return copy;
        }

        #endregion
        
    }
}