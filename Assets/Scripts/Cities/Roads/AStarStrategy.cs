using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Cities.Roads
{
    /// <summary>
    /// Generates the optimal roads between a set of start and goal nodes with a given height bias.
    /// Extends the <see cref="Strategy{TI,TO}"/> class.
    /// </summary>
    internal class AStarStrategy : Strategy<float[,], RoadNetwork>
    {

        #region Properties and constructors

        /// <summary>
        /// A dictionary of the the start and goal nodes.
        /// Start nodes are given as keys.
        /// Goal nodes from a given start node is given as a set of nodes from the start key.
        /// Is used in <see cref="Generate"/> to find the optimal path between each pair of start and goal nodes.
        /// Each pair of start and goal nodes is implicitly used in <see cref="Path"/> to found the optimal path
        /// between the given pair of nodes.
        /// Since the height map in <see cref="Strategy{TI,TO}.Injector"/> may be changed externally, the nodes in the
        /// dictionary may be invalid. 
        /// </summary>
        private readonly Dictionary<Vector2Int, ISet<Vector2Int>> _paths;

        /// <summary>
        /// The height bias for finding the optimal path.
        /// Is used by <see cref="Node"/> in <see cref="Node.Cost"/> to calculate the cost of the given node.
        /// A greater value implies that height impacts the nodes cost more.
        /// When zero, the height has no impact.
        /// </summary>
        private float _heightBias;

        /// <summary>
        /// Internal getter and setter for <see cref="_heightBias"/>
        /// Assures that the height bias is between zero and one.
        /// </summary>
        internal float HeightBias
        {
            set => _heightBias = Math.Max(0, Math.Min(1, value)); 
        }

        /// <summary>
        /// Constructs a new A* Generator with a given height map injector and an empty set of start and goal nodes.
        /// </summary>
        /// <param name="terrainMeshInjector">Non null terrain mesh filter injector object.</param>
        internal AStarStrategy([NotNull] IInjector<float[,]> terrainMeshInjector) : base(terrainMeshInjector)
        {
            _paths = new Dictionary<Vector2Int, ISet<Vector2Int>>();
        }

        #endregion
        
        #region Public and internal methods
        
        /// <summary>
        /// Implementation of <see cref = "Interfaces.IGenerator{T}.Generate()"/>.
        /// Generates a road network with the optimal roads between each pair of start and goal nodes in the
        /// height map from <see cref="Strategy{TI,TO}.Injector"/>.
        /// Iterates each pair of nodes in <see cref="_paths"/> and calculates the optimal path with <see cref="Path"/>.
        /// </summary>
        /// <returns>The generated <see cref="RoadNetwork"/> object.</returns>
        public override RoadNetwork Generate()
        {
            var heights = Injector.Get();
            var roadNetwork = new RoadNetwork();
            foreach (var startVector in _paths.Keys)
            {
                // Cancel if requested
                if (CancelToken.IsCancellationRequested) return null;
                
                if (!(0 <= startVector.x && startVector.x < heights.GetLength(0) && 0 <= startVector.y &&
                    startVector.y < heights.GetLength(1)))
                {
                    continue;
                }
                
                var start = new Node(startVector.x, heights[startVector.x, startVector.y], startVector.y);
                foreach (var goalVector in _paths[startVector])
                {
                    // Cancel if requested
                    if (CancelToken.IsCancellationRequested) return null;
                    
                    if (!(0 <= goalVector.x && goalVector.x < heights.GetLength(0) && 0 <= goalVector.y &&
                          goalVector.y < heights.GetLength(1)))
                    {
                        continue;
                    }
                    var goal = new Node(goalVector.x, heights[goalVector.x, goalVector.y], goalVector.y);
                    roadNetwork.AddRoad(Path(start, goal, heights));
                }
            }

            return roadNetwork;
        }
        
        /// <summary>
        /// Adds a new pair of start and goal nodes to <see cref="_paths"/>.
        /// If the start node does not already exist, a empty set of goal nodes from that node is added.
        /// The validity of the start and goal node is not checked, since the height map in
        /// <see cref="Strategy{TI,TO}.Injector"/> may be changed externally.
        /// </summary>
        /// <param name="start">The given start node.</param>
        /// <param name="goal">The given goal node.</param>
        internal void Add(Vector2Int start, Vector2Int goal)
        {
            if (!_paths.ContainsKey(start)) _paths[start] = new HashSet<Vector2Int>();
            _paths[start].Add(goal);
        }
        
        /// <summary>
        /// Clears the start and goal nodes in <see cref="_paths"/>.
        /// </summary>
        public void Clear() => _paths.Clear();

        #endregion

        #region Private methods
        
        /// <summary>
        /// Find the optimal path by using the A* search algorithm between a start and goal node given
        /// as <see cref="Node"/> objects.
        /// The optimal path is defined as the cheapest path in terms the cost between the start and goal node.
        /// The cost is defined as the sum of the cost of reaching the node, <see cref="Node._cost"/>, and the
        /// estimated cost to the goal from the node, <see cref="Node._heuristic"/>, as given by the A* algorithm.
        /// The path with the lowest cost is found by using a <see cref="SortedSet{T}"/> object with a custom
        /// <see cref="IComparable{T}"/> object defined with <see cref="Node.CompareTo"/>.
        /// The search is expanded by using <see cref="Node.Adjacent"/> of the currently cheapest node.
        /// </summary>
        /// <param name="start">The given start node</param>
        /// <param name="goal">The given goal node</param>
        /// <param name="heights">The height map from <see cref="Strategy{TI,TO}.Injector"/></param>
        /// <returns>An enumerable object of the location of the nodes is the path, given as vectors.</returns>
        private IEnumerable<Vector3> Path(Node start, Node goal, float[,] heights)
        {
            // define a sorted set with custom sort order
            var queue = new SortedSet<Node>(Comparer<Node>.Create((node1, node2) => node1.CompareTo(node2)));
            
            // store the already visited nodes
            var visited = new HashSet<Node>();

            queue.Add(start);
            Node node;
            
            // iterates the adjacent nodes of the cheapest node until a goal is found
            for (node = queue.First(); !goal.Equals(node) && queue.Any(); node = queue.First())
            {
                queue.Remove(node);
                if (!visited.Add(node)) continue;
                queue.AddRange(node.Adjacent(heights, visited, goal, _heightBias));
            }

            // return the reversed path from the start node to the goal.
            return node.Path();
        }
        
        #endregion
        
        #region Node class
        
        /// <summary>
        /// Private class representing a node in a path.
        /// Is used by <see cref="Path"/> for finding the optimal node between two points.
        /// Implements the <see cref="IComparable{T}"/> interface for a custom sorting order in sorted set.
        /// </summary>
        private class Node : IComparable<Node>
        {
            #region Properties and constructors

            /// <summary>
            /// 3D location of the node in the height map from <see cref="Strategy{TI,TO}.Injector"/>.
            /// x and z values are defined as the indices in the height map, while the y is defined as the value
            /// of the indices in the height map.
            /// </summary>
            private readonly Vector3 _location;
            
            /// <summary>
            /// The previous node in the path to this node.
            /// </summary>
            private readonly Node _predecessor;

            /// <summary>
            /// The accumulated cost from the previous node to this node.
            /// Is calculated with <see cref="Cost"/>
            /// </summary>
            private readonly float _cost;
            
            /// <summary>
            /// Estimated cost between the node and the goal node.
            /// Is calculated with <see cref="Cost"/>
            /// </summary>
            private readonly float _heuristic;
            
            /// <summary>
            /// Constructs a node object of a given location with no cost.
            /// Is used by <see cref="AStarStrategy.Generate"/> to construct the start and goal nodes.
            /// </summary>
            /// <param name="x">x coordinate.</param>
            /// <param name="y">y coordinate.</param>
            /// <param name="z">x coordinate.</param>
            internal Node(int x, float y, int z)
            {
                _location = new Vector3(x, y, z);
            }
            
            /// <summary>
            /// Constructs a node object if a given location and calculates the cost and heuristics from given
            /// predecessor and goal nodes with a given height bias using <see cref="Cost"/>.
            /// Is only used by <see cref="Adjacent"/> to construct the adjacent nodes of the current node.
            /// </summary>
            /// <param name="x">x coordinate.</param>
            /// <param name="y">y coordinate.</param>
            /// <param name="z">x coordinate.</param>
            /// <param name="predecessor">Previous node in the path.</param>
            /// <param name="goal">Goal node of the path.</param>
            /// <param name="heightBias">The bias of height. Is used by <see cref="Cost"/> to compute the cost
            /// between two nodes. </param>
            private Node(int x, float y, int z, Node predecessor, Node goal, float heightBias) : this(x, y, z)
            {
                _predecessor = predecessor;
                _cost = Cost(predecessor, heightBias);
                _heuristic = Cost(goal, 0);
            }
            
            #endregion
            
            #region Public and internal methods
            
            /// <summary>
            /// Computes adjacent nodes of the current node from a given height map and returns
            /// the not already visited nodes.
            /// </summary>
            /// <param name="heights">Height map of which the nodes are located in.</param>
            /// <param name="visited">Set of the already visited node.</param>
            /// <param name="goal">The goal node. Is used by the constructor and implicitly by <see cref="Cost"/> to
            /// compute the heuristics of the node.</param>
            /// <param name="heightBias">Is implicitly used by <see cref="Cost"/> to compute the cost
            /// between two nodes. </param>
            /// <returns></returns>
            internal IEnumerable<Node> Adjacent(float[,] heights, ICollection<Node> visited, Node goal, float heightBias)
            {
                // find valid min and max indices
                var xMin = (int) Math.Max(_location.x - 1, 0);
                var xMax = (int) Math.Min(_location.x + 1, heights.GetLength(0) - 1);
                var zMin = (int) Math.Max(_location.z - 1, 0);
                var zMax = (int) Math.Min(_location.z + 1, heights.GetLength(1) - 1);
                
                // iterate adjacent nodes
                // yield if not visited
                for (var x = xMin; x <= xMax; x++)
                {
                    for (var z = zMin; z <= zMax; z++)
                    {
                        // skip node if it is equal to this node
                        if (x == (int) _location.x && z == (int) _location.z) continue;
                        
                        var node = new Node(x, heights[x, z], z, this, goal, heightBias);
                        
                        // skip node if it is already visited or if it is the previous node in the path
                        if (visited.Contains(node) || node.Equals(_predecessor)) continue;
                        yield return node;
                    }
                }
            }

            /// <summary>
            /// Iterate the path from the current backwards to the starting node.
            /// </summary>
            /// <returns>An iterable object of the reversed path.</returns>
            internal IEnumerable<Vector3> Path()
            {
                for (var node = this; node != null; node = node._predecessor)
                {
                    yield return node._location;
                }
            }

            /// <summary>
            /// Custom <see cref="IComparable{T}.CompareTo"/> method for defining the sorting order between nodes.
            /// Sorting order is defined by the lowest sum of <see cref="_cost"/> and <see cref="_heuristic"/>.
            /// Equality is defined as equal locations, to avoid duplicate nodes with the same location in a
            /// <see cref="SortedSet{T}"/>.
            /// Equal costs is defined as one being lower, to allow nodes with equal cost in a
            /// <see cref="SortedSet{T}"/>.
            /// </summary>
            /// <param name="other">The node to compare with.</param>
            /// <returns>The value of the comparison defined by <see cref="IComparable{T}"/></returns>
            public int CompareTo(Node other)
            {
                if (Equals(other)) return 0;
                var cost = (_cost + _heuristic).CompareTo(other._cost + other._heuristic);
                return cost == 0 ? -1 : cost;
            }

            /// <summary>
            /// Redefine equality as equal locations.
            /// </summary>
            /// <param name="other"></param>
            /// <returns>True if the location of the objects are equal, else false.</returns>
            public override bool Equals(object other) => ReferenceEquals(this, other) || 
                                                         other is Node node && _location == node._location;

            /// <summary>
            /// Redefine hashcode as the hashcode of only the location
            /// </summary>
            /// <returns>The hashcode of the location</returns>
            public override int GetHashCode() => _location.GetHashCode();
            
            /// <summary>
            /// Redefine the conversion to a string to return the location, location of predecessor and the cost used
            /// in <see cref="CompareTo"/>.
            /// </summary>
            /// <returns>A string of the node.</returns>
            public override string ToString() => "Node: {path: " + _location + 
                                                 (_predecessor == null ? "" : " from " + _predecessor._location) + 
                                                 ", cost :" + (_cost + _heuristic) + "} \n";

            #endregion

            #region Private methods
            
            /// <summary>
            /// Calculates cost between two nodes.
            /// The cost is defined as the sum of the 3D distance between the node and the 3D distance times the height
            /// difference between the nodes.
            /// A height bias defined by <see cref="AStarStrategy._heightBias"/> determines the impacts of the
            /// height difference.
            /// When the height bias is zero, the height has no impact.
            /// </summary>
            /// <param name="other">The other node.</param>
            /// <param name="heightBias">The bias of height.</param>
            /// <returns></returns>
            private float Cost(Node other, float heightBias)
            {
                return Vector3.Distance(_location, other._location) * 
                       (1 + heightBias * Math.Abs(_location.y - other._location.y));
            }
            
            #endregion
        }
        
        #endregion
    }
}