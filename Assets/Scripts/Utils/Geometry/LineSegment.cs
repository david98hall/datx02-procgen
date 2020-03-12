using UnityEngine;

namespace Utils.Geometry
{
    public class LineSegment
    {
        public Vector3 Start
        {
            get => CopyVector(_start);
            set => _start = CopyVector(value);
        }
        private Vector3 _start;
        
        public Vector3 End
        {
            get => CopyVector(_end);
            set => _end = CopyVector(value);
        }
        private Vector3 _end;
        
        public Vector3 RelativeVector => _end - _start;
        
        public Vector3 DirectionVector => RelativeVector.normalized;

        public LineSegment(Vector3 start, Vector3 end)
        {
            Start = _start;
            End = _end;
        }

        public bool IsParallelTo(LineSegment other)
        {
            var thisDirVec = DirectionVector;
            var otherDirVec = other.DirectionVector;
            return thisDirVec.Equals(otherDirVec) || thisDirVec.Equals(-otherDirVec);
        }
        
        private static Vector3 CopyVector(Vector3 v) => new Vector3(v.x, v.y, v.z);
    }
}