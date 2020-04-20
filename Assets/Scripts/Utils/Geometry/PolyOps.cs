using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Geometry;
using System.Linq;

/// <summary>
/// Contains useful operations on polygonal shapes.
/// </summary>
public static class PolyOps
{

    /// <summary>
    /// Scale a polygon inwards using offsets. Currently does not handle vanishing vertices.
    /// NOTE: polygons are defined as paths; the last vertex is the first
    /// </summary>
    /// <param name="poly"></param> The polygon to scale
    /// <param name="offset"></param> The amount to scale
    /// <param name="cw"></param> Winding order of polygon
    /// <returns></returns> A new, scaled set of points for polygon
    public static ICollection<Vector3> ShrinkPoly(IList<Vector3> poly, float offset)
    {
        var scaledPoly = new List<Vector3>();

        // Scale all edges
        for (int i = 1; i < poly.Count; i++)
        {
            var prev = poly[i - 1];
            var next = poly[i];
            var dir = next - prev;

            // Obtain orthogonal direction vector
            dir = Vector3.Cross(dir, Vector3.up).normalized;

            var offsetPrev = prev + (dir * offset);
            var offsetNext = next + (dir * offset);
            scaledPoly.Add(offsetPrev);
            scaledPoly.Add(offsetNext);
        }

        var newPoly = new List<Vector3>();

        // Get new intersections
        int n = scaledPoly.Count;
        for (int i = 1; i < scaledPoly.Count; i += 2)
        {
            int j = (i < scaledPoly.Count - 1) ? i + 2 : 1;

            var v1 = scaledPoly[i - 1];
            var v2 = scaledPoly[i];
            var v3 = scaledPoly[j - 1];
            var v4 = scaledPoly[j];

            if (Maths3D.LineSegmentIntersection(out var intersection, v1, v2, v3, v4))
            {
                newPoly.Add(intersection);
            }
        }
        newPoly.Add(newPoly.First());

        return newPoly;
    }
}
