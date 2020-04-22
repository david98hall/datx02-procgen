namespace Interfaces
{
    /// <summary>
    /// A strategy that uses an injected input to generate an output.
    /// </summary>
    /// <typeparam name="TI">The input type</typeparam>
    /// <typeparam name="TO">The output type.</typeparam>
    public abstract class Strategy<TI, TO> : IGenerator<TO>
    {
        /// <summary>
        /// The injector of the input.
        /// </summary>
        protected IInjector<TI> Injector;

        /// <summary>
        /// Initializes the strategy by setting the input injector.
        /// </summary>
        /// <param name="injector">The input injector</param>
        protected Strategy(IInjector<TI> injector)
        {
            Injector = injector;
        }
        
        /// <summary>
        /// Generates something based on the injected input.
        /// </summary>
        /// <returns>The generated output.</returns>
        public abstract TO Generate();
    }
}