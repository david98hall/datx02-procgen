using Interfaces;

namespace Cities.Roads{
    internal class LSystemStrategy : Strategy<float[,], RoadNetwork>
    {
        public LSystem system;
        int iterations;
        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector) : base(terrainNoiseMapInjector)
        {
            system = new LSystem('F');
            iterations = 3;
        }

        internal LSystemStrategy(IInjector<float[,]> terrainNoiseMapInjector, int i) : base(terrainNoiseMapInjector)
        {
            system = new LSystem('F');
            iterations = i;
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
