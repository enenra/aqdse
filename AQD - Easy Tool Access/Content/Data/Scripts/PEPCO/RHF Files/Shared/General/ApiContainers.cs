using System.Collections;
using System.Collections.Generic;
using VRage;
using VRageMath;
using System;

namespace RichHudFramework
{
    /// <summary>
    /// Read-only collection of cached and indexed RHF API wrappers
    /// </summary>
    public class ReadOnlyApiCollection<TValue> : IReadOnlyList<TValue>, IIndexedCollection<TValue>
    {
        /// <summary>
        /// Returns the element at the given index.
        /// </summary>
        public virtual TValue this[int index]
        {
            get 
            {
                int count = GetCountFunc();

                if (index >= count)
                    throw new Exception($"Index ({index}) was out of Range. Must be non-negative and less than {count}.");

                while (wrapperList.Count < count)
                {
                    for (int n = wrapperList.Count; wrapperList.Count < count; n++)
                        wrapperList.Add(GetNewWrapperFunc(n));
                }

                if (count > 9 && wrapperList.Count > count * 3)
                {
                    wrapperList.RemoveRange(count, wrapperList.Count - count);
                    wrapperList.TrimExcess();
                }

                return wrapperList[index];
            }
        }

        /// <summary>
        /// Returns the number of elements in the collection
        /// </summary>
        public virtual int Count => GetCountFunc();

        protected readonly Func<int, TValue> GetNewWrapperFunc;
        protected readonly Func<int> GetCountFunc;
        protected readonly List<TValue> wrapperList;
        protected readonly CollectionDataEnumerator<TValue> enumerator;

        public ReadOnlyApiCollection(Func<int, TValue> GetNewWrapper, Func<int> GetCount)
        {
            this.GetNewWrapperFunc = GetNewWrapper;
            this.GetCountFunc = GetCount;

            wrapperList = new List<TValue>();
            enumerator = new CollectionDataEnumerator<TValue>(x => this[x], GetCount);
        }

        public ReadOnlyApiCollection(MyTuple<Func<int, TValue>, Func<int>> tuple)
            : this(tuple.Item1, tuple.Item2)
        { }

        public virtual IEnumerator<TValue> GetEnumerator() =>
            enumerator;

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    /// <summary>
    /// Read-only collection backed by delegates
    /// </summary>
    public class ReadOnlyCollectionData<TValue> : IReadOnlyList<TValue>, IIndexedCollection<TValue>
    {
        /// <summary>
        /// Returns the element at the given index.
        /// </summary>
        public virtual TValue this[int index] => Getter(index);

        /// <summary>
        /// Returns the number of elements in the collection
        /// </summary>
        public virtual int Count => GetCountFunc();

        protected readonly Func<int, TValue> Getter;
        protected readonly Func<int> GetCountFunc;
        protected readonly CollectionDataEnumerator<TValue> enumerator;

        public ReadOnlyCollectionData(Func<int, TValue> Getter, Func<int> GetCount)
        {
            this.Getter = Getter;
            this.GetCountFunc = GetCount;
            enumerator = new CollectionDataEnumerator<TValue>(x => this[x], GetCount);
        }

        public ReadOnlyCollectionData(MyTuple<Func<int, TValue>, Func<int>> tuple)
            : this(tuple.Item1, tuple.Item2)
        { }

        public virtual IEnumerator<TValue> GetEnumerator() =>
            enumerator;

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}