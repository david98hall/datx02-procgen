using UnityEngine;
using static System.Math;

namespace BIAS.Utils.Geometry
{
    /// <summary>
    /// 3D geometry math utility methods.
    /// </summary>
    public static class Maths3D
    {
        private const float tolerance = 0.000001f;
        
        /// <summary>
        /// Returns true if the vertex is on the line segment.
        /// </summary>
        /// <param name="vertex">The vertex to check.</param>
        /// <param name="segmentStart">The start point of the line segment.</param>
        /// <param name="segmentEnd">The end point of the line segment.</param>
        /// <returns>true if the vertex is on the line segment.</returns>
        public static bool OnLineSegment(Vector3 vertex, Vector3 segmentStart, Vector3 segmentEnd)
        {
            return OnLine(vertex, segmentStart, segmentEnd)
                   && Min(segmentStart.x, segmentEnd.x) <= vertex.x && vertex.x <= Max(segmentStart.x, segmentEnd.x)
                   && Min(segmentStart.y, segmentEnd.y) <= vertex.y && vertex.y <= Max(segmentStart.y, segmentEnd.y)
                   && Min(segmentStart.z, segmentEnd.z) <= vertex.z && vertex.z <= Max(segmentStart.z, segmentEnd.z);
        }
        
        /// <summary>
        /// Returns true if the vertex is on the line.
        /// </summary>
        /// <param name="linePoint1">A point on the line.</param>
        /// <param name="linePoint2">Another point on the line.</param>
        /// <param name="vertex">Te vertex to check.</param>
        /// <returns>true if the vertex is on the line.</returns>
        // public static bool OnLine(Vector3 linePoint1, Vector3 linePoint2, Vector3 vertex)
        public static bool OnLine(Vector3 vertex, Vector3 linePoint1, Vector3 linePoint2)
        {
            var lineX = linePoint2.x - linePoint1.x;
            var lineY = linePoint2.y - linePoint1.y;
            var lineZ = linePoint2.z - linePoint1.z;

            if (lineY == 0 && lineZ == 0)
                return vertex.y == 0 && vertex.z == 0;

            if (lineX == 0 && lineZ == 0)
                return vertex.x == 0 && vertex.z == 0;

            if (lineX == 0 && lineY == 0)
                return vertex.x == 0 && vertex.y == 0;

            var xProportion = (vertex.x - linePoint1.x) / lineX;
            var yProportion = (vertex.y - linePoint1.y) / lineY;
            var zProportion = (vertex.z - linePoint1.z) / lineZ;
            return Abs(xProportion - yProportion) < tolerance && Abs(zProportion - yProportion) < tolerance;
        }

        /// <summary>
        /// Returns true and the intersection point if the two line segments intersect.
        /// </summary>
        /// <param name="intersection">The intersection point.</param>
        /// <param name="point1">The starting point of the first line.</param>
        /// <param name="point2">The second point of the first line.</param>
        /// <param name="point3">The starting point of the second line.</param>
        /// <param name="point4">The second point of the second line.</param>
        /// <returns>true if the two line segments intersect, false otherwise.</returns>
        public static bool LineSegmentIntersection(
            out Vector3 intersection,
            Vector3 point1, Vector3 point2,
            Vector3 point3, Vector3 point4)
        {
            var intersectionInDir1 = LineIntersection(out _, point1, point2, point3, point4, true);
            var intersectionInDir2 = 
                LineIntersection(out intersection, point3, point4, point1, point2, true);

            return intersectionInDir1 && intersectionInDir2;
        }

        /// <summary>
        /// Returns true and the intersection point if the two lines intersect.
        /// </summary>
        /// <param name="intersection">The intersection point.</param>
        /// <param name="point1">One point on the first line.</param>
        /// <param name="point2">Another point on the first line.</param>
        /// <param name="point3">One point on the second line.</param>
        /// <param name="point4">Another point on the second line.</param>
        /// <returns>true if the two lines intersect, false otherwise.</returns>
        public static bool LineIntersection(
            out Vector3 intersection,
            Vector3 point1, Vector3 point2,
            Vector3 point3, Vector3 point4)
        {
            var intersectionInDir1 = LineIntersection(out _, point1, point2, point3, point4, false);
            var intersectionInDir2 = 
                LineIntersection(out intersection, point3, point4, point1, point2, false);

            return intersectionInDir1 && intersectionInDir2;
        }
        
        // Direction based
        private static bool LineIntersection(
            out Vector3 intersection, 
            Vector3 point1, Vector3 point2, 
            Vector3 point3, Vector3 point4,
            bool segmentIntersection)
        {
            var relVec1 = point2 - point1;
            var relVec2 = point4 - point3;
            
            var pointLine = point3 - point1;
            var cross1 = Vector3.Cross(relVec1, relVec2);
            var cross2 = Vector3.Cross(pointLine, relVec2);

            var planarFactor = Vector3.Dot(pointLine, cross1);
            if (Mathf.Abs(planarFactor) < tolerance && cross1.sqrMagnitude > tolerance)
            {
                // The lines are in the same plane and are not parallel.
                // Start at point 1 and "go" along vector 1 to the point of intersection
                var scale = Vector3.Dot(cross2, cross1) / cross1.sqrMagnitude;
                
                if (!segmentIntersection || 0 <= scale && scale <= 1)
                {
                    // The intersection is on the line segments and not outside, somewhere else on the lines
                    intersection = point1 + relVec1 * scale;
                    return true;   
                }
            }

            // The lines are not in the same plane, therefore they are not intersecting
            intersection = Vector3.negativeInfinity;
            return false;
        }

        /// <summary>
        /// Finds the perpendicular clockwise vector (normal) to a given vector in the x-z plane.
        /// </summary>
        /// <param name="vector">The vector from which to calculate the normal.</param>
        /// <returns>The normal in the x-z plane to the given vector.</returns>
        public static Vector3 PerpendicularClockwise(Vector3 vector)
        {
            return new Vector3(vector.z, vector.y, -vector.x);
        }
    }
}