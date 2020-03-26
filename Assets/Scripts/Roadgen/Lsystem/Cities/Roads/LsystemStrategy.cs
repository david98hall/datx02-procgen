using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

namespace Cities.Roads{


    class LsystemStrategy : RoadNetworkStrategy
    {
        Lsystem system;
        internal LsystemStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
            system = new Lsystem('F');
        }
        
        public override RoadNetwork Generate(){
            //system = new Lsystem(system.axiom);
            system.Rewrite();
            return system.network;
        }
    }
}
