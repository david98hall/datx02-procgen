using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Geometry
{
    public static class Maths2D
    {

        public static float PseudoDistanceFromPointToLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return (int)Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        public static int SideOfLine(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return (int)Mathf.Sign((cx - ax) * (-by + ay) + (cy - ay) * (bx - ax));
        }

        public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            var area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            var s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            var t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }

        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, bool considerSegmentExtremes = false)
        {
            var denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            var numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            var numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            var r = numerator1 / denominator;
            var s = numerator2 / denominator;

            return considerSegmentExtremes 
                ? (0 <= r && r <= 1) && (0 <= s && s <= 1)
                : (0 < r && r < 1) && (0 < s && s < 1);
        }

        // TODO Not 100% that this works. Make it public if you're sure.
        /// <summary>
        /// Tries to get an intersection point of two line segments.
        /// </summary>
        /// <param name="intersection">The found intersection point.</param>
        /// <param name="start1">The start of line segment 1.</param>
        /// <param name="end1">The end of line segment 1.</param>
        /// <param name="start2">The start of line segment 2.</param>
        /// <param name="end2">The end of line segment 1.</param>
        /// <returns>true if an intersection is found.</returns>
        private static bool TryGetLineSegmentIntersection(
            out Vector2 intersection, Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
        {
            var dirVec1 = end1 - start1;
            var dirVec2 = end2 - start2;

            // Solve equation system
            // start1.x + t * dirVec1.x = start2.x + s * dirVec2.x
            // start1.y + t * dirVec1.y = start2.y + s * dirVec2.y
            // Used to get rid of one of the variables
            var elimination = dirVec1.x / dirVec1.y;
            var s = (start1.x - start2.x - elimination * (start1.y - start2.y)) / (dirVec2.x - dirVec2.y * elimination);
            var t = (start2.x + s * dirVec2.x - start1.x) / dirVec1.x;

            if (0 <= t && t <= 1)
            {
                intersection = start1 + t * dirVec1;
                return true;  
            }

            intersection = Vector2.negativeInfinity;
            return false;
        }

        #region Polygon

        /// <summary>
        /// Returns true if the vertex is within the extreme bounds.
        /// Here, extreme bounds means minimum and maximum x and y coordinates.
        /// </summary>
        /// <param name="vertex">The vertex to check if it's inside the bounds.</param>
        /// <param name="vertices">The vertices to get the extreme bounds from.</param>
        /// <returns>true if the vertex is within the extreme bounds.</returns>
        public static bool IsInsideExtremeBounds(Vector2 vertex, IEnumerable<Vector2> vertices)
        {
            return IsInsideExtremeBounds(vertex, GetExtremeBounds(vertices));
        }

        /// <summary>
        /// Returns true if the vertex is within the extreme bounds.
        /// Here, extreme bounds means minimum and maximum x and y coordinates.
        /// </summary>
        /// <param name="vertex">The vertex to check if it's inside the bounds.</param>
        /// <param name="extremeBounds">The bounds.</param>
        /// <returns>true if the vertex is within the extreme bounds.</returns>
        public static bool IsInsideExtremeBounds(
            Vector2 vertex, (float MinX, float MinY, float MaxX, float MaxY) extremeBounds)
        {
            var (minX, minY, maxX, maxY) = extremeBounds;
            var inHorizontalBounds = minX <= vertex.x && vertex.x <= maxX;
            var inVerticalBounds = minY <= vertex.y && vertex.y <= maxY;
            return inHorizontalBounds && inVerticalBounds;
        }
        
        /// <summary>
        /// Gets the extreme bounds of the given vertices.
        /// Here, extreme bounds means minimum and maximum x and y coordinates.
        /// </summary>
        /// <param name="vertices">The vertices to get the bounds from.</param>
        /// <returns>The extreme bounds.</returns>
        public static (float MinX, float MinY, float MaxX, float MaxY) GetExtremeBounds(IEnumerable<Vector2> vertices)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            foreach (var polygonVertex in vertices)
            {
                if (polygonVertex.x < minX)
                    minX = polygonVertex.x;
                else if (polygonVertex.x > maxX)
                    maxX = polygonVertex.x;
                
                if (polygonVertex.y < minY)
                    minY = polygonVertex.y;
                else if (polygonVertex.y > maxY)
                    maxY = polygonVertex.y;
            }

            return (minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Gets the center point based on the extreme bounds of the given vertices.
        /// </summary>
        /// <param name="vertices">The vertices to get the center point from.</param>
        /// <returns>The center point.</returns>
        public static Vector2 GetCenterPoint(IEnumerable<Vector2> vertices)
        {
            var (minX, minY, maxX, maxY) = GetExtremeBounds(vertices);
            return new Vector2(minX, minY) + new Vector2(maxX - minX, maxY - minY) / 2;
        }

        /// <summary>
        /// Returns true if the given vertex is inside the polygon represented by the given vertices.
        /// </summary>
        /// <param name="vertex">The vertex to check if it is inside of the polygon.</param>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>true if the given vertex is inside.</returns>
        public static bool IsInsidePolygon(Vector2 vertex, IEnumerable<Vector2> vertices)
        {
            var verticesList = vertices.ToList();
            
            if (verticesList.Contains(vertex))
                return true;
            
            // There must be at least 3 vertices in the body's shape.
            // For the vertex to be inside the polygon, it can't be outside its extreme bounds.
            var extremeBounds = GetExtremeBounds(verticesList);
            if (verticesList.Count < 3 && !IsInsideExtremeBounds(vertex, extremeBounds))
                return false;

            // Return true if the ray cast intersection count is odd, false otherwise
            var rayCastIntersection = RayCastIntersections(
                vertex,
                new Vector2(extremeBounds.MaxX, vertex.y), 
                verticesList);
            return rayCastIntersection.Count() % 2 == 1;
        }

        /// <summary>
        /// Returns true if any center point in the first polygon is inside the second.
        /// This is useful in cases where overlaps are searched for but edge intersections do not count.
        /// </summary>
        /// <param name="polygon1">The first polygon.</param>
        /// <param name="polygon2">The second polygon.</param>
        /// <returns>true if any center point in the first polygon is inside the second.</returns>
        public static bool AnyPolygonCenterOverlap(IEnumerable<Vector2> polygon1, IEnumerable<Vector2> polygon2)
        {
            // Use ray casting to find center points within the first polygon
            const float raySpacing = 0.2f;
            var rayCastCenters = GetRayCastPolygonCenters(polygon1, raySpacing);
            
            // If any center point in within the second polygon, the polygons overlap
            return rayCastCenters.Any(centerPoint => IsInsidePolygon(centerPoint, polygon2));
        }
        
        /// <summary>
        /// Gets all intersection points along the ray cast of the specified coordinates.
        /// </summary>
        /// <param name="rayStart">The start of the ray.</param>
        /// <param name="rayEnd">The end of the ray.</param>
        /// <param name="vertices">The vertices, which edges will be checked if they intersect with the ray.</param>
        /// <returns>All ray/edge intersections.</returns>
        public static IEnumerable<Vector2> RayCastIntersections(Vector2 rayStart, Vector2 rayEnd, IEnumerable<Vector2> vertices)
        {
            var intersections = new LinkedList<Vector2>();

            // Traverse all edges based on the vertices' order
            var vertexEnumerator = vertices.GetEnumerator();
            vertexEnumerator.MoveNext();
            var first = vertexEnumerator.Current;
            var segmentStart = first;
            while (vertexEnumerator.MoveNext())
            {
                // If there is an intersection, add it to the result
                AddIfIntersection(vertexEnumerator.Current);
                segmentStart = vertexEnumerator.Current;
            }
            vertexEnumerator.Dispose();
            AddIfIntersection(first);
            
            // A method for checking if there is in fact an intersection and if so, adding it to the result
            void AddIfIntersection(Vector2 segmentEnd)
            {
                // Vector conversion methods
                Vector3 Vec2ToVec3(Vector2 v) => new Vector3(v.x, 0, v.y);
                Vector2 Vec3ToVec2(Vector3 v) => new Vector2(v.x, v.z);
                
                // Check if the ray and the line segment (edge) intersect
                if (Maths3D.LineSegmentIntersection(
                    out var intersection,
                    Vec2ToVec3(rayStart), Vec2ToVec3(rayEnd), 
                    Vec2ToVec3(segmentStart), Vec2ToVec3(segmentEnd)))
                {
                    // If the ray and the edge intersect; add it to the result
                    intersections.AddLast(Vec3ToVec2(intersection));
                }
            }

            // Return all ray/edge intersections
            return intersections;
        }
        
        /// <summary>
        /// Uses ray casting to get all "center points" within a polygon. These points will form a path within the
        /// center of the polygon, which is what this method returns.
        /// </summary>
        /// <param name="polygonVertices">The vertices of the polygon.</param>
        /// <param name="step">The y-distance of each ray cast.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws if step is less than 1.</exception>
        public static IEnumerable<Vector2> GetRayCastPolygonCenters(IEnumerable<Vector2> polygonVertices, float step)
        {
            // If the step is negative, the ray casting will be done in the wrong direction and if it is zero,
            // the program will freeze.
            if (step <= 0)
                throw new ArgumentException("The step argument has to larger than zero!");

            var castCenters = new LinkedList<Vector2>();
            
            // Move from the minimum to the maximum y-coordinate of the
            // polygon's extreme bounds and create a ray cast every step.
            var vertices = polygonVertices.ToList();
            var (minX, minY, maxX, maxY) = GetExtremeBounds(vertices);
            for (var y = minY + step; y <= maxY - step; y += step)
            {
                // Find all ray intersections with the polygon's edges
                var rayStart = new Vector2(minX, y);
                var rayEnd = new Vector2(maxX, y);
                var intersections = 
                    RayCastIntersections(rayStart, rayEnd,vertices).GetEnumerator();
                if (!intersections.MoveNext())
                    continue;

                // If there are any intersections, traverse each pair and add the center vertex
                // between its vertices to the result
                var intersectionStart = intersections.Current;
                while (intersections.MoveNext())
                {
                    // Add the center vertex between the first and second vertex in the intersection pair
                    castCenters.AddLast(intersectionStart + (intersections.Current - intersectionStart) / 2);
                    intersectionStart = intersections.Current;
                }
                intersections.Dispose();
                
            }

            // Return all "center points" found when ray casting
            // for each step when moving from minimum to maximum y
            return castCenters;
        }
        
        #endregion
        
    }
}