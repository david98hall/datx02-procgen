using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cities.Plots;
using Interfaces;

namespace Cities.Buildings
{
    /// <summary>
    /// Creates strategies for generating plot contents.
    /// </summary>
    public class Factory
    {
        private readonly IInjector<IEnumerable<Plot>> _plotsInjector;
        private readonly IInjector<float[,]> _heightMapInjector;

        /// <summary>
        /// Creates a Factory with an injector for Plots and a height map.
        /// </summary>
        /// <param name="plotsInjector">Provides the plots.</param>
        /// <param name="heightMapInjector">Provides the height map.</param>
        public Factory(IInjector<IEnumerable<Plot>> plotsInjector, IInjector<float[,]> heightMapInjector)
        {
            _plotsInjector = plotsInjector;
            _heightMapInjector = heightMapInjector;
        }

        /// <summary>
        /// Creates a strategy for constructing buildings using the extrusion method.
        /// </summary>
        /// <returns>The generated buildings.</returns>
        public IGenerator<IEnumerable<Plot>> CreateExtrusionStrategy()
        {
            return new ExtrusionStrategy(_plotsInjector, _heightMapInjector);
        }
    }
    
}

