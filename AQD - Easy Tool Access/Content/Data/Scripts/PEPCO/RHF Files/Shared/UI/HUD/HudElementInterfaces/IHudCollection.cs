using System;
using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection<TElementContainer, TElement> : IReadOnlyList<TElementContainer>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudNodeBase
        {
            /// <summary>
            /// UI elements in the collection
            /// </summary>
            IReadOnlyList<TElementContainer> Collection { get; }

            /// <summary>
            /// Finds the collection member that meets the conditions required by the predicate.
            /// </summary>
            TElementContainer Find(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Finds the index of the collection member that meets the conditions required by the predicate.
            /// </summary>
            int FindIndex(Func<TElementContainer, bool> predicate);
        }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection<TElementContainer> : IReadOnlyHudCollection<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        { }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection : IReadOnlyHudCollection<HudElementContainer<HudElementBase>, HudElementBase>
        { }

        /// <summary>
        /// Interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection<TElementContainer, TElement> : IReadOnlyHudCollection<TElementContainer, TElement>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudNodeBase
        {
            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the collection.
            /// </summary>
            void Add(TElement element, bool preload = false);

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> to the collection.
            /// </summary>
            void Add(TElementContainer element, bool preload = false);

            /// <summary>
            /// Add the given range to the end of the collection.
            /// </summary>
            void AddRange(IReadOnlyList<TElementContainer> newContainers, bool preload = false);

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            void Insert(int index, TElementContainer container, bool preload = false);

            /// <summary>
            /// Insert the given range into the collection.
            /// </summary>
            void InsertRange(int index, IReadOnlyList<TElementContainer> newContainers, bool preload = false);

            /// <summary>
            /// Removes the specified element from the collection.
            /// </summary>
            bool Remove(TElementContainer collectionElement);

            /// <summary>
            /// Removes the collection member that meets the conditions required by the predicate.
            /// </summary>
            bool Remove(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Remove the collection element at the given index.
            /// </summary>
            bool RemoveAt(int index);

            /// <summary>
            /// Removes the specfied range from the collection. Normal child elements not affected.
            /// </summary>
            void RemoveRange(int index, int count);

            /// <summary>
            /// Remove all elements in the collection. Does not affect normal child elements.
            /// </summary>
            void Clear();
        }

        /// <summary>
        /// Interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection<TElementContainer> : IHudCollection<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        { }

        /// <summary>
        /// Interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection : IHudCollection<HudElementContainer<HudElementBase>, HudElementBase>
        { }
    }
}
