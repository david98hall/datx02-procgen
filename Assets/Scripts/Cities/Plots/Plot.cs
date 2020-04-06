using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Cities.Plots
{
    /// <summary>
    /// Represents a plot of land where something can be built.
    /// </summary>
    public class Plot : ICloneable
    {
        public IEnumerable<Vector3> Vertices => _shapeVertices.Select(v => v.Clone());
        private readonly ICollection<Vector3> _shapeVertices = new LinkedList<Vector3>();

        public Plot()
        {
        }

        /// <summary>
        /// Sets the shape of this plot.
        /// </summary>
        /// <param name="shape">The shape of this plot.</param>
        public Plot(IEnumerable<Vector3> shape)
        {
            SetShapeVertices(shape);
        }
        
        /// <summary>
        /// Clones the passed Plot.
        /// </summary>
        /// <param name="other">The Plot to clone.</param>
        public Plot(Plot other) : this(other._shapeVertices)
        {
        }
        
        /// <summary>
        /// Sets the vertices of the plot's shape.
        /// </summary>
        /// <param name="shapeVertices">The vertices of the plot's shape.</param>
        internal void SetShapeVertices(IEnumerable<Vector3> shapeVertices)
        {
            _shapeVertices.Clear();
            _shapeVertices.AddRange(shapeVertices, v => new Vector3(v.x, v.y, v.z));
        }

        public object Clone() => new Plot(this);

        public Boolean collidesWith(Plot p) 
        {
            var axes = new LinkedList<Vector3>();
            
            throw new System.NotImplementedException();
            /* Pseudocode
            # project both shapes onto the axis
            Projection p1 = shape1.project(axis);
            Projection p2 = shape2.project(axis);
             # do the projections overlap?
            if (!p1.overlap(p2)) {
                // then we can guarantee that the shapes do not overlap
                return false;
            }
            */
        }

        // Find the normals of each edge of the plot        
        private IEnumerable<Vector3> edgeNormals() 
        {
            var normals = new LinkedList<Vector3>();

            // Iterate over the vertices to find each edge
            using (var vertexEnum = Vertices.GetEnumerator())
            {
                
            }
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                Vector3 v1 = Vertices[i];
                Vector3 v2 = Vertices[i + 1];

                // Subtract the two vertices to get the edge vector
                Vector3 ev = v1 - v2;
                // The direction of the normal doesn't matter

            }
            return normals;
        }
    }
}