using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

namespace Cities.Roads{


    class LsystemStrategy : RoadNetworkStrategy
    {
        bool start;
        Lsystem system;
        internal LsystemStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
            system = new Lsystem('F');
            start = true;

        }
        
        public override RoadNetwork Generate(){
            if(start)
                system = new Lsystem('F');
            /*
            system = new Lsystem(system.axiom);
            for (int i = 0; i < 3; i++)
            {
                system.Rewrite();
            }
            */
            Debug.Log(system.ToString());
            system.Rewrite();
            start = false;
            //int vr;
            return system.network;
        }
        
    }
}
