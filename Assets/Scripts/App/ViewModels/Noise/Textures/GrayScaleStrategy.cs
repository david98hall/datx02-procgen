using System;
using Terrain.Textures;
using UnityEngine;

namespace App.ViewModels.Noise.Textures
{
    /// <summary>
    /// View-model for displaying and generating textures with the gray scale strategy.
    /// </summary>
    [Serializable]
    public class GrayScaleStrategy : ViewModelStrategy<float[,], Texture2D>
    {

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override Texture2D Generate() => new Factory(Injector).CreateGrayScaleStrategy().Generate();
    }
}