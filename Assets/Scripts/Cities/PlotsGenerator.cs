using System.Collections.Generic;
using Interfaces;

namespace Cities
{
    internal class PlotsGenerator : IGenerator<IEnumerable<Plot>>
    {
        internal IGenerator<IEnumerable<Plot>> Strategy { get; set; }

        internal PlotsGenerator(IGenerator<IEnumerable<Plot>> strategy)
        {
            Strategy = strategy;
        }
        
        public IEnumerable<Plot> Generate() => Strategy.Generate();
    }
}