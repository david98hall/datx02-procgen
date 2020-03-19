using Interfaces;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates terrain meshes based on noise maps generated with arbitrary strategies.
    /// </summary>
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>
    {

        #region Noise map generation fields and properties
        
        private float[,] _noiseMap;

        private readonly NoiseGenerator noiseGenerator;
        private readonly NoiseMeshGenerator noiseMeshGenerator;
        
        /// <summary>
        /// The noise map generation strategy.
        /// </summary>
        public IGenerator<float[,]> NoiseMapStrategy { set => noiseGenerator.Strategy = value;}

        /// <summary>
        /// The scale of heights when generating a terrain mesh. The larger the scale, the higher the "mountains".
        /// </summary>
        public float HeightScale
        {
            get => noiseMeshGenerator.HeightScale;
            set => noiseMeshGenerator.HeightScale = value;
        }

        /// <summary>
        /// Defines what different values in generated noise maps mean
        /// when calculating vertex heights during the mesh generation.
        /// </summary>
        public AnimationCurve HeightCurve
        {
            get => noiseMeshGenerator.HeightCurve;
            set => noiseMeshGenerator.HeightCurve = value;
        }

        #endregion

        #region Texture generation

        /// <summary>
        /// Generator for creating textures
        /// </summary>
        private readonly TextureGenerator textureGenerator;
        
        /// <summary>
        /// Strategy to set for the texture generator
        /// </summary>
        public IGenerator<Texture2D> TextureStrategy { set => textureGenerator.Strategy = value;}
        
        
        /// <summary>
        /// Factory for creating texture generators.
        /// Is required to be a instance and not static to avoid circular dependencies between
        /// this terrain generator and texture generators.
        /// </summary>
        internal readonly TextureGeneratorFactory TextureGeneratorFactory;

        #endregion
        
        /// <summary>
        /// Initializes the terrain generator by setting its generators to no strategy
        /// </summary>
        /// <param name="noiseStrategy">The strategy of generating noise maps.</param>
        public TerrainGenerator(IGenerator<float[,]> noiseStrategy)
        {
            noiseGenerator = new NoiseGenerator(noiseStrategy);
            noiseMeshGenerator = new NoiseMeshGenerator();
            textureGenerator = new TextureGenerator(null);
            TextureGeneratorFactory = new TextureGeneratorFactory(textureGenerator);
        }
        
        /// <summary>
        /// Generates a terrain as a tuple consisting of a mesh and its texture
        /// </summary>
        /// <returns>The terrain tuple.</returns>
        public (Mesh, Texture2D) Generate()
        {
            var noiseMap = noiseGenerator.Generate();
            noiseMeshGenerator.NoiseMap = noiseMap;
            textureGenerator.NoiseMap = noiseMap;
            return (noiseMeshGenerator.Generate(), textureGenerator.Generate());
        }
    }
}