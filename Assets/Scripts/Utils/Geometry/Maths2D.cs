using System;
using Extensions;
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
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }

        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r > 0 && r < 1) && (s > 0 && s < 1);
        }

        public static bool LineSegmentIntersection(
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

        /// <summary>
        /// Returns the angle in degrees between the two points.
        /// The angle is always zero if the points are on the same x or y level.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The angle in degrees between the two points</returns>
        public static float GetDegreesBetween(Vector2 point1, Vector2 point2)
        {
            return (float) (GetRadiansBetween(point1, point2) * 180 / Math.PI);
        }
        
        /// <summary>
        /// Returns the angle in radians between the two points.
        /// The angle is always zero if the points are on the same x or y level.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The angle in radians between the two points</returns>
        public static float GetRadiansBetween(Vector2 point1, Vector2 point2)
        {
            var deltaX = point2.x - point1.x;

            if (deltaX == 0)
                return 0;
            
            return (float) Math.Atan((point2.y - point1.y) / deltaX);
        }

    }
}