using System;
using Extensions;
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

        public enum Texture2DType
        {
            Whittaker, GrayScale
        }

        public Texture2DType TextureType { get; set; }

        #endregion
        
        /// <summary>
        /// Initializes the terrain generator by setting its
        /// noise map generation strategy.
        /// </summary>
        /// <param name="noiseStrategy">The strategy of generating noise maps.</param>
        public TerrainGenerator(IGenerator<float[,]> noiseStrategy)
        {
            noiseGenerator = new NoiseGenerator(noiseStrategy);
            noiseMeshGenerator = new NoiseMeshGenerator();
            TextureType = Texture2DType.Whittaker;
        }
        
        /// <summary>
        /// Generates a terrain as a tuple consisting of a mesh and its texture
        /// </summary>
        /// <returns>The terrain tuple.</returns>
        public (Mesh, Texture2D) Generate()
        {
            var noiseMap = noiseGenerator.Generate();
            noiseMeshGenerator.NoiseMap = noiseMap;
            
            return (noiseMeshGenerator.Generate(), GenerateTexture(noiseMap));
        }

        private Texture2D GenerateTexture(float[,] noiseMap)
        {
            switch (TextureType)
            {
                case Texture2DType.Whittaker:
                    return new WhittakerGenerator(noiseMap, 5, 8).Generate();
                case Texture2DType.GrayScale:
                    return new GrayScaleGenerator(noiseMap).Generate();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}