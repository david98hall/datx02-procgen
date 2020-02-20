using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Terrain
{
    public class NoiseGenerator : IGenerator<double[,]>, IStrategyzer<IGenerator<double[,]>>
    {
        public IGenerator<double[,]> Strategy { get; set; }
        
        public double[,] Generate() => Strategy.Generate();
    }
}
