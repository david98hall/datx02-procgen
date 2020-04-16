namespace Interfaces
{
    public abstract class Strategy<TI, TO> : IGenerator<TO>
    {
        public IInjector<TI> Injector { get; set; }

        protected Strategy(IInjector<TI> injector)
        {
            Injector = injector;
        }
        
        public abstract TO Generate();
    }
}