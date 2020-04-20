using Interfaces;
using UnityEngine;

namespace Cities.Roads{
    internal class LSystemStrategy : Strategy<float[,], RoadNetwork>
    {

        /// <summary>
        /// The start point of the L-system road network generation. 
        /// </summary>
        internal Vector2 Origin { get; set; }
        
        /// <summary>
        /// The number of times this strategy will rewrite the
        /// L-system before returning a road network based on it.
        /// </summary>
        internal int RewritesCount { get; set; }

        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector, int rewritesCount = 6) 
            : base(terrainNoiseMapInjector)
        {
            RewritesCount = rewritesCount;
        }

        public override RoadNetwork Generate(){
            var system = new LSystem('F', Origin, Injector);
            for (var i = 0; i < RewritesCount; i++)
            {
                system.Rewrite();
            }
            return system.network;
        }
    }
}
