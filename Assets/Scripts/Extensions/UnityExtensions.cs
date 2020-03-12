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
        
        public static Vector2 ToXZ(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }
        
        public static Vector2 ToXY(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }
        
        public static Vector2 ToZY(this Vector3 v3)
        {
            return new Vector2(v3.z, v3.y);
        }
        
    }
}