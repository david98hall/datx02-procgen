using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates 2D textures with arbitrary strategies.
    /// See <see cref = "Interfaces.IGenerator{T}"/>.
    /// Acts as a <see cref="Interfaces.IInjector{T}"/> to avoid circular dependencies between
    /// terrain and texture generators.
    /// </summary>
    internal class TextureGenerator : IGenerator<Texture2D>, IInjector<float[,]>
    {
        /// <summary>
        /// The noise map dependency protected by the injector method
        /// </summary>
        private float[,] _noiseMap;
        
        /// <summary>
        /// Internal setter for <see cref="_noiseMap"/>
        /// </summary>
        internal float[,] NoiseMap
        {
            set => _noiseMap = value;
        }

        /// <summary>
        /// Implementation of injection methods <see cref="IInjector{T}.Get"/>
        /// </summary>
        /// <returns>
        /// A reference to <see cref="_noiseMap"/>
        /// </returns>
        public float[,] Get()
        {
            return _noiseMap;
        }

        /// <summary>
        /// The strategy of generating 2D textures.
        /// </summary>
        /// <returns>
        /// The current strategy
        /// </returns>
        internal IGenerator<Texture2D> Strategy { get; set; }
        
        /// <summary>
        /// Constructs a TextureGenerator object with a given strategy.
        /// </summary>
        /// /// <param name = "strategy"> The given strategy.</param>
        /// /// <example>
        /// Constructing a TextureGenerator object with a WhittakerStrategy:
        /// <code>
        /// TextureGenerator generator = new TextureGenerator(new WhittakerGenerator(...))
        /// </code>
        /// </example>
        internal TextureGenerator(IGenerator<Texture2D> strategy)
        {
            Strategy = strategy;
        }
        
        /// <summary>
        /// Generates a 2D textures with the current strategy.
        /// See <see cref = "Interfaces.IGenerator{T}.Generate()"/>.
        /// </summary>
        /// <returns>
        /// The generated Texture2D object
        /// </returns>
        public Texture2D Generate() => Strategy?.Generate();
    }
}