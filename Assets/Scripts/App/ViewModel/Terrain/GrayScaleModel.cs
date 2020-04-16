using System;
using Interfaces;
using Terrain.Textures;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class GrayScaleModel : EditorStrategyView<float[,], Texture2D>
    {
        private Factory _textureStrategyFactory;

        public override void Initialize()
        {
            _textureStrategyFactory = new Factory(Injector);
        }
        
        public override void Display()
        {
        }

        public override Texture2D Generate()
        {
            return _textureStrategyFactory?.CreateGrayScaleStrategy().Generate();
        }
        
    }
}