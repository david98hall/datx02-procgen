using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Linq;
using Cities.Plots;
using Utils.Geometry;
using Interfaces;
using Terrain;
using Utils;
using Utils.Concurrency;

/// <summary>
/// Generator for buildings. 
/// Will use strategy to construct building according to lot shape and population density (NYI).
/// Some plots are suitable for multiple buildings; these should be split into street-bordering 
/// buildings and a central green area. This can be done by a Lot generator.
/// </summary>
public class ExtrusionStrategy : Strategy<(TerrainInfo, IEnumerable<Plot>), IEnumerable<Building>>
{
    #region Fields

    /// <summary>
    /// The plots to generate buildings on.
    /// </summary>
    private IEnumerable plots;

    /// <summary>
    /// The generated buildings.
    /// </summary>
    private ICollection<Building> buildings;

    /// <summary>
    /// Population heat map for determining building types and height.
    /// </summary>
    private float[,] heatMap;

    /// <summary>
    /// The injected height map for the terrain heights.
    /// </summary>
    //private float[,] heightMap;

    /// <summary>
    /// Minimal lot area for a building to be placed on it.
    /// </summary>
    private readonly float minArea;

    /// <summary>
    /// Maximal lot area for a building to be placed on it.
    /// </summary>
    private readonly float maxArea;

    #endregion

    /// <summary>
    /// Construct a building extrusion strategy, using injector inputs.
    /// </summary>
    /// <param name="injector">The injector for height map and plots.</param>
    /// <param name="minArea">The minimal area of lot.</param>
    /// <param name="maxArea">The maximal area of lot.</param>
    public ExtrusionStrategy(
        IInjector<(TerrainInfo TerrainInfo, IEnumerable<Plot> Plots)> injector, 
        float minArea, 
        float maxArea) 
        : base(injector)
    {
        this.minArea = minArea;
        this.maxArea = maxArea;

        //heightMap = injector.Get().TerrainInfo.HeightMap;
        //plots = injector.Get().Item2;

        buildings = new List<Building>();
    }

    /// <summary>
    /// Generate all buildings in all the supplied plots. Can be used with a lot generator
    /// or just a single plot.
    /// </summary>
    /// <returns>The set of all generated buildings.</returns>
    public override IEnumerable<Building> Generate()
    {
        foreach (var p in Injector.Get().Item2)
        {
            // Cancel if requested
            if (CancelToken.IsCancellationRequested) return null;
            
            //LotGenerator lg = new LotGenerator((Plot)plots.Current, 0);
            //ICollection<Lot> lots = lg.Generate();

            var vertices = p.Vertices;

            // Necessary for non-counter-clockwise plots
            if (!Maths2D.PointsAreCounterClockwise(vertices.ToList()))
                vertices = vertices.Reverse();

            Lot lot = new Lot(vertices);

            GetBuildings(new List<Lot> { lot });
        }

        return buildings;
    }


    /// <summary>
    /// Generate all buildings in a set of lots. Will only generate one building per lot.
    /// These may be convex or concave shapes, and the building appearance will be based on it.
    /// </summary>
    /// <param name="lots">The lots to generate a building in.</param>
    public void GetBuildings(ICollection<Lot> lots)
    {
        foreach (Lot lot in lots)
        {
            // Cancel if requested
            if (CancelToken.IsCancellationRequested) return;
            
            // Only generate building if suitable lot
            if (ValidLot(lot))
            {
                float y = MathUtils.RandomInclusiveFloat(1f, 5.5f);

                IList<Vector3> vertices = lot.Vertices.ToList();

                // Polygon centroid
                Vector2 c = Maths2D.GetConvexCenter(ToXZ(vertices));

                vertices.RemoveAt(vertices.Count - 1);
                var tup = SetBaseHeight(vertices);

                var (verts, tris) = ExtrudePolygon(vertices, tup.max + y);
                Mesh m = BuildMesh(verts, tris);

                buildings.Add(new Building(c.ToXYZ(0f), Vector2.zero, m));
            }
        }
    }

    /// <summary>
    /// Check the parameters to determine whether a building can be placed inside the lot.
    /// </summary>
    /// <param name="lot">The lot to potentially place the building in.</param>
    /// <returns>Can be placed or not.</returns>
    private bool ValidLot(Lot lot)
    {
        return lot.area >= minArea && lot.area < maxArea;
    }

    /// <summary>
    /// Finds the height of the terrain for each building base vertex, as well as the minimum 
    /// height for any points under the shape so that it is always under the mesh.
    /// </summary>
    /// <param name="poly">The polygonal 2D shape of the building.</param>
    /// <returns>The y-translated shape and the maximum height.</returns>
    private (IList<Vector3> vertices, float max) SetBaseHeight(IList<Vector3> poly)
    {
        float max = float.MinValue;
        float min = MinimumHeightInBounds(poly);

        var heightMap = Injector.Get().Item1.HeightMap;
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

    /// <summary>
    /// Get the minimum height in the bounding box of the building, ensuring it is below
    /// terrain height.
    /// </summary>
    /// <param name="poly">The polygonal shape.</param>
    /// <returns>The minimum value.</returns>
    private float MinimumHeightInBounds(IList<Vector3> poly)
    {
        var (minX, minY, maxX, maxY) = Maths2D.GetExtremeBounds(ToXZ(poly));

        int MinX = Mathf.FloorToInt(minX);
        int MaxX = Mathf.CeilToInt(maxX);
        int MinY = Mathf.FloorToInt(minY);
        int MaxY = Mathf.CeilToInt(maxY);

        float min = float.MaxValue;

        var heightMap = Injector.Get().Item1.HeightMap;
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
    ///  Extends a polygon upwards and connects the vertices on the sides.
    /// </summary>
    /// <param name="vertices">The points of the original 2D shape.</param>
    /// <param name="distance">The distance to extend the polygon.</param>
    /// <returns>A tuple of the new vertex and index list.</returns>
    private (ICollection<Vector3>, ICollection<int>) ExtrudePolygon(IList<Vector3> vertices, float distance)
    {
        // Prepare a shape for extrusion (using triangulator to get tris)
        Triangulator t = new Triangulator(new Polygon(ToXZ(vertices).ToArray(), new Vector2[0][]));
        int[] bottomTris = t.Triangulate();

        if (bottomTris == null)
        {
            Debug.Log("Polygon: ");
            foreach (var v in vertices)
                Debug.Log(v);
        }

        IList<int> tris = new List<int>();
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
    /// Constructs a simple face of 4 vertices for each edge, connecting the extruded polygon 
    /// to the base one.
    /// </summary>
    /// <param name="verts">The full list of vertices.</param>
    /// <param name="tris">The full list of triangles.</param>
    /// <returns>The provided vertices and triangles with added sides.</returns>
    private (ICollection<Vector3>, ICollection<int>) ConstructSideFaces(
        IList<Vector3> verts, IList<int> tris)
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
        return Dispatcher.Instance.EnqueueFunction(() =>
        {
            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = new Vector2[verts.Count];
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        });
    }


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