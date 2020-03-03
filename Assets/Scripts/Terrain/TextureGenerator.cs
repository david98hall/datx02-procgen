using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates 2D textures with arbitrary strategies.
    /// See <see cref = "Interfaces.IGenerator{T}"/>.
    /// </summary>
    internal class TextureGenerator : IGenerator<Texture2D>
    {
        
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
        public Texture2D Generate() => Strategy == null ? Texture2D.redTexture : Strategy.Generate();
    }
}