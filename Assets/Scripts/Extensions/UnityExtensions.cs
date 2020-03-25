using System;
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

        /// <summary>
        /// Gets the angle in radians from v1 to v2.
        /// </summary>
        /// <param name="v1">The vector to get the angle from.</param>
        /// <param name="v2">The vector to get the angle to.</param>
        /// <returns>The angle in radians from v1 to v2.</returns>
        public static float RadiansTo(this Vector2 v1, Vector2 v2)
        {
            return (float) (DegreesTo(v1, v2) * Math.PI / 180);
        }
        
        /// <summary>
        /// Gets the angle in degrees from v1 to v2.
        /// </summary>
        /// <param name="v1">The vector to get the angle from.</param>
        /// <param name="v2">The vector to get the angle to.</param>
        /// <returns>The angle in degrees from v1 to v2.</returns>
        public static float DegreesTo(this Vector2 v1, Vector2 v2)
        {
            var v1Rotated90 = new Vector2(-v1.y, v1.x);
            var sign = (Vector2.Dot(v1Rotated90, v2) < 0) ? -1.0f : 1.0f;
            return Vector2.Angle(v1, v2) * sign;
        }

        /// <summary>
        /// Creates and returns the underlying height map from a 3D mesh
        /// </summary>
        /// <param name="mesh">The given mesh</param>
        /// <returns>A height map of the mesh</returns>
        public static float[,] HeightMap(this Mesh mesh)
        {
            var xMax = 0;
            var zMax = 0;

            foreach (var vertex in mesh.vertices)
            {
                if (vertex.x > xMax) xMax = (int) vertex.x;
                if (vertex.z > zMax) zMax = (int) vertex.z;
            }

            var heightMap = new float[xMax + 1, zMax + 1];
            foreach (var vertex in mesh.vertices)
            {
                heightMap[(int) vertex.x, (int) vertex.z] = vertex.y;
            }

            return heightMap;
        }
    }
}