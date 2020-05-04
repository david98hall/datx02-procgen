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

            foreach (var l in newLots)
                BisectorDivide(l);
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
    private IList<List<Vector3>> SplitAtEdge(IList<Vector3> poly, int index)
    {

        var indices = new List<int>();
        var intersectionLines = new Dictionary<Vector3, (Vector3 start, Vector3 end)>();
        var intersections = new List<(Vector3, Vector3)>();

        var prev = poly[index];
        var next = poly[(index + 1) % poly.Count];
        var nextDir = (next - prev);

        var splitPoint = (nextDir * 0.5f) + prev;

        // Perpendicular line direction, scaled outside bounds
        var splitVector = (clockwise ? Vector3.Cross(nextDir, Vector3.up) : Vector3.Cross(Vector3.up, nextDir)).normalized;
        splitVector = splitVector * boundsFactor;

        var p0 = splitPoint - splitVector;
        var p1 = splitVector + splitPoint;

        // Collect all intersections
        for (int i = 1; i < poly.Count; i++)
        {
            if (Maths3D.LineSegmentIntersection(out var intersection, poly[i - 1], poly[i], p0, p1))
            {
                if (!intersections.Contains( (poly[i-1], intersection) ))
                {
                    intersections.Add( (poly[i-1], intersection) );
                    indices.Add(i - 1); // ordered
                }
            }
        }

        // Gather intersections into pairs by sorting according to order of occurrence along split line
        IList<(Vector3 key, Vector3 point)> sorted = intersections.OrderBy(v => (v.Item2 - splitPoint).magnitude).ToList();

        // With the sorted list, we can know which intersection pairs are inside the polygon and thus are valid edges
        for (int i = 1; i < sorted.Count; i+=2)
        {
            intersectionLines.Add(sorted[i-1].key, (sorted[i-1].point, sorted[i].point));
            intersectionLines.Add(sorted[i].key, (sorted[i].point, sorted[i-1].point));
        }

        var polyLink = new LinkedList<Vector3>(poly);
        IList<List<Vector3>> polys = new List<List<Vector3>>();

        // The intersection pairs should be indexed and ready to be added when we add points to our new polygons
        int k = -1;
        var start = polyLink.First;
        var curr = start;
        var nextt = curr.Next;
        while (polyLink.Count > 0)
        {
            // We have arrived back at the start for this polygon, create new list and start over
            if (curr == start)
            {
                start = polyLink.First;
                curr = start;
                polys.Add(new List<Vector3>());
                k++;
            }


            if (intersectionLines.TryGetValue(curr.Value, out var intersectionLine1))
            {
                // Add intersection to polygon
                polys[k].Add(intersectionLine1.start);
                polys[k].Add(intersectionLine1.end);

                // Iterate past other polygon back to this one
                while (!intersectionLines.TryGetValue(nextt.Value, out var v))
                {
                    curr = nextt;
                    nextt = nextt.Next ?? polyLink.First;
                }
                curr = nextt.Next ?? polyLink.First;
                nextt = curr.Next ?? polyLink.First;

                // need to consider what happens when we're back at start
            }

            // Add this node to new polygon and remove from the list
            polys[k].Add(curr.Value);
            polyLink.Remove(curr);

            curr = nextt;
            nextt = nextt.Next ?? polyLink.First;
        }


        //// Should not happen
        //if (intersections.Count < 2)
        //{
        //    throw new UnityException("Not enough intersections when splitting lot");
        //}

        return polys;
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
