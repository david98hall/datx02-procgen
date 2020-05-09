using System;
using Interfaces;
using UnityEngine;

namespace Textures
{
    /// <summary>
    /// Factory for creating textures. Texture generators are mainly dependent on the noise map
    /// from a terrain generator. To avoid circular dependencies for the generators, this class is
    /// dependent on an injector for noise maps.
    /// </summary>
    public class Factory
    {
        /// <summary>
        /// The dependency injection object
        /// </summary>
        private readonly IInjector<float[,]> _noiseMapInjector;

        /// <summary>
        /// constructs the factory with a new noise map injector
        /// </summary>
        /// <param name="noiseMapInjector"></param>
        public Factory(IInjector<float[,]> noiseMapInjector)
        {
            _noiseMapInjector = noiseMapInjector;
        }
        
        public Factory(Func<float[,]> injector)
        {
            _noiseMapInjector = new Injector<float[,]>(injector);
        }
        
        /// <summary>
        /// Constructs and returns a whittaker texture generation from <see cref="_noiseMapInjector"/>
        /// </summary>
        /// <param name="precipitationScale">The scale used for generating a precipitation map.</param>
        /// <param name="temperatureScale">The scale used for generating a temperature map.</param>
        /// <returns>A whittaker generator as its abstract super type.</returns>
        public IGenerator<Texture2D> CreateWhittakerStrategy(float precipitationScale = 50, float temperatureScale = 50) 
            => new WhittakerStrategy(_noiseMapInjector)
            {
                PrecipitationScale = precipitationScale,
                TemperatureScale = temperatureScale
            };

        /// <summary>
        /// Constructs and returns a grayscale texture generation from <see cref="_noiseMapInjector"/>
        /// </summary>
        /// <returns>A grayscale generator as its abstract super type</returns>
        public IGenerator<Texture2D> CreateGrayScaleStrategy() => new GrayScaleStrategy(_noiseMapInjector);
    }
}