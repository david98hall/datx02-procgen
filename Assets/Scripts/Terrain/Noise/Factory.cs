using Interfaces;

namespace Terrain.Noise
{
    /// <summary>
    /// Creates different types of noise map generation strategies.
    /// </summary>
    public class Factory
    {
        /// <summary>
        /// Instantiates a perlin noise strategy for generating noise maps.
        /// </summary>
        /// <returns></returns>
        public IGenerator<float[,]> CreatePerlinNoiseStrategy() => new PerlinNoiseStrategy();

    }
}