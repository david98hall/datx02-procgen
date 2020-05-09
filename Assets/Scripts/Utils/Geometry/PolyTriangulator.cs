using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace Utils.Geometry
{
    /// <summary>
    /// A class for triangulating polygons by ear-clipping.
    /// </summary>
    public class PolyTriangulator
    {
        #region Fields

        /// <summary>
        /// The indices to output.
        /// </summary>
        private IList<int> indices;

        /// <summary>
        /// The vertex array that will be edited during triangulation.
        /// </summary>
        private LinkedList<Vector3> verts;

        /// <summary>
        /// The set of convex (angle less than 180) vertices in the polygon.
        /// </summary>
        private LinkedList<Vector3> convex;

        /// <summary>
        /// The set of reflex (angle more than 180) vertices in the polygon.
        /// </summary>
        private LinkedList<Vector3> reflex;

        /// <summary>
        /// The ears of the polygon. These are convex vertices which contain no reflex
        /// vertices from the polygon when forming a triangle with its two neighbours.
        /// </summary>
        private LinkedList<Vector3> ears;

        /// <summary>
        /// The polygon vertices. Will not be edited but is used for lookup.
        /// </summary>
        private IList<Vector3> vertices;

        #endregion

        #region Constructor and Setup

        /// <summary>
        /// Constructor immediately prepares the lists for the triangulation operation.
        /// </summary>
        /// <param name="vertices"></param>
        public PolyTriangulator(IList<Vector3> vertices)
        {
            this.vertices = vertices;
            verts = new LinkedList<Vector3>(vertices);
            convex = new LinkedList<Vector3>();
            reflex = new LinkedList<Vector3>();
            ears = new LinkedList<Vector3>();
            indices = new List<int>();

            PrepareLists();
        }

        /// <summary>
        /// Construct the necessary lists for triangulation with ear clipping.
        /// </summary>
        private void PrepareLists()
        {
            // Get reflex/convex vertices
            var j = vertices.Count - 1;
            var i = j - 1;
            for (int k = 0; k < vertices.Count; k++)
            {
                if (!Maths2D.IsConvex(vertices[i], vertices[j], vertices[k]))
                    reflex.AddLast(vertices[j]);
                else
                    convex.AddLast(vertices[j]);

                i = j;
                j = k;
            }

            // Get ears
            var curr = convex.First;
            var prev = verts.Find(curr.Value).Previous ?? verts.Last;
            var next = verts.Find(curr.Value).Next ?? verts.First;
            while (curr != verts.First)
            {
                if (CheckIfEar(prev.Value, curr.Value, next.Value))
                {
                    ears.AddLast(curr.Value);
                }

                curr = curr.Next;
                if (curr == null)
                    break;
                prev = verts.Find(curr.Value).Previous ?? verts.Last;
                next = verts.Find(curr.Value).Next ?? verts.First;
            }
        }

        #endregion

        #region Triangulation

        /// <summary>
        /// Triangulation of polygon using prepared lists. Removes each ear and updates
        /// neighbouring vertices to their new state for the next iteration until no ears
        /// remain, or 3 vertices remain.
        /// </summary>
        public IList<int> Triangulate()
        {
            var curr = ears.First;
            var prev = verts.Find(curr.Value).Previous ?? verts.Last;
            var next = verts.Find(curr.Value).Next ?? verts.First;
            while (ears.Count > 0 && verts.Count > 3)
            {

                // Update previous vertex as it can change from reflex to convex and become an ear
                var newPrev = prev.Previous ?? verts.Last;
                UpdateVertex(newPrev.Value, prev.Value, next.Value);

                // Update next vertex as it can change from reflex to convex and become an ear
                var newNext = next.Next ?? verts.First;
                UpdateVertex(prev.Value, next.Value, newNext.Value);


                // Remove the ear and add triangle indices
                indices.Add(vertices.IndexOf(next.Value));
                indices.Add(vertices.IndexOf(curr.Value));
                indices.Add(vertices.IndexOf(prev.Value));

                ears.Remove(curr);
                verts.Remove(curr.Value);

                curr = ears.First;
                if (curr == null)
                    break;
                prev = verts.Find(curr.Value).Previous ?? verts.Last;
                next = verts.Find(curr.Value).Next ?? verts.First;

            }

            if (verts.Count == 3)
            {
                curr = verts.First;
                indices.Add(vertices.IndexOf(curr.Next.Next.Value));
                indices.Add(vertices.IndexOf(curr.Next.Value));
                indices.Add(vertices.IndexOf(curr.Value));
            }

            return indices;
        }


        /// <summary>
        /// Update a vertex as it can change from reflex to convex and convex vertices may 
        /// become ears when other ears are removed.
        /// </summary>
        /// <param name="prev">Previous vertex.</param>
        /// <param name="curr">Current vertex.</param>
        /// <param name="next">Next vertex.</param>
        private void UpdateVertex(Vector3 prev, Vector3 curr, Vector3 next)
        {
            // Check if vertex became convex and update if so
            if (reflex.Contains(curr))
            {
                if (Maths2D.IsConvex(prev, curr, next))
                {
                    reflex.Remove(curr);
                    convex.AddLast(curr);
                }
            }
            // Check if the vertex is convex and if so, check if it is an ear.
            if (convex.Contains(curr))
            {
                if (CheckIfEar(prev, curr, next))
                {
                    if (!ears.Contains(curr))
                        ears.AddLast(curr);
                }
                else if (ears.Contains(curr))
                {
                    ears.Remove(curr);
                }
            }
        }

        /// <summary>
        /// Check if a vertex is an ear by iterating over reflex vertices and checking
        /// containment inside triangle formed by vertex and neighbours.
        /// </summary>
        /// <param name="prev">Previous vertex.</param>
        /// <param name="curr">Current vertex.</param>
        /// <param name="next">Next vertex.</param>
        /// <returns>Ear or not.</returns>
        private bool CheckIfEar(Vector3 prev, Vector3 curr, Vector3 next)
        {
            // If any reflex point is inside, it's not an ear.
            foreach (var vertex in reflex)
            {
                if (Maths2D.PointInTriangle(prev.ToXZ(), curr.ToXZ(), next.ToXZ(), vertex.ToXZ()) &&
                    vertex != prev && vertex != curr && vertex != next)
                    return false;
            }
            return true;
        }


        #endregion
    }
}
