using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Linq;
using Cities.Plots;
using Utils.Geometry;
using Interfaces;

/// <summary>
/// Generator for buildings. 
/// Will use strategy to construct building according to lot shape and population density (NYI).
/// Some plots are suitable for multiple buildings; these should be split into street-bordering 
/// buildings and a central green area. This is done by a Lot generator.
/// </summary>
public class ExtrusionStrategy : Strategy<float[,], ICollection<Building>>
{
    private IEnumerator plots;

    private float[,] heatMap;
    private float[,] heightMap;

    private float minArea;

    private ICollection<Building> buildings;


    internal ExtrusionStrategy(IInjector<float[,]> heightmapInjector, float minArea) : base(heightmapInjector)
    {
        this.minArea = minArea;
        heightMap = heightmapInjector.Get();
        buildings = new List<Building>();
    }

    internal void AddPlots(IEnumerator plots)
    {
        this.plots = plots;
    }

    /// <summary>
    /// Generate all buildings in the given set of plots.
    /// </summary>
    /// <returns>The generated buildings.</returns>
    public override ICollection<Building> Generate()
    {

        while (plots.MoveNext())
        {
            if (plots.Current != null)
            {
                LotGenerator lg = new LotGenerator( (Plot) plots.Current, 0);
                ICollection<Lot> lots = lg.Generate();

                buildings = GetBuildings((IList<Lot>)lots);
            }
        }

        return buildings;
    }

    /// <summary>
    /// Generate all buildings in a set of lots.
    /// </summary>
    /// <param name="lots">The lots to generate a building in.</param>
    /// <returns></returns>
    public ICollection<Building> GetBuildings(ICollection<Lot> lots)
    {
        IList<Building> buildings = new List<Building>();

        foreach (Lot lot in lots)
        {
            // Only generate building if suitable lot
            if (ValidLot(lot))
            {
                //float y = heightMap[Mathf.RoundToInt(lot.center.x), Mathf.RoundToInt(lot.center.z)];
                float y = Random.Range(1f, 5.5f);
                float marginSize = 0.5f;

                IList<Vector3> vertices = (IList<Vector3>) PolyOps.ShrinkPoly(lot.Vertices.ToList(), marginSize);

                // Polygon centroid
                Vector2 c = Maths2D.GetConvexCenter(ToXZ(vertices));
                Vector3 center = new Vector3(c.x, 0, c.y);

                vertices.RemoveAt(vertices.Count - 1);
                var tup = SetBaseHeight(vertices);

                var (verts, tris) = ExtrudePolygon(vertices, tup.max + y);
                Mesh m = BuildMesh(verts, tris);

                buildings.Add(new Building(center, Vector2.zero, m));
            }
        }

        return buildings;
    }

    private bool ValidLot(Lot lot)
    {
        bool valid = false;

        if (lot.area >= minArea)
            valid = true;

        return valid;
    }

    /// <summary>
    /// Finds the height of the terrain for each building base vertex, as well as the maximum height for 
    /// extruding the top into a flat shape suitable for more floors. (May be unnecessary with min height)
    /// </summary>
    /// <param name="poly">The polygonal 2D shape of the building.</param>
    /// <returns>The y-translated shape and the maximum height.</returns>
    private (IList<Vector3> vertices, float max) SetBaseHeight(IList<Vector3> poly)
    {
        float max = float.MinValue;
        float min = MinimumHeightInBounds(poly);

        for (int i = 0; i < poly.Count; i++)
        {
            int x = Mathf.FloorToInt(poly[i].x);
            int y = Mathf.FloorToInt(poly[i].z);
            float fx = poly[i].x - x;
            float fy = poly[i].z - y;

            float height1 = Mathf.Lerp(heightMap[x, y], heightMap[x + 1, y], fx);
            float height2 = Mathf.Lerp(heightMap[x, y + 1], heightMap[x + 1, y + 1], fx);
            float finalHeight = Mathf.Lerp(height1, height2, fy);

            max = Mathf.Max(finalHeight, max);

            poly[i] = new Vector3(poly[i].x, min, poly[i].z);
        }
        return (poly, max);
    }

    private float MinimumHeightInBounds(IList<Vector3> poly)
    {
        foreach (var p in poly)
            Debug.Log(p.x + ", " + p.z);
        var (minX, minY, maxX, maxY) = Maths2D.GetExtremeBounds(ToXZ(poly));

        int MinX = Mathf.FloorToInt(minX);
        int MaxX = Mathf.CeilToInt(maxX);
        int MinY = Mathf.FloorToInt(minY);
        int MaxY = Mathf.CeilToInt(maxY);

        float min = 0f;

        for (int y = MinY; y <= MaxY; y++)
        {
            for (int x = MinX; x <= MaxX; x++)
            {
                min = Mathf.Min(min, heightMap[x, y]);
            }
        }

        return min;
    }

    /// <summary>
    ///  Uniformly extends a polygon upwards and connects the vertices on the sides.
    /// </summary>
    /// <param name="vertices">The points of the original 2D shape.</param>
    /// <param name="distance">The distance to extend the polygon.</param>
    /// <returns>A tuple of the new vertex and index list.</returns>
    private (ICollection<Vector3>, ICollection<int>) ExtrudePolygon(IList<Vector3> vertices, float distance)
    {
        //// determine winding order
        //bool ccw = Maths2D.PointsAreCounterClockwise(ToXZ(vertices));

        // Prepare a shape for extrusion (using triangulator to get tris)
        Triangulator t = new Triangulator(new Polygon(ToXZ(vertices).ToArray(), new Vector2[0][]));
        int[] bottomTris = t.Triangulate();

        ICollection<int> tris = new List<int>();
        tris.AddRange(bottomTris);

        // Add indices and points for the additional vertex array (which has same relative indices as bottom)
        int n = vertices.Count;
        for (int i = 0; i < bottomTris.Length; i++)
        {
            tris.Add(bottomTris[i] + n);
        }

        for (int i = 0; i < n; i++)
        {
            vertices.Add(new Vector3(vertices[i].x, distance, vertices[i].z));
        }

        return ConstructSideFaces(vertices, tris);
    }

    /// <summary>
    /// Constructs a simple face of 4 vertices for each edge, connecting the extruded polygon to the base one.
    /// </summary>
    /// <param name="verts">The full list of vertices.</param>
    /// <param name="tris">The full list of triangles.</param>
    /// <returns>The provided vertices and triangles with added sides.</returns>
    private (ICollection<Vector3>, ICollection<int>) ConstructSideFaces(IList<Vector3> verts, ICollection<int> tris)
    {
        int n = (verts.Count / 2);
        int j = verts.Count;
        int i11 = n - 1;
        int i21 = j - 1;
        int i22 = n;

        for (int i12 = 0; i12 < n; i11 = i12++)
        {
            // Triangle one
            verts.Add(verts[i12]);
            verts.Add(verts[i11]);
            verts.Add(verts[i22]);

            tris.Add(j++);
            tris.Add(j++);
            tris.Add(j++);

            // Triangle two
            verts.Add(verts[i21]);

            tris.Add(j - 1);
            tris.Add(j - 2);
            tris.Add(j++);
            i21 = i22++;
        }

        return (verts, tris);
    }


    /// <summary>
    /// Constructs a mesh out of the given vertices and triangles.
    /// </summary>
    /// <param name="verts">The list of points to add to the mesh.</param>
    /// <param name="tris">The indices for the vertex array.</param>
    /// <returns>The mesh constructed from the arrays.</returns>
    private Mesh BuildMesh(ICollection<Vector3> verts, ICollection<int> tris)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = new Vector2[verts.Count];
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    #region Utility methods

    private List<Vector2> ToXZ(IEnumerable<Vector3> vecs)
    {
        List<Vector2> vecsXZ = new List<Vector2>();

        foreach (Vector3 v in vecs)
        {
            vecsXZ.Add(v.ToXZ());
        }
        return vecsXZ;
    }

    #endregion
}
