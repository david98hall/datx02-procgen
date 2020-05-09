using UnityEngine;

namespace BIAS.PCG.Terrain
{
    /// <summary>
    /// Contains terrain data.
    /// </summary>
    public struct TerrainInfo
    {
        /// <summary>
        /// The offset of the terrain.
        /// </summary>
        public Vector3 Offset { get; set; }
        
        /// <summary>
        /// The height map of the terrain.
        /// </summary>
        public float[,] HeightMap { get; set; }
    }
}