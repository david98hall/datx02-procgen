using Interfaces;
using Terrain;
using UnityEngine;

namespace Cities.Roads{
    
    /// <summary>
    /// A strategy which is injected with a tuple of terrain data: its offset in the scene, and its height map.
    /// Based on this data, a road network can be generated with an L-system algorithm.
    /// </summary>
    internal class LSystemStrategy : Strategy<TerrainInfo, RoadNetwork>
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

        internal LSystemStrategy(IInjector<TerrainInfo> terrainInjector, int rewritesCount = 6) : base(terrainInjector)
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