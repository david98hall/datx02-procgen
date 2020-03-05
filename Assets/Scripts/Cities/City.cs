using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cities
{
    /// <summary>
    /// Represents a city with road networks, blocks and plots for buildings, et cetera.
    /// </summary>
    public class City
    {
        public Vector3 Position { get; }

        // Adjacency list for road network vectors
        public IDictionary<Vector3, IEnumerable<Vector3>> RoadNetwork => CopyRoadNetwork(_roadNetwork);
        private readonly IDictionary<Vector3, IEnumerable<Vector3>> _roadNetwork;
        
        public IEnumerable<Plot> Plots => _plots.Select(x => x.Copy());
        private readonly IEnumerable<Plot> _plots;
        
        public City(Vector3 position)
        {
            Position = position;
            _roadNetwork = new Dictionary<Vector3, IEnumerable<Vector3>>();
            _plots = new List<Plot>();
        }

        #region Copy Road Network

        private static IDictionary<Vector3, IEnumerable<Vector3>> CopyRoadNetwork(
            IDictionary<Vector3, IEnumerable<Vector3>> roadNetwork)
        {
            var copy = roadNetwork
                .Select(pair => 
                    new KeyValuePair<Vector3, IEnumerable<Vector3>>(
                        CopyVector(pair.Key), 
                        pair.Value.Select(CopyVector))
                );
            return (IDictionary<Vector3, IEnumerable<Vector3>>) copy;
        }

        private static Vector3 CopyVector(Vector3 vector) => new Vector3(vector.x, vector.y, vector.z);

        #endregion

    }
}