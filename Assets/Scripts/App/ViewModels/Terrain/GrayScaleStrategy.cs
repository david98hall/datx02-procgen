using System;
using Terrain.Textures;
using UnityEngine;

namespace App.ViewModels.Terrain
{
    [Serializable]
    public class GrayScaleStrategy : ViewModelStrategy<float[,], Texture2D>
    {
        private Factory _textureStrategyFactory;

        public override void Initialize() => _textureStrategyFactory = new Factory(Injector);

        public override Texture2D Generate() => _textureStrategyFactory?.CreateGrayScaleStrategy().Generate();
        
    }
}