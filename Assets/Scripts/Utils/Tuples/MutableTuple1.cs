using System.Collections.Generic;

namespace Utils.Tuples
{
    public class MutableTuple<T1, T2, T3> : MutableTuple<T1, T2>
    {
        public T2 Item3 { get; set; }
        
        public MutableTuple(T1 item1, T2 item2, T2 item3) : base(item1, item2)
        {
        }

        protected bool Equals(MutableTuple<T1, T2, T3> other)
        {
            return base.Equals(other) && EqualityComparer<T2>.Default.Equals(Item3, other.Item3);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MutableTuple<T1, T2, T3>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ EqualityComparer<T2>.Default.GetHashCode(Item3);
            }
        }
    }
}