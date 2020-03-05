namespace Interfaces
{
    /// <summary>
    /// Can be copied.
    /// </summary>
    public interface ICopyable<T>
    {
        /// <summary>
        /// Copies the given object.
        /// </summary>
        /// <returns>The copy.</returns>
        T Copy();
    }
}