using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;

namespace Utils
{
    /// <summary>
    /// An enumerator that returns a copy of each element during traversal.
    /// </summary>
    /// <typeparam name="T">The copyable type contained in the enumerator.</typeparam>
    public class CopyableEnumerator<T> : IEnumerator<T> where T : ICopyable<T>
    {
        private readonly IEnumerator<T> _enumerator;
        
        /// <summary>
        /// Bases this enumerator on one from the given enumerable.
        /// </summary>
        /// <param name="enumerable">The enumerable to base this enumerator on.</param>
        public CopyableEnumerator(IEnumerable<T> enumerable) : this(enumerable.GetEnumerator())
        {
        }
        
        /// <summary>
        /// Bases this enumerator on the given one.
        /// </summary>
        /// <param name="enumerator"></param>
        public CopyableEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }
        
        public bool MoveNext() => _enumerator.MoveNext();

        public void Reset() => _enumerator.Reset();

        /// <summary>
        /// Returns a copy of the current and original value from the initially passed enumerator.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public T Current
        {
            get
            {
                if (_enumerator.Current != null) return _enumerator.Current.Copy();
                throw new NullReferenceException("The current enumerator value is null and cannot be copied!");
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() => _enumerator.Dispose();
    }
}