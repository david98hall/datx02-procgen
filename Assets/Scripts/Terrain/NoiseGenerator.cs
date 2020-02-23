using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Can generate noise maps with an arbitrary set of strategies.
    /// </summary>
    public class NoiseGenerator : IGenerator<float[,]>, IStrategyzer<IGenerator<float[,]>>
    {
        /// <summary>
        /// The strategy of generating a noise map.
        /// </summary>
        public IGenerator<float[,]> Strategy { get; set; }
        
        /// <summary>
        /// Generates a noise map based on the set strategy.
        /// </summary>
        /// <returns>The generated noise map.</returns>
        public float[,] Generate() => Strategy.Generate();

        /// <summary>
        /// Initialized the generator with a strategy for noise map generation.
        /// </summary>
        /// <param name="strategy">The noise map generation strategy.</param>
        public NoiseGenerator(IGenerator<float[,]> strategy)
        {
            Strategy = strategy;
        }
        
    }
}
