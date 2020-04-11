using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using Utils.Geometry;

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

        /// <summary>
        /// Check if this plot collides with another given plot using the separating axis theorem (SAT).
        /// Based on http://www.dyn4j.org/2010/01/sat/#sat-convex
        /// </summary>
        /// <param name="p">The plot to check collision with.</param>
        /// <returns>A boolean that states whether or not the plots are colliding</returns>
        public bool CollidesWith(Plot p) 
        {
            // In SAT, the axes you must test are the normals of each shape's edges.
            var axes = Maths2D.EdgeNormals(Vertices).Concat(Maths2D.EdgeNormals(p.Vertices));

            foreach (var axis in axes)
            {
                var p1 = Maths2D.ProjectPolygonOntoAxis(Vertices, axis);
                var p2 = Maths2D.ProjectPolygonOntoAxis(p.Vertices, axis);
                if (!overlap(p1, p2))
                {
                    // Based on SAT
                    // If we found no overlap on any of the axes, we know there is no collision.
                    return false;
                }
            }
            
            return true;
        }
        
        // Tests if two projections on an axis are overlapping
        private static bool overlap(System.ValueTuple<float, float> p1, System.ValueTuple<float, float> p2)
        {
            // Easy to understand if you think of the intervals as time. The equation essentially answers the
            // question: "could two people have met?", with: "yes, if both were born before the other died".
            return p1.Item1 < p2.Item2 && p2.Item1 < p1.Item2;
        }
    }
    
    
}