using System;
using System.Collections.Generic;
using Interfaces;
using Terrain.Textures;
using UnityEngine;
using VSCodeEditor;

namespace Demo.Test
{
    [Serializable]
    public class TextureGenerator
    {
        private readonly IInjector<float[,]> _heightMapInjector;
        
        private readonly Dictionary<Strategy, IGenerator<Texture2D>> _strategies 
            = new Dictionary<Strategy, IGenerator<Texture2D>>();

        public TextureGenerator(IInjector<float[,]> injector)
        {
            _strategies[Strategy.Grayscale] = new GrayScaleStrategy(injector);
            _strategies[Strategy.Whittaker] = new Terrain.Textures.WhittakerStrategy(injector);
        }
        
        public Strategy strategy;

        public WhittakerStrategy whittakerStrategy;
        
        public IGenerator<Texture2D> GetGenerator() =>_strategies[strategy];
        
        public enum Strategy
        {
            Grayscale, Whittaker
        }
        
        [Serializable]
        public struct WhittakerStrategy
        {
            public float percipitationScale;
            public float temperatureScale;
        }
    }
}