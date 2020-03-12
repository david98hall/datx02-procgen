using System;
using Extensions;
using UnityEngine;

namespace Utils.Geometry
{
    /// <summary>
    /// 3D math utility methods.
    /// </summary>
    public static class Maths3D
    {

        public static bool LineSegmentIntersection(
            out Vector3 intersection, 
            Vector3 start1, Vector3 end1, 
            Vector3 start2, Vector3 end2)
        {
            var dirVec1 = end1 - start1;
            var dirVec2 = end2 - start2;

            // Solve equation system
            // start1.x + t * dirVec1.x = start2.x + s * dirVec2.x
            // start1.y + t * dirVec1.y = start2.y + s * dirVec2.y
            // start1.z + t * dirVec1.z = start2.z + s * dirVec2.z
            // Used to get rid of one of the variables
            
            float t, lhs, rhs;
            if (!float.IsInfinity(dirVec1.x / dirVec1.y))
            {
                var elimination = dirVec1.x / dirVec1.y;
                var s = (start1.x - start2.x - elimination * (start1.y - start2.y)) / (dirVec2.x - dirVec2.y * elimination);
                t = (start2.x + s * dirVec2.x - start1.x) / dirVec1.x;   
                
                lhs = start1.z + t * dirVec1.z;
                rhs = start2.z + s * dirVec2.z;
            }
            else if (!float.IsInfinity(dirVec1.x / dirVec1.z))
            {
                var elimination = dirVec1.x / dirVec1.z;
                var s = (start1.x - start2.x - elimination * (start1.z - start2.z)) / (dirVec2.x - dirVec2.z * elimination);
                t = (start2.x + s * dirVec2.x - start1.x) / dirVec1.x;   
                
                lhs = start1.y + t * dirVec1.y;
                rhs = start2.y + s * dirVec2.y;
            }
            else
            {
                var elimination = dirVec1.y / dirVec1.z;
                var s = (start1.y - start2.y - elimination * (start1.z - start2.z)) / (dirVec2.y - dirVec2.z * elimination);
                t = (start2.y + s * dirVec2.y - start1.y) / dirVec1.y;   
                
                lhs = start1.x + t * dirVec1.x;
                rhs = start2.x + s * dirVec2.x;
            }

            if (Math.Abs(lhs - rhs) < 0.01f)
            {
                intersection = start1 + t * dirVec1;
                return true;  
            }
            
            intersection = Vector2.negativeInfinity;
            return false;
        }
        
        /*
        public static bool LineSegmentIntersection(
            out Vector3 intersection, 
            Vector3 point1, Vector3 point2, 
            Vector3 point3, Vector3 point4)
        {
            if (Maths2D.LineSegmentIntersection(
                out var intersectionXZ,
                point1.ToXZ(),
                point2.ToXZ(),
                point3.ToXZ(),
                point4.ToXZ()
                ))
            {
                var largestY = Math.Abs(point1.y) > Math.Abs(point2.y) ? point1.y : point2.y;
                var normal = new Vector2(intersectionXZ.x, largestY);

                if (Maths2D.LineSegmentIntersection(
                    out var intersectionXY,
                    intersectionXZ, normal,
                    point3.ToXY(),
                    point4.ToXY()
                    ))
                {

                    intersection = new Vector3(intersectionXZ.x, intersectionXY.y, intersectionXZ.y);
                    return true;
                }
            }

            intersection = Vector3.negativeInfinity;
            return false;
        }
        */

    }
}