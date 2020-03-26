using UnityEngine;

namespace Extensions
{
    /// <summary>
    /// Useful extensions for classes in the Unity Engine API.
    /// </summary>
    public static class UnityExtensions
    {
        
        /// <summary>
        /// Copies this animation curve and returns the result.
        /// </summary>
        /// <param name="animationCurve">The animation curve to copy.</param>
        /// <returns>A copy of the animation curve.</returns>
        public static AnimationCurve Copy(this AnimationCurve animationCurve) =>
            new AnimationCurve(animationCurve.keys)
            {
                postWrapMode = animationCurve.postWrapMode, preWrapMode = animationCurve.preWrapMode
            };
    }
}