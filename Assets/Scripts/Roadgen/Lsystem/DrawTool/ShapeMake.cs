using System.Collections.Generic;
using UnityEngine;
using Utils.Geometry;

namespace DrawTool
{
    /// <summary>
    /// The main class for creating scene-based shapes in Unity.
    /// </summary>
    public class ShapeMake : MonoBehaviour
    {
        public MeshFilter meshFilter;

        [HideInInspector]
        // The list containing all the shapes
        public List<Shape> shapes = new List<Shape>();

        [HideInInspector]
        public bool showShapesList;
        /// <summary>
        /// The radius of each vertex in all shapes.
        /// </summary>
        public float handleRadius = .5f;

        public void UpdateMesh()
        {
            CompositeShape compShape = new CompositeShape(shapes);
            meshFilter.mesh = compShape.GetMesh();
        }
    }
}
