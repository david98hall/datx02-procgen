using System;
using UnityEngine;

namespace BIAS.Utils.Extensions
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
        /// Returns true if the vectors are equal depending on the tolerance.
        /// </summary>
        /// <param name="v1">The first vector to compare.</param>
        /// <param name="v2">The second vector to compare.</param>
        /// <param name="tolerance">The tolerance for equality.</param>
        /// <returns>true if the vectors are equal depending on the tolerance.</returns>
        public static bool EqualWithTolerance(this Vector3 v1, Vector3 v2, float tolerance)
        {
            return Math.Abs(v1.x - v2.x) <= tolerance
                   && Math.Abs(v1.y - v2.y) <= tolerance
                   && Math.Abs(v1.z - v2.z) <= tolerance;
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
        
        /// <summary>
        /// Gets a perpendicular vector to the given one, going clockwise.
        /// </summary>
        /// <param name="vector">The vector to get the perpendicular vector from.</param>
        /// <returns>The perpendicular vector.</returns>
        public static Vector2 PerpendicularClockwise(this Vector2 vector) => new Vector2(vector.y, -vector.x);

        /// <summary>
        /// Gets a perpendicular vector to the given one, going counterclockwise.
        /// </summary>
        /// <param name="vector">The vector to get the perpendicular vector from.</param>
        /// <returns>The perpendicular vector.</returns>
        public static Vector2 PerpendicularCounterclockwise(this Vector2 vector) => new Vector2(-vector.y, vector.x);

        /// <summary>
        /// Rounds the vector's values to the given number of decimal places.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>The rounded vector.</returns>
        public static Vector3 Round(this Vector3 vector, int decimalPlaces)
        {
            return new Vector3(
                (float) Math.Round(vector.x, decimalPlaces),
                (float) Math.Round(vector.y, decimalPlaces),
                (float) Math.Round(vector.z, decimalPlaces)
            );
        }        

        public static Vector2 ToXZ(this Vector3 v)
        {
            return new Vector3(v.x, v.z);
        }

        public static Vector3 ToXYZ(this Vector2 v, float y)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 ToXYZ(this Vector2 v, int y)
        {
            return new Vector3(v.x, y, v.y);
        }
    }
}