using System;
using Interfaces;
using Terrain.Textures;
using UnityEngine;

namespace App.ViewModel.Terrain
{
    [Serializable]
    public class GrayScaleModel : IViewAdapter<IGenerator<Texture2D>>
    {
        private GrayScaleStrategy _strategy;

        public IGenerator<Texture2D> Model
        {
            get => _strategy;
            set => _strategy = value as GrayScaleStrategy;
        }

        public void Display()
        {
        }
    }
}