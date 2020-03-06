using System.Collections.Generic;

namespace Extensions
{
    public static class LinkedListExtensions   
    {
        public static void AppendRange<T>(this LinkedList<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                source.AddLast(item);
            }
        }

        public static void PrependRange<T>(this LinkedList<T> source, IEnumerable<T> items)
        {
            var first = source.First;
            foreach (var item in items)
            {
                source.AddBefore(first, item);
            }
        }
    }
}