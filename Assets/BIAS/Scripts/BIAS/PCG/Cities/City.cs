using System.Collections.Generic;
using System.Linq;
using BIAS.PCG.Cities.Buildings;
using BIAS.PCG.Cities.Plots;
using BIAS.PCG.Cities.Roads;
using UnityEngine;

namespace BIAS.PCG.Cities
{
    /// <summary>
    /// Represents a city with road networks, blocks and plots for buildings, et cetera.
    /// </summary>
    public class City
    {
        /// <summary>
        /// The relative position of the city.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The network of roads in the city.
        /// </summary>
        public RoadNetwork RoadNetwork { get; set; }

        /// <summary>
        /// All plots (places to build) in the city.
        /// </summary>
        public IEnumerable<Plot> Plots
        {
            get => _plots.Select(p => (Plot) p.Clone());
            set => _plots = value;
        }

        private IEnumerable<Plot> _plots;

        /// <summary>
        /// All buildings in the city.
        /// </summary>
        public IEnumerable<Building> Buildings { get; set; }

    }
}