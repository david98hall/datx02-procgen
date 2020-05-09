using System;

namespace BIAS.Utils.Interfaces
{
    public class Injector<T> : IInjector<T>
    {
        private readonly Func<T> _injector;

        public Injector(Func<T> injector) => _injector = injector;
        
        public T Get() => _injector();
    }
}