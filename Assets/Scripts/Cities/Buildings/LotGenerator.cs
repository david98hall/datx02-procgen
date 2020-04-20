using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cities.Plots;
using Utils.Geometry;
using System.Linq;
using Extensions;

/// <summary>
/// Generates lots from plots through subdivision. 
/// The lots will then hold buildings placed within their lots according to parameters.
/// </summary>
/// 
public class LotGenerator
{
    internal float splitOffset;
    internal float minSize = 10f;
    internal float maxRatio = 2f;
    internal float maxSide;
    internal float minSide;

    private readonly float boundsFactor;
    private readonly bool clockwise;
    private int mode;

    private readonly Plot plot;
    private readonly ICollection<Lot> lots = new List<Lot>();
    //private HashSet<Vector3> accessPoints;


    public LotGenerator(Plot plot, int mode)
    {
        this.plot = plot;

        var verts = (List<Vector3>) plot.Vertices;
        clockwise = Maths2D.PointsAreCounterClockwise(verts);

        var bounds = Maths2D.GetExtremeBounds(ToXZ(verts));
        boundsFactor = new Vector2(bounds.MaxX - bounds.MinX, bounds.MaxY - bounds.MinY).sqrMagnitude;

        //accessPoints = new HashSet<Vector3>(verts);
        this.mode = mode;
    }

    /// <summary>
    /// Generates lots from plots, depending on chosen method.
    /// </summary>
    /// <returns>A collection of the lots obtained from subdivision.</returns>
    public ICollection<Lot> Generate()
    {
        lots.Clear();

        var verts = plot.Vertices;
        var poly = new List<Vector3>();
        poly.AddRange(verts);

        switch (mode)
        {
            case 0: BisectorDivide(poly); break;
            case 1: OffsetDivide(poly, 2f); break;
        }

        return lots;
    }

    #region Offset lot division

    /// <summary>
    /// Subdivides a plot by shrinking its borders and then splitting margin according to given parameters.
    /// </summary>
    /// <param name="mainLot"></param> The main lot.
    /// <param name="offset"></param> The amount to offset inwards.
    private void OffsetDivide(List<Vector3> mainLot, float offset)
    {
        var center = Maths2D.GetCenterPoint(ToXZ(mainLot));

        //// Old for reference
        //lots.Add(new Lot(poly, new Vector3(center.x, 0, center.y)));

        IList<Vector3> innerBorder = (IList<Vector3>) PolyOps.ShrinkPoly(mainLot, offset);

        SplitMargin(mainLot, innerBorder);
    }

    // TODO: not working
    private void SplitMargin(IList<Vector3> outerBorder, IList<Vector3> innerBorder)
    {
        float length = 1f;
        IList<Vector3> pairs = new List<Vector3>();

        for (int i = 1; i < innerBorder.Count; i++)
        {
            var inner1 = innerBorder[i - 1];
            var inner2 = innerBorder[i];
            var outer1 = outerBorder[i - 1];
            var outer2 = outerBorder[i];
            var dir = inner2 - inner1;

            var c = Vector3.Cross(Vector3.up, dir.normalized) * 10;

            var line1 = (dir * (0.5f) + inner1) - c; // point j * length units along inner border
            var line2 = line1 + (c*2); // orthogonal line from inner border outwards

            if (Maths3D.LineSegmentIntersection(out var intersectionIn, inner1, inner2, line1, line2))
                pairs.Add(intersectionIn);
            else if (Maths3D.LineSegmentIntersection(out var intersectionOut, outer1, outer2, line1, line2))
                pairs.Add(intersectionOut);

                //for (int j = 1; j < Mathf.RoundToInt(dir.magnitude); j++)
                //{
                //    var line1 = dir.normalized * (length * j) + inner1; // point j * length units along inner border
                //    var line2 = line1 + c; // orthogonal line from inner border outwards

                //    if (Maths3D.LineSegmentIntersection(out var intersectionIn, inner1, inner2, line1, line2))
                //        pairs.Add(intersectionIn);
                //    else if (Maths3D.LineSegmentIntersection(out var intersectionOut, outer1, outer2, line1, line2))
                //        pairs.Add(intersectionOut);
                //}
        }

        for (int i = 3; i < pairs.Count; i+=2)
        {
            lots.Add(new Lot(new List<Vector3> {pairs[i-3], pairs[i-2], pairs[i], pairs[i-1], pairs[i-3]}));
        }
    }

    #endregion

    #region Bisector lot division

    /// <summary>
    /// Recursively split a polygonal shape along its bisector.
    /// </summary>
    /// <param name="poly">The polygonal shape to split.</param>
    private void BisectorDivide(IList<Vector3> poly)
    {
        var baseVars = FindBaseVariables(poly);

        if (TerminationConditions(baseVars.area, baseVars.ratio))
        {
            var center = Maths2D.GetCenterPoint(ToXZ(poly));
            lots.Add(new Lot(poly));
        }
        else
        {
            var newLots = SplitAtEdge(poly, baseVars.index);

            BisectorDivide(newLots.Item1);
            BisectorDivide(newLots.Item2);
        }
    }

    /// <summary>
    /// Checks base cases for recursion
    /// </summary>
    /// <param name="area">Area of polygon.</param>
    /// <param name="ratio">Side ratio of polygon.</param>
    /// <returns>Continue or not.</returns>
    private bool TerminationConditions(float area, float ratio)
    {
        return area < minSize || ratio > maxRatio;
    }


    /// <summary>
    /// Split a polygon along the bisector of the longest edge. Works for convex polygons and concave polygons
    /// for which the bisector only exits the polygon once.
    /// </summary>
    /// <param name="poly">The polygon to split.</param>
    /// <param name="index">The index at which to split.</param>
    /// <returns></returns>
    private (IList<Vector3>, IList<Vector3>) SplitAtEdge(IList<Vector3> poly, int index)
    {

        var indices = new List<int>();
        var intersections = new Dictionary<int, Vector3>();

        var prev = poly[index];
        var next = poly[(index + 1) % poly.Count];
        var nextDir = (next - prev);

        // Halfway point along the edge
        var splitPoint = (nextDir * 0.5f ) + prev;

        // Perpendicular line direction, scaled outside bounds
        var splitVector = (clockwise ? Vector3.Cross(nextDir, Vector3.up) : Vector3.Cross(Vector3.up, nextDir)).normalized;
        splitVector = splitVector * boundsFactor;

        for (int i = 1; i < poly.Count; i++)
        {
            if (Maths3D.LineSegmentIntersection(out var intersection, poly[i - 1], poly[i], splitPoint - splitVector, splitVector + splitPoint))
            {
                if (!intersections.ContainsValue(intersection))
                {
                    intersections.Add(i - 1, intersection);
                    indices.Add(i - 1); // ordered
                }
            }
        }

        // Should not happen
        if (intersections.Count < 2)
        {
            throw new UnityException("Not enough intersections when splitting lot");
        }

        // Get new polygons with intersections
        var poly1 = new List<Vector3>();
        var poly2 = new List<Vector3>();

        int start = indices[0];
        int end = indices[1];

        for (int i = 0; i < poly.Count; i++)
        {
            if (i <= start || i > end)
                poly1.Add(poly[i]);
            else
                poly2.Add(poly[i]);
        }

        poly1.Insert(start + 1, intersections[indices[1]]);
        poly1.Insert(start + 1, intersections[indices[0]]);

        poly2.Add(intersections[indices[1]]);
        poly2.Add(intersections[indices[0]]);
        poly2.Add(poly[start + 1]);

        return (poly1, poly2);
    }

    /// <summary>
    /// Finds variables for recursive split, as well as the index at which the polygon should split.
    /// </summary>
    /// <param name="poly">The polygon to split.</param>
    /// <returns>The split index, area and side ratio of the polygon</returns>
    private (int index, float area, float ratio) FindBaseVariables(IList<Vector3> poly)
    {
        int splitIndex = 0;
        float max = (poly[1] - poly[0]).magnitude;
        float min = max;
        float area = 0;

        for (int i = 1; i < poly.Count; i++)
        {
            float f = (poly[i] - poly[i - 1]).magnitude;

            if (f > max)
            {
                max = f;
                splitIndex = i - 1;
            }
            else
            {
                min = Mathf.Min(min, f);
            }

            float a = poly[i - 1].x * poly[i].z;
            float b = poly[i - 1].z * poly[i].x;
            area += (a - b);
        }

        var (MinX, MinY, MaxX, MaxY) = Maths2D.GetExtremeBounds(ToXZ(poly));

        var bmax = (Mathf.Max((MaxX - MinX), (MaxY - MinY)));
        var bmin = (Mathf.Min((MaxX - MinX), (MaxY - MinY)));

        return (splitIndex, Mathf.Abs(area/2), bmax / bmin);
    }

    #endregion

    private List<Vector2> ToXZ(IEnumerable<Vector3> vecs)
    {
        List<Vector2> vecsXZ = new List<Vector2>();

        foreach (Vector3 v in vecs)
        {
            vecsXZ.Add(v.ToXZ());
        }
        return vecsXZ;
    }
}
