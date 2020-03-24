namespace Interfaces
{
    /// <summary>
    /// Class used as dependency injection to avoid circular dependencies.
    /// </summary>
    /// <typeparam name="T">The protected dependency type.</typeparam>
    public interface IInjector<out T>
    {
        /// <summary>
        /// Returns the protected object
        /// </summary>
        /// <returns>The protected object</returns>
        T Get();
    }
}