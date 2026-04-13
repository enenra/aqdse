using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using VRage;
using System;

namespace RichHudFramework
{
    /// <summary>
    /// Interface used to define the instantiation/reset behavior of types, T, in 
    /// ObjectPool(T)
    /// </summary>
    public interface IPooledObjectPolicy<T>
    {
        /// <summary>
        /// Instantiates a new object instance of type T
        /// </summary>
        T GetNewObject();

        /// <summary>
        /// Resets the object for later reuse before being added back to the pool
        /// </summary>
        void ResetObject(T obj);

        /// <summary>
        /// Resets the range of objects in the given collection
        /// </summary>
        void ResetRange(IReadOnlyList<T> objects, int index, int count);

        /// <summary>
        /// Resets the range of objects in the given collection
        /// </summary>
        void ResetRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index, int count);
    }

    /// <summary>
    /// Generic pooled object policy using delegates
    /// </summary>
    public class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
    {
        private readonly Func<T> GetNewObjectFunc;
        private readonly Action<T> ResetObjectAction;

        public PooledObjectPolicy(Func<T> GetNewObjectFunc, Action<T> ResetObjectAction)
        {
            if (GetNewObjectFunc == null || ResetObjectAction == null)
                throw new Exception("Neither GetNewObjectFunc nor ResetObjectAction can be null.");
        
            this.GetNewObjectFunc = GetNewObjectFunc;
            this.ResetObjectAction = ResetObjectAction;
        }

        /// <summary>
        /// Instantiates a new object instance of type T
        /// </summary>
        public T GetNewObject()
        {
            return GetNewObjectFunc();
        }

        /// <summary>
        /// Resets the object for later reuse before being added back to the pool
        /// </summary>
        public void ResetObject(T obj)
        {
            ResetObjectAction(obj);
        }

        /// <summary>
        /// Resets the range of objects in the given collection
        /// </summary>
        public void ResetRange(IReadOnlyList<T> objects, int index, int count)
        {
            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
                ResetObjectAction(objects[index + n]);
        }

        /// <summary>
        /// Resets the range of objects in the given collection
        /// </summary>
        public void ResetRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index, int count)
        {
            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
                ResetObjectAction(objects[index + n].Item1);
        }
    }

    /// <summary>
    /// Maintains a pool of reusable reference types. Based on List(T); not thread safe.
    /// </summary>
    public class ObjectPool<T>
    {
        public int Count => pooledObjects.Count;

        public int Capacity => pooledObjects.Capacity;

        protected readonly List<T> pooledObjects;
        protected readonly IPooledObjectPolicy<T> objectPolicy;

        /// <summary>
        /// Creates a new ObjectPool with the given object policy
        /// </summary>
        public ObjectPool(IPooledObjectPolicy<T> objectPolicy)
        {
            if (objectPolicy == null)
                throw new Exception("Pooled object policy cannot be null.");

            pooledObjects = new List<T>();
            this.objectPolicy = objectPolicy;
        }

        /// <summary>
        /// Creates a new ObjectPool with an generic object policy defined by the given delegates
        /// </summary>
        public ObjectPool(Func<T> GetNewObjectFunc, Action<T> ResetObjectAction)
        {
            if (GetNewObjectFunc == null || ResetObjectAction == null)
                throw new Exception("Neither GetNewObjectFunc nor ResetObjectAction can be null.");

            this.pooledObjects = new List<T>();
            this.objectPolicy = new PooledObjectPolicy<T>(GetNewObjectFunc, ResetObjectAction);
        }

        /// <summary>
        /// Removes and returns and object from the pool or creates
        /// a new one if none are available.
        /// </summary>
        public T Get()
        {
            T obj;

            if (pooledObjects.Count > 0)
            {
                int last = pooledObjects.Count - 1;
                obj = pooledObjects[last];
                pooledObjects.RemoveAt(last);
            }
            else
            {
                obj = objectPolicy.GetNewObject();
            }

            return obj;
        }

        /// <summary>
        /// Adds the given object back to the pool for later reuse.
        /// </summary>
        public void Return(T obj)
        {
            objectPolicy.ResetObject(obj);
            pooledObjects.EnsureCapacity(Capacity);
            pooledObjects.Add(obj);
        }

        /// <summary>
        /// Returns the specified range of objects in the collection to the pool.
        /// </summary>
        public void ReturnRange(IReadOnlyList<T> objects, int index = -1, int count = -1)
        {
            if (index == -1)
                index = 0;

            if (count == -1)
                count = objects.Count;

            objectPolicy.ResetRange(objects, index, count);
            pooledObjects.EnsureCapacity(Capacity);

            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
                pooledObjects.Add(objects[index + n]);
        }

        /// <summary>
        /// Returns the specified range of objects in the collection to the pool.
        /// </summary>
        public void ReturnRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index = -1, int count = -1)
        {
            if (index == -1)
                index = 0;

            if (count == -1)
                count = objects.Count;

            objectPolicy.ResetRange(objects, index, count);
            pooledObjects.EnsureCapacity(Capacity);

            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
                pooledObjects.Add(objects[index + n].Item1);
        }

        /// <summary>
        /// Sets the capacity of the pool equal to the number of members
        /// </summary>
        public void TrimExcess()
        {
            pooledObjects.TrimExcess();
        }

        /// <summary>
        /// Clears all objects from the pool
        /// </summary>
        public void Clear()
        {
            pooledObjects.Clear();
        }
    }
}