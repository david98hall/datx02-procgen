﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cities.Plots;

/// <summary>
/// Contains exactly one building.
/// Contains area, center and facing for building generation operations.
/// </summary>
public class Lot
{
    public readonly float area;
    //public readonly Vector3 center;
    public readonly Vector2 facing;

    /// <summary>
    /// The polygonal shape of the lot.
    /// </summary>
    internal IEnumerable<Vector3> Vertices { get; }

    public Lot(IEnumerable<Vector3> vertices)
    {
        Vertices = vertices;

        List<Vector3> v = vertices.ToList();
        float area = 0;
        for (int i = 1; i < v.Count; i++)
        {
            float a = v[i - 1].x * v[i].z;
            float b = v[i - 1].z * v[i].x;

            area += (a - b);
        }
        this.area = Mathf.Abs(area / 2);

        //this.center = center;
        //this.facing = facing;
    }
}