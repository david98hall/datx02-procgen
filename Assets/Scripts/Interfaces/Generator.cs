namespace Interfaces
{
    /// <summary>
    /// Generates something based on a given strategy.
    /// </summary>
    /// <typeparam name="T">The type of what can be generated.</typeparam>
    public class Generator<T> : IGenerator<T> where T : class
    {
        /// <summary>
        /// The generation strategy.
        /// </summary>
        public IGenerator<T> Strategy { get; set; }

        /// <summary>
        /// Generates something based on the set strategy.
        /// </summary>
        /// <returns>The generation result.</returns>
        public T Generate() => Strategy?.Generate();
    }
}