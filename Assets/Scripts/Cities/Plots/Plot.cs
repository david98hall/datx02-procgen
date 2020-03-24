using System;
using System.Collections;
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
    }
}