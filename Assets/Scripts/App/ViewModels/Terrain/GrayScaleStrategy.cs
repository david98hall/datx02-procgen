using System;
using Terrain.Textures;
using UnityEngine;

namespace App.ViewModels.Terrain
{
    /// <summary>
    /// View-model for displaying and generating textures with the gray scale strategy.
    /// </summary>
    [Serializable]
    public class GrayScaleStrategy : ViewModelStrategy<float[,], Texture2D>
    {
        /// <summary>
        /// Underlying <see cref="Factory"/> for creating the gray scale  strategy object
        /// </summary>
        private Factory _textureStrategyFactory;

        /// <summary>
        /// Is required for initializing the non-serializable properties of the view model.
        /// </summary>
        public override void Initialize() => _textureStrategyFactory = new Factory(Injector);

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override Texture2D Generate() => _textureStrategyFactory?.CreateGrayScaleStrategy().Generate();
    }
}