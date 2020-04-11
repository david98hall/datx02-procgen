using Interfaces;
using UnityEngine;

namespace Cities.Roads{
    internal class LSystemStrategy : Strategy<float[,], RoadNetwork>
    {

        /// <summary>
        /// The start point of the L-system road network generation. 
        /// </summary>
        internal Vector2 Origin { get; set; }

        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
        }

        public override RoadNetwork Generate(){
            var system = new Lsystem('F', Origin, Injector);
            for (var i = 0; i < 5; i++)
            {
                system.Rewrite();
            }
            return system.HeightMappedNetwork;
        }
    }
}
