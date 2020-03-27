using Interfaces;
using Terrain.Textures;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// Generates terrain meshes based on noise maps generated with arbitrary strategies.
    /// </summary>
    public class TerrainGenerator : IGenerator<(Mesh, Texture2D)>, IInjector<float[,]>
    {
        
        #region Noise map generation fields and properties

        private float[,] _noiseMap;
        
        private readonly Generator<float[,]> _noiseGenerator;
        private readonly NoiseMeshGenerator _noiseMeshGenerator;
        
        /// <summary>
        /// The noise map generation strategy.
        /// </summary>
        public IGenerator<float[,]> NoiseStrategy { set => _noiseGenerator.Strategy = value;}

        /// <summary>
        /// The scale of heights when generating a terrain mesh. The larger the scale, the higher the "mountains".
        /// </summary>
        public float HeightScale
        {
            get => _noiseMeshGenerator.HeightScale;
            set => _noiseMeshGenerator.HeightScale = value;
        }

        /// <summary>
        /// Defines what different values in generated noise maps mean
        /// when calculating vertex heights during the mesh generation.
        /// </summary>
        public AnimationCurve HeightCurve
        {
            get => _noiseMeshGenerator.HeightCurve;
            set => _noiseMeshGenerator.HeightCurve = value;
        }

        #endregion

        #region Texture generation

        /// <summary>
        /// Generator for creating textures
        /// </summary>
        //private readonly TextureGenerator textureGenerator;
        
        private readonly Generator<Texture2D> _textureGenerator;
        
        /// <summary>
        /// Strategy to set for the texture generator
        /// </summary>
        public IGenerator<Texture2D> TextureStrategy { set => _textureGenerator.Strategy = value;}
        
        
        /// <summary>
        /// Factory for creating texture generators.
        /// Is required to be a instance and not static to avoid circular dependencies between
        /// this terrain generator and texture generators.
        /// </summary>
        public readonly Textures.Factory TextureStrategyFactory;

        #endregion
        
        /// <summary>
        /// Initializes the terrain generator by setting its generators to no strategy
        /// </summary>
        public TerrainGenerator()
        {
            _noiseGenerator = new Generator<float[,]>();
            _noiseMeshGenerator = new NoiseMeshGenerator(this);
            _textureGenerator = new Generator<Texture2D>();
            TextureStrategyFactory = new Textures.Factory(this);
        }
        
        /// <summary>
        /// Generates a terrain as a tuple consisting of a mesh and its texture
        /// </summary>
        /// <returns>The terrain tuple.</returns>
        public (Mesh, Texture2D) Generate()
        {
            _noiseMap = _noiseGenerator.Generate();
            return (_noiseMeshGenerator.Generate(), _textureGenerator.Generate());
        }
        
        public float[,] Get() => (float[,])_noiseMap.Clone();
    }
}