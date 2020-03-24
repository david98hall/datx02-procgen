namespace Interfaces
{
    public abstract class Strategy<TI, TO> : IGenerator<TO>
    {
        protected readonly IInjector<TI> Injector;

        protected Strategy(IInjector<TI> injector)
        {
            Injector = injector;
        }
        public abstract TO Generate();
    }
}