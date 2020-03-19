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
        
        /// <summary>
        /// Clones this vector.
        /// </summary>
        /// <param name="v">The vector to clone</param>
        /// <returns>The cloned vector.</returns>
        public static Vector3 Clone(this Vector3 v) => new Vector3(v.x, v.y, v.z);
        
    }
}