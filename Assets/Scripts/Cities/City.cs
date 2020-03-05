using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;

namespace Cities
{
    /// <summary>
    /// Represents a city with road networks, blocks and plots for buildings, et cetera.
    /// </summary>
    public class City
    {
        /// <summary>
        /// The relative position of the city.
        /// </summary>
        public Vector3 Position { get; }

        // Adjacency set for road network vectors
        private readonly IDictionary<Vector3, ICollection<Vector3>> _roadNetwork;
        
        /// <summary>
        /// All plots in the city.
        /// </summary>
        public IEnumerator<Plot> Plots => new CopyableEnumerator<Plot>(_plots);
        private readonly IEnumerable<Plot> _plots;

        /// <summary>
        /// Initializes the city and sets its relative position.
        /// </summary>
        /// <param name="position">The relative position of the city.</param>
        public City(Vector3 position)
        {
            Position = position;
            _roadNetwork = new Dictionary<Vector3, ICollection<Vector3>>();
            _plots = new List<Plot>();
        }

        /// <summary>
        /// Initialized the city with the relative position (0, 0, 0).
        /// </summary>
        public City() : this(Vector3.zero)
        {
        }
        
        #region Adding roads
        
        /// <summary>
        /// Creates a road by creating edges between vertices.
        /// </summary>
        /// <param name="roadVertices">The vertices of the road.</param>
        public void AddRoad(IEnumerator<Vector3> roadVertices)
        {
            var firstIteration = true;
            var previousVertex = Vector3.negativeInfinity;
            while (roadVertices.MoveNext())
            {
                // Adds the current vertex to the road network
                var currentVertex = roadVertices.Current;                
                AddRoadVertex(currentVertex);
                
                if (!firstIteration)
                {
                    // Add an edge from the previous vertex to the current one
                    _roadNetwork[previousVertex].Add(currentVertex);
                }
                else
                {
                    firstIteration = false;
                }

                // Update the previous vertex to the current one
                previousVertex = currentVertex;
            }
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
        
    }
}