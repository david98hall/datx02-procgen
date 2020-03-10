using System.Collections.Generic;
using Interfaces;

namespace Cities.Plots
{
    internal class PlotsGenerator : IGenerator<IEnumerable<Plot>>
    {
        internal IGenerator<IEnumerable<Plot>> Strategy { get; set; }

        public IEnumerable<Plot> Generate() => Strategy.Generate();
    }
}