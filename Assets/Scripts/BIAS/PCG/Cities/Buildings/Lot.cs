using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils.Geometry;

namespace BIAS.PCG.Cities.Buildings
{
    /// <summary>
    /// Contains exactly one building.
    /// Contains area, center and facing for building generation operations.
    /// </summary>
    internal class Lot
    {
        internal readonly float area;
        //public readonly Vector3 center;
        // internal readonly Vector2 facing;

        /// <summary>
        /// The polygonal shape of the lot.
        /// </summary>
        internal IList<Vector3> Vertices { get; }

        internal Lot(IList<Vector3> vertices)
        {
            Vertices = vertices;

            this.area = Maths2D.CalculatePolygonArea((IEnumerable<Vector2>) ToXZ(vertices));
        }

        private List<Vector2> ToXZ(IEnumerable<Vector3> vecs)
        {
            List<Vector2> vecsXZ = new List<Vector2>();

            foreach (Vector3 v in vecs)
            {
                vecsXZ.Add(new Vector2(v.x, v.z));
            }
            return vecsXZ;
        }
    }
}
