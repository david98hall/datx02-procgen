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
        var verts = plot.Vertices.ToList();

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
            //case 1: OffsetDivide(poly, 2f); break;
        }

        return lots;
    }


    #region Bisector lot division

    /// <summary>
    /// Recursively split a polygonal shape along its bisector.
    /// </summary>
    /// <param name="poly"></param> The polygonal shape to split.
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
    /// <param name="area"></param> Area of polygon
    /// <param name="ratio"></param> Side ratio of polygon
    /// <returns>Continue or not.</returns>
    private bool TerminationConditions(float area, float ratio)
    {
        return area < minSize || ratio > maxRatio;
    }


    /// <summary>
    /// Split a polygon along the bisector of the longest edge. Works for convex polygons and concave polygons
    /// for which the bisector only exits the polygon once.
    /// </summary>
    /// <param name="poly"></param> The polygon to split
    /// <param name="index"></param> The index at which to split
    /// <returns></returns>
    private (IList<Vector3>, IList<Vector3>) SplitAtEdge(IList<Vector3> poly, int index)
    {

        var indices = new List<int>();
        var intersections = new Dictionary<int, Vector3>();

        var prev = poly[index];
        var next = poly[(index + 1) % poly.Count];
        var nextDir = (next - prev);

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

                    //if (accessPoints.Contains(poly[i - 1]) && accessPoints.Contains(poly[i]))
                    //    accessPoints.Add(intersection);
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
    /// <param name="poly"></param> The polygon to split.
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

    // Add these as V3 extensions when time
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
