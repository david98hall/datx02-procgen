using Interfaces;
using UnityEngine;

namespace Terrain
{
    internal class TextureGenerator : IGenerator<Texture2D>
    {
        internal TextureGenerator(IGenerator<Texture2D> strategy)
        {
            Strategy = strategy;
        }
        public Texture2D Generate() => Strategy.Generate();

        internal IGenerator<Texture2D> Strategy { get; set; }
    }
}