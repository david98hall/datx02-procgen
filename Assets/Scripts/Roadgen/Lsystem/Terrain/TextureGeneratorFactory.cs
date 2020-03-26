using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Factory for creating textures. Texture generators are mainly dependent on the noise map
    /// from a terrain generator. To avoid circular dependencies for the generators, this class is
    /// dependent on an injector for noise maps.
    /// </summary>
    public class TextureGeneratorFactory
    {
        /// <summary>
        /// The dependency injection object
        /// </summary>
        private readonly IInjector<float[,]> _noiseMapInjector;

        /// <summary>
        /// constructs the factory with a new noise map injector
        /// </summary>
        /// <param name="noiseMapInjector"></param>
        internal TextureGeneratorFactory(IInjector<float[,]> noiseMapInjector)
        {
            _noiseMapInjector = noiseMapInjector;
        }

        /// <summary>
        /// Constructs and returns a whittaker texture generation from <see cref="_noiseMapInjector"/>
        /// </summary>
        /// <returns>A whittaker generator as its abstract super type</returns>
        public IGenerator<Texture2D> Whittaker()
        {
            return new WhittakerGenerator(_noiseMapInjector);
        }
        
        /// <summary>
        /// Constructs and returns a grayscale texture generation from <see cref="_noiseMapInjector"/>
        /// </summary>
        /// <returns>A grayscale generator as its abstract super type</returns>
        public IGenerator<Texture2D> GrayScale()
        {
            return new GrayScaleGenerator(_noiseMapInjector);
        }
    }
}