using System.Collections.Generic;
using System;
using BIAS.PCG.Cities.Plots;
using BIAS.Utils.Interfaces;
using BIAS.PCG.Terrain;

namespace BIAS.PCG.Cities.Buildings
{
    /// <summary>
    /// Creates strategies for generating buildings.
    /// </summary>
    public class Factory
    {

        private readonly IInjector<(TerrainInfo, IEnumerable<Plot>)> _injector;

        /// <summary>
        /// Initializes this factory with a MeshFilter and plot collection injector.
        /// </summary>
        /// <param name="injector">The "Noise map" and Plot collection injector.</param>
        public Factory(IInjector<(TerrainInfo, IEnumerable<Plot>)> injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// Initializes this factory with a TerrainInfo and Plot collection injector.
        /// </summary>
        /// <param name="injector">The MeshFilter and Plot collection injector.</param>
        public Factory(Func<(TerrainInfo, IEnumerable<Plot>)> injector)
        {
            _injector = new Injector<(TerrainInfo, IEnumerable<Plot>)>(injector);
        }

        /// <summary>
        /// Creates an extrusion strategy for generating buildings.
        /// </summary>
        /// <param name="minArea">The minimal area that a building can be generated in.</param>
        /// <param name="maxArea">The maximal area that a building can be generated in.</param>
        /// <returns>A collection of the generated buildings.</returns>
        public IGenerator<IEnumerable<Building>> CreateExtrusionStrategy(float minArea = 2, float maxArea = 100) =>
            new ExtrusionStrategy(_injector, minArea, maxArea);
    }


}

