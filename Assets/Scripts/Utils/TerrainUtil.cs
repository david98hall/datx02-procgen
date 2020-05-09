using System;
using UnityEngine;

namespace Utils
{
    public static class TerrainUtil
    {
        
        /// <summary>
        /// Returns a vertex based on the passed one, that is within the width and depth of the terrain.
        /// </summary>
        /// <param name="v">The original vertex.</param>
        /// <param name="width">The width of the terrain.</param>
        /// <param name="depth">The depth of the terrain.</param>
        /// <returns>A vertex within the terrain's bounds.</returns>
        public static Vector2 ToTerrainVertex(this Vector2 v, int width, int depth)
        {
            v.x = Math.Max(Math.Min(v.x, width - 1), 0);
            v.y = Math.Max(Math.Min(v.y, depth - 1), 0);
            return v;
        }
        
        /// <summary>
        /// Returns a vertex based on the passed one, that is within the width and depth of the terrain.
        /// </summary>
        /// <param name="v">The original vertex.</param>
        /// <param name="width">The width of the terrain.</param>
        /// <param name="depth">The depth of the terrain.</param>
        /// <returns>A vertex within the terrain's bounds.</returns>
        public static Vector2Int ToTerrainVertex(this Vector2Int v, int width, int depth)
        {
            v.x = Math.Max(Math.Min(v.x, width - 1), 0);
            v.y = Math.Max(Math.Min(v.y, depth - 1), 0);
            return v;
        }
    }
}