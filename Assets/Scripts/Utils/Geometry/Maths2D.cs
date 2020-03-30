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
        private static bool LineSegmentIntersection(
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

        public static bool IsInsideExtremeBounds(Vector2 vertex, IEnumerable<Vector2> vertices)
        {
            return IsInsideExtremeBounds(vertex, GetExtremeBounds(vertices));
        }

        public static bool IsInsideExtremeBounds(
            Vector2 vertex, (float MinX, float MinY, float MaxX, float MaxY) extremeBounds)
        {
            var (minX, minY, maxX, maxY) = extremeBounds;
            var inHorizontalBounds = minX <= vertex.x && vertex.x <= maxX;
            var inVerticalBounds = minY <= vertex.y && vertex.y <= maxY;
            return inHorizontalBounds && inVerticalBounds;
        }
        
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
        /// Returns true if the given vertex is inside the polygon represented by the given vertices.
        /// </summary>
        /// <param name="vertex">The vertex to check if it is inside of the polygon.</param>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>true if the given vertex is inside.</returns>
        public static bool IsInsidePolygon(Vector2 vertex, IReadOnlyCollection<Vector2> vertices)
        {
            if (vertices.Contains(vertex))
                return true;

            // There must be at least 3 vertices in the body's shape
            if (vertices.Count < 3)
                return false;

            // Ray casting:
            
            // Create a ray going through the vertex
            var rayEnd = new Vector2(float.MaxValue, vertex.y);

            // Count intersections of the above line with sides of polygon 
            var rayIntersectionCount = 0;
            var vertexEnumerator = vertices.GetEnumerator();
            vertexEnumerator.MoveNext();
            var first = vertexEnumerator.Current;
            var segmentStart = first;
            while (vertexEnumerator.MoveNext())
            {
                if (LineSegmentsIntersect(vertex, rayEnd, segmentStart, vertexEnumerator.Current, true))
                    rayIntersectionCount++;

                segmentStart = vertexEnumerator.Current;
            }
            vertexEnumerator.Dispose();
            
            if (LineSegmentsIntersect(vertex, rayEnd, segmentStart, first, true))
                rayIntersectionCount++;

            // Return true if count is odd, false otherwise
            return rayIntersectionCount % 2 == 1;
        }

        #endregion
        
    }
}