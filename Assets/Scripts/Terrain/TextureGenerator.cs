using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class TextureGenerator : IGenerator<Texture2D>, IStrategyzer<IGenerator<Texture2D>>
    {
        public TextureGenerator(IGenerator<Texture2D> strategy)
        {
            Strategy = strategy;
        }
        public Texture2D Generate() => Strategy.Generate();

        public IGenerator<Texture2D> Strategy { get; set; }
    }
}