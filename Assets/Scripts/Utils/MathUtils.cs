
using System;

namespace Utils
{
    /// <summary>
    /// Mathematics utility methods.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Returns a random float in the given range.
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static float RandomFloatInRange(float lower, float upper)
        {
            if (lower > upper)
                throw new ArgumentException("The lower bound cannot be greater than the upper bound!");
            return (float) (lower + new Random().NextDouble() * (upper - lower));
        }
    }
}