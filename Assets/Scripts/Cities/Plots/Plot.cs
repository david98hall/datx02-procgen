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
            // The axes you must test are the normals of each shape's edges.
            var axes = this.EdgeNormals().Concat(p.EdgeNormals());

            foreach (var axis in axes)
            {
                var p1 = projectOnto(axis);
                var p2 = p.projectOnto(axis);
                if (!overlap(p1, p2))
                {
                    // Based on SAT
                    return false;
                }
            }
            
            // If we found no overlap on any of the axes, we know there is no collision.
            return true;
        }

        // Find the normals of each edge of the plot        
        private IEnumerable<Vector3> EdgeNormals() 
        {
            var normals = new LinkedList<Vector3>();

            // Iterate over the vertices to find each edge vector
            using (var vertexEnum = Vertices.GetEnumerator())
            {
                if (!vertexEnum.MoveNext())
                    throw new ApplicationException("Cannot find edge normals of a plot without vertices.");
                
                var v1 = vertexEnum.Current;
                while (vertexEnum.MoveNext())
                {
                    var v2 = vertexEnum.Current;
                    // The direction of the normal doesn't matter so the order of subtraction is arbitrarily chosen.
                    var ev = v1 - vertexEnum.Current;
                    normals.AddLast(Maths3D.PerpendicularClockwise(ev).normalized);
                    v1 = v2;
                }
            }
            return normals;
        }

        /// <summary>
        /// Project each vertex of this plot onto a given axis, returning the minimum and maximum value.
        /// This can be seen as squashing a polygon (2D) onto a line (1D) returning an interval along the line.
        /// </summary>
        /// <returns>The minimum and maximum value of the projection.</returns>
        private Tuple<float, float> projectOnto(Vector3 axis)
        {
            // Use the enumerator instead of foreach in order to only have to loop once.
            using (var vertexEnum = Vertices.GetEnumerator())
            {
                if (!vertexEnum.MoveNext())
                    throw new ApplicationException("Cannot project a plot without vertices onto an axis.");
                var min = Vector3.Dot(vertexEnum.Current, axis);
                var max = min;

                while (vertexEnum.MoveNext())
                {
                    // For the projection we use the dot product
                    var dp = Vector3.Dot(vertexEnum.Current, axis);
                    if (dp < min)
                    {
                        min = dp;
                    } else if (dp > max)
                    {
                        max = dp;
                    }
                }
                return new Tuple<float, float>(min, max);
            }
        }
        
        // Tests if two projections on an axis are overlapping
        private static bool overlap(Tuple<float, float> p1, Tuple<float, float> p2)
        {
            // Easy to understand if you think of the intervals as time. The equation essentially answers the
            // question: "could two people have met?", with: "yes, if both were born before the other died".
            return p1.Item1 < p2.Item2 && p2.Item1 < p1.Item2;
        }
    }
    
    
}