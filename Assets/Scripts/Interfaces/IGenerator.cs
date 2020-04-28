using System.Threading;

namespace Interfaces
{
    /// <summary>
    /// Can generate things.
    /// </summary>
    /// <typeparam name="T">The type of what can be generated.</typeparam>
    public interface IGenerator<out T>
    {
        /// <summary>
        /// Used to cancel the generation of this object.
        /// </summary>
        CancellationToken CancelToken { get; set; }
        
        /// <summary>
        /// Generates things.
        /// </summary>
        /// <returns>What has been generated.</returns>
        T Generate();
    }
}