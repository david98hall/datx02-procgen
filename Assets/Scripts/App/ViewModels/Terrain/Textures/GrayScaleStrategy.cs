using System;
using Textures;
using UnityEngine;

namespace App.ViewModels.Terrain.Textures
{
    /// <summary>
    /// View-model for displaying and generating textures with the gray scale strategy.
    /// </summary>
    [Serializable]
    public class GrayScaleStrategy : ViewModelStrategy<float[,], Texture2D>
    {
        public override void Display()
        {
            // Nothing to display 
        }

        /// <summary>
        /// Creates a generator with the serialized values from the editor.
        /// Delegates the generation to the created generator.
        /// </summary>
        /// <returns>The result of the delegated generation call.</returns>
        public override Texture2D Generate()
        {
            var generator = new Factory(Injector).CreateGrayScaleStrategy();
            // Set the cancellation token so that the generation can be canceled
            generator.CancelToken = CancelToken;
            return generator.Generate();
        }
    }
}