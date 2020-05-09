using System;
using BIAS.Utils.Parallelism;
using Random = UnityEngine.Random;

namespace BIAS.Utils.Maths
{
    /// <summary>
    /// Mathematics utility methods.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Returns a random float in the range [lower, upper].
        /// </summary>
        /// <param name="lower">Inclusive lower bound.</param>
        /// <param name="upper">Inclusive upper bound.</param>
        /// <returns>A random float.</returns>
        public static float RandomInclusiveFloat(float lower, float upper)
        {
            // Dispatched since UnityEngine.Random.Range can only be called on Unity's main thread
            return Dispatcher.Instance.EnqueueFunction(() => Random.Range(lower, upper));
        }
        
        /// <summary>
        /// Returns a random float in the range [lower, upper).
        /// </summary>
        /// <param name="lower">Inclusive lower bound.</param>
        /// <param name="upper">Exclusive upper bound.</param>
        /// <returns>A random float.</returns>
        public static float RandomHalfClosedFloat(float lower, float upper)
        {
            if (lower > upper)
                throw new ArgumentException("The lower bound cannot be greater than the upper bound!");
            return (float) (lower + new System.Random().NextDouble() * (upper - lower));
        }
        
    }
}