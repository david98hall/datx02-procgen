using System.Collections.Generic;
using UnityEngine;
using Cities.Plots;
using Cities.Roads;
using Utils;

namespace Cities
{
    /// <summary>
    /// Represents a city with road networks, blocks and plots for buildings, et cetera.
    /// </summary>
    public struct City
    {
        /// <summary>
        /// The relative position of the city.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The network of roads in the city.
        /// </summary>
        public RoadNetwork RoadNetwork { get; internal set; }
        
        /// <summary>
        /// All plots (places to build) in the city.
        /// </summary>
        public IEnumerator<Plot> Plots => new CopyableEnumerator<Plot>(PlotsEnumerable);
        internal IEnumerable<Plot> PlotsEnumerable { private get; set; }

    }
}