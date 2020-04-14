using Interfaces;
using UnityEngine;

namespace Cities.Roads{
    internal class LSystemStrategy : Strategy<float[,], RoadNetwork>
    {

        /// <summary>
        /// The start point of the L-system road network generation. 
        /// </summary>
        internal Vector2 Origin { get; set; }
        
        int iterations;

        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector, int i = 6) 
            : base(terrainNoiseMapInjector)
        {
            iterations = i;
        }

        public override RoadNetwork Generate(){
            var system = new Lsystem('F', Origin, Injector);
            for (var i = 0; i < iterations; i++)
            {
                system.Rewrite();
            }
            return system.NoiseMappedNetwork;
        }
    }
}
