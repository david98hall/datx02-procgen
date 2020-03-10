using UnityEngine;

namespace Utils.Geometry
{
    /// <summary>
    /// 3D math utility methods.
    /// </summary>
    public static class Maths3D
    {
        /// <summary>
        /// Returns true and the intersection point if the two lines intersect.
        /// </summary>
        /// <param name="intersection">The intersection point.</param>
        /// <param name="point1">The starting point of the first line.</param>
        /// <param name="point2">The second point of the first line.</param>
        /// <param name="point3">The starting point of the second line.</param>
        /// <param name="point4">The second point of the second line.</param>
        /// <returns>true if the two lines intersect, false otherwise.</returns>
        public static bool Intersection(
            out Vector3 intersection, 
            Vector3 point1, Vector3 point2, 
            Vector3 point3, Vector3 point4)
        {
            var relVec1 = point2 - point1;
            var relVec2 = point4 - point3;
            
            var pointLine = point3 - point1;
            var cross1 = Vector3.Cross(relVec1, relVec2);
            var cross2 = Vector3.Cross(pointLine, relVec2);

            var planarFactor = Vector3.Dot(pointLine, cross1);
            const float epsilon = 0.0001f;
            if (Mathf.Abs(planarFactor) < epsilon && cross1.sqrMagnitude > epsilon)
            {
                // The lines are in the same plane and are not parallel.
                // Start at point 1 and "go" along vector 1 to the point of intersection
                var scale = Vector3.Dot(cross2, cross1) / cross1.sqrMagnitude;
                intersection = point1 + relVec1 * scale;
                return true;
            }

            // The lines are not in the same plane, therefore they are not intersecting
            intersection = Vector3.negativeInfinity;
            return false;
        }
        
    }
}