using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class NoiseGenerator : IGenerator<float[,]>, IStrategyzer<IGenerator<float[,]>>
    {
        public IGenerator<float[,]> Strategy { get; set; }
        
        public float[,] Generate() => Strategy.Generate();
    }
}
