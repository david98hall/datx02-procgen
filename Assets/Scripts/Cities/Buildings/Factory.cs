using System.Collections;
using System.Collections.Generic;
using System;
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

        private readonly IInjector<(MeshFilter, IEnumerable<Plot>)> _injector;

        /// <summary>
        /// Initializes this factory with a MeshFilter and plot collection injector.
        /// </summary>
        /// <param name="injector">The "Noise map" and Plot collection injector.</param>
        public Factory(IInjector<(MeshFilter, IEnumerable<Plot>)> injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// Initializes this factory with a MeshFilter and Plot collection injector.
        /// </summary>
        /// <param name="injector">The MeshFilter and Plot collection injector.</param>
        public Factory(Func<(MeshFilter, IEnumerable<Plot>)> injector)
        {
            _injector = new Injector(injector);
        }

        /// <summary>
        /// Creates an extrusion strategy for generating buildings.
        /// </summary>
        /// <param name="minArea">The minimal area that a building can be generated in.</param>
        /// <param name="maxArea">The maximal area that a building can be generated in.</param>
        /// <returns>A collection of the generated buildings.</returns>
        public IGenerator<IEnumerable<Building>> CreateExtrusionStrategy(float minArea = 2, float maxArea = 100) =>
            new ExtrusionStrategy(_injector, minArea, maxArea)
            {
                
            };


        /// <summary>
        /// Converts a Func with the return type (MeshFilter, IEnumerable<Plot>) to an injector of the same type
        /// </summary>
        private class Injector : IInjector<(MeshFilter, IEnumerable<Plot>)>
        {
            private readonly Func<(MeshFilter, IEnumerable<Plot>)> _injector;

            public Injector(Func<(MeshFilter, IEnumerable<Plot>)> injector)
            {
                _injector = injector;
            }

            public (MeshFilter, IEnumerable<Plot>) Get() => _injector();
        }
    }


}

