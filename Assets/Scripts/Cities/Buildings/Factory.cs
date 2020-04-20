using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cities.Plots;
using Interfaces;

namespace Cities.Buildings
{
    /// <summary>
    /// Creates strategies for generating buildings.
    /// </summary>
    public class Factory
    {

        private readonly IInjector<(float[,], IEnumerable<Plot>)> _injector;

        /// <summary>
        /// Initializes this factory with a noise map injector and plot collection injector.
        /// </summary>
        /// <param name="terrainMeshNoiseMapInjector">The noise map injector.</param>
        public Factory(IInjector<(float[,], IEnumerable<Plot>)> injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// Creates an extrusion strategy for generating buildings.
        /// </summary>
        /// <param name="minArea">The minimal area that a building can be generated in.</param>
        /// <returns>A collection of the generated buildings.</returns>
        public IGenerator<IEnumerable<Building>> CreateExtrusionStrategy(float minArea = 2, float maxArea = 100) =>
            new ExtrusionStrategy(_injector, minArea, maxArea)
            {
                
            };
    }
}

