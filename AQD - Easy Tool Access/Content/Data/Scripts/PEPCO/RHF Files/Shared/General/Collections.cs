using System.Collections;
using System.Collections.Generic;
using VRage;
using System;

namespace RichHudFramework
{
    /// <summary>
    /// Interface for collections with an indexer and a count property.
    /// </summary>
    public interface IIndexedCollection<T>
    {
        /// <summary>
        /// Returns the element associated with the given index.
        /// </summary>
        T this[int index] { get; }

        /// <summary>
        /// The number of elements in the collection
        /// </summary>
        int Count { get; }
    }

    /// <summary>
    /// Generic enumerator using delegates.
    /// </summary>
    public class CollectionDataEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// Returns the element at the enumerator's current position
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Returns the element at the enumerator's current position
        /// </summary>
        public T Current => Getter(index);

        protected readonly Func<int, T> Getter;
        protected readonly Func<int> CountFunc;
        protected int index;

        public CollectionDataEnumerator(Func<int, T> Getter, Func<int> CountFunc)
        {
            this.Getter = Getter;
            this.CountFunc = CountFunc;
            index = -1;
        }

        public void Dispose()
        { }

        public bool MoveNext()
        {
            index++;
            return index < CountFunc();
        }

        public void Reset()
        {
            index = -1;
        }
    }
}