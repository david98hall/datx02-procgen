﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Extensions
{
    /// <summary>
    /// Extensions for classes implementing ICollection or ICollection&lt;T&gt;.
    /// </summary>
    public static class CollectionExtensions   
    {
        
        /// <summary>
        /// Appends elements to the end of the collection.
        /// </summary>
        /// <param name="source">The source collection to append to.</param>
        /// <param name="items">The items to append.</param>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            AddRange(source, items, t => t);
        }

        /// <summary>
        /// Adds element to the collection.
        /// </summary>
        /// <param name="source">The source collection to add to.</param>
        /// <param name="items">The items to add.</param>
        /// <param name="func">The function to apply to each element when adding it to the source collection.</param>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items, Func<T,T> func)
        {
            foreach (var item in items)
            {
                source.Add(func(item));
            }
        }
        
        /// <summary>
        /// Adds elements to the front of the linked list.
        /// </summary>
        /// <param name="source">The source collection to prepend to.</param>
        /// <param name="items">The elements to add.</param>
        /// <typeparam name="T">The element type of the list.</typeparam>
        public static void PrependRange<T>(this LinkedList<T> source, IEnumerable<T> items)
        {
            PrependRange(source, items, t => t);
        }
        
        /// <summary>
        /// Adds elements to the front of the linked list.
        /// </summary>
        /// <param name="source">The source collection to prepend to.</param>
        /// <param name="items">The elements to add.</param>
        /// <param name="func">The function to apply to each element when adding it to the source collection.</param>
        /// <typeparam name="T">The element type of the list.</typeparam>
        public static void PrependRange<T>(this LinkedList<T> source, IEnumerable<T> items, Func<T,T> func)
        {
            var first = source.First;
            foreach (var item in items)
            {
                source.AddBefore(first, func(item));
            }
        }
    }
}