using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generated buildings. Each building is defined by its vertices, bounds, position and facing.
/// </summary>
public class Building
{
    private float width;
    private float height;
    internal Vector3 position;
    internal Vector2 facing;
    internal  Mesh mesh;

    public Building(Vector3 position, Vector2 facing, Mesh mesh)
    {
        this.width = mesh.bounds.size.x;
        this.height = mesh.bounds.size.z;
        this.position = position;
        this.facing = facing;
        this.mesh = mesh;
    }

}
