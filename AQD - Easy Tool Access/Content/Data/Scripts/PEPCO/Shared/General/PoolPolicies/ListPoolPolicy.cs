using System.Collections.Generic;
using VRage;

namespace RichHudFramework
{
    /// <summary>
    /// Pool policy for use with generic lists.
    /// </summary>
    public class ListPoolPolicy<T> : IPooledObjectPolicy<List<T>>
    {
        public List<T> GetNewObject()
        {
            return new List<T>();
        }

        public void ResetObject(List<T> list)
        {
            list.Clear();
        }

        public void ResetRange(IReadOnlyList<List<T>> lists, int index, int count)
        {
            for (int i = 0; (i < count && (index + i) < lists.Count); i++)
                lists[index + i].Clear();
        }

        public void ResetRange<T2>(IReadOnlyList<MyTuple<List<T>, T2>> lists, int index, int count)
        {
            for (int i = 0; (i < count && (index + i) < lists.Count); i++)
                lists[index + i].Item1.Clear();
        }

        /// <summary>
        /// Returns a new <see cref="ObjectPool{T}"/> using <see cref="ListPoolPolicy{T}"/>
        /// </summary>
        public static ObjectPool<List<T>> GetNewPool()
        {
            return new ObjectPool<List<T>>(new ListPoolPolicy<T>());
        }
    }
}