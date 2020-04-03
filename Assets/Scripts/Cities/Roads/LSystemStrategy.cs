using Interfaces;

namespace Cities.Roads{
    internal class LSystemStrategy : Strategy<float[,], RoadNetwork>
    {
        bool start;
        LSystem system;
        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
            system = new LSystem('F');
            start = true;
        }
        
        public override RoadNetwork Generate(){
            system = new LSystem(system.axiom);
            for (var i = 0; i < 3; i++)
            {
                system.Rewrite();
            }
            return system.network;
        }
        
    }
}
