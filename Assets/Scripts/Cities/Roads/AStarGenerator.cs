using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Cities.Roads
{
    internal class AStarGenerator : IGenerator<RoadNetwork>
    {

        #region Properties and constructors
        
        private readonly IInjector<float[,]> _heightMapInjector;
        
        private readonly Dictionary<(int, int), ISet<(int, int)>> _paths;
        internal float Beta { get; set; }

        internal AStarGenerator([NotNull] IInjector<float[,]> heightMapInjector) 
        {
            _heightMapInjector = heightMapInjector;
            _paths = new Dictionary<(int, int), ISet<(int, int)>>();
        }
        
        #endregion
        
        #region Public and internal methods
        public RoadNetwork Generate()
        {
            var heights = _heightMapInjector.Get();
            var roadNetwork = new RoadNetwork();
            
            foreach (var (xStart, zStart) in _paths.Keys)
            {
                var start = new Node(xStart, heights[xStart, zStart], zStart);
                foreach (var (xGoal, zGoal) in _paths[(xStart, zStart)])
                {
                    var goal = new Node(xGoal, heights[xGoal, zGoal], zGoal);
                    roadNetwork.AddRoad(Path(start, goal, heights));

                    Debug.Log("Start: " + start + ", Goal: " + goal);
                }
            }

            return roadNetwork;
        }
        
        public void Add((int, int) start, (int, int) goal)
        {
            if (!_paths.ContainsKey(start)) _paths[start] = new HashSet<(int, int)>();
            _paths[start].Add(goal);
        }
        
        public void Clear() => _paths.Clear();

        #endregion

        #region Private methods

        private IEnumerable<Vector3> Path(Node start, Node goal, float[,] heights)
        {
            var queue = new SortedSet<Node>(Comparer<Node>.Create((node1, node2) => node1.CompareTo(node2)));
            var visited = new HashSet<Node>();
            
            Node node;
            for (node = start; !node.Equals(goal) && queue.Any(); node = queue.First())
            {
                queue.Remove(node);
                if (!visited.Add(node)) continue;
                queue.AddRange(node.Neighbors(heights, visited, goal, Beta));
                Debug.Log("Current: " + node);
            }

            return node.Path();
        }
        
        #endregion

        #region Node class
        private class Node : IComparable
        {
            #region Properties and constructors

            private readonly Vector3 _location;
            private readonly float _cost;
            private readonly Node _predecessor;
            
            internal Node(int x, float y, int z)
            {
                _location = new Vector3(x, y, z);
            }
            private Node(int x, float y, int z, Node predecessor, Node goal, float beta) : this(x, y, z)
            {
                _cost = Cost(predecessor, beta) + Cost(goal, beta);
                _predecessor = predecessor;
            }
            
            #endregion
            
            #region Public and internal methods
            public int CompareTo(object other) => ReferenceEquals(this, other) || 
                                                  other is Node node && _cost.CompareTo(node._cost) >= 0 ? 0 : -1;
            
            public override bool Equals(object other) => ReferenceEquals(this, other) || 
                                                         other is Node node && _location.Equals(node._location);
            
            public override int GetHashCode() => _location.GetHashCode();

            internal IEnumerable<Node> Neighbors(float[,] heights, ICollection<Node> visited, Node goal, float beta)
            {
                var xMin = (int) Math.Max(_location.x - 1, 0);
                var xMax = (int) Math.Min(_location.x + 1, heights.GetLength(0));
                var zMin = (int) Math.Max(_location.z - 1, 0);
                var zMax = (int) Math.Min(_location.x + 1, heights.GetLength(1));

                for (var x = xMin; x < xMax; x++)
                {
                    for (var z = zMin; z < zMax; z++)
                    {
                        if (x == (int) _location.x && z == (int) _location.z) continue;
                        var node = new Node(x, heights[x, z], z, this, goal, beta);
                        if (visited.Contains(node) || _predecessor.Equals(node)) continue;
                        yield return node;
                    }
                }
            }

            public override string ToString()
            {
                return "Node: {path: " + _location + 
                       (_predecessor == null ? "" : " to " + _predecessor._location) + 
                       ", cost :" + _cost + "} \n";
            }

            internal IEnumerable<Vector3> Path()
            {
                for (var node = this; node != null; node = node._predecessor)
                {
                    yield return node._location;
                }
            }
            
            #endregion

            #region Private methods
            
            private float Cost(Node node, float beta)
            {
                var distance = Vector2.Distance(new Vector2(_location.x, _location.z), 
                    new Vector2(node._location.x, node._location.z));
                return distance + (1 - Math.Abs(_location.z - _location.z)) * beta;
            }
            
            #endregion
        }
        
        #endregion
    }
}