using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    public class ListBoxData<T> : ReadOnlyApiCollection<EntryData<T>>
    {
        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public EntryData<T> Selection 
        {
            get 
            {
                var index = (int)GetOrSetMemberFunc(null, (int)ListBoxAccessors.SelectionIndex);
                return (index != -1) ? this[index] : null;
            }
        }

        /// <summary>
        /// Index of the current selection. -1 if empty.
        /// </summary>
        public int SelectionIndex
        {
            get
            {
                return (int)GetOrSetMemberFunc(null, (int)ListBoxAccessors.SelectionIndex);
            }
        }

        private readonly ApiMemberAccessor GetOrSetMemberFunc;

        public ListBoxData(ApiMemberAccessor GetOrSetMemberFunc) : base(GetListData(GetOrSetMemberFunc))
        {
            this.GetOrSetMemberFunc = GetOrSetMemberFunc;
        }

        private static MyTuple<Func<int, EntryData<T>>, Func<int>> GetListData(ApiMemberAccessor GetOrSetMemberFunc)
        {
            var listData = (CollectionData)GetOrSetMemberFunc(null, (int)ListBoxAccessors.ListMembers);
            Func<int, EntryData<T>> GetEntryFunc = x => new EntryData<T>(listData.Item1(x));

            return new MyTuple<Func<int, EntryData<T>>, Func<int>>()
            {
                Item1 = GetEntryFunc,
                Item2 = listData.Item2
            };
        }

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public void Add(RichText text, T assocObject)
        {
            var data = new MyTuple<List<RichStringMembers>, object>()
            {
                Item1 = text.apiData,
                Item2 = assocObject
            };

            GetOrSetMemberFunc(data, (int)ListBoxAccessors.Add);
        }

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText text, T assocObject)
        {
            var data = new MyTuple<int, List<RichStringMembers>, object>()
            {
                Item1 = index,
                Item2 = text.apiData,
                Item3 = assocObject
            };

            GetOrSetMemberFunc(data, (int)ListBoxAccessors.Insert);
        }

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public bool Remove(EntryData<T> entry) =>
            (bool)GetOrSetMemberFunc(entry.ID, (int)ListBoxAccessors.Remove);

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public void RemoveAt(int index) =>
            GetOrSetMemberFunc(index, (int)ListBoxAccessors.RemoveAt);

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(EntryData<T> entry) =>
            GetOrSetMemberFunc(entry.ID, (int)ListBoxAccessors.Selection);

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember) =>
            GetOrSetMemberFunc(assocMember, (int)ListBoxAccessors.SetSelectionAtData);

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(int index) =>
            GetOrSetMemberFunc(index, (int)ListBoxAccessors.SelectionIndex);
    }

    public class EntryData<T>
    {
        /// <summary>
        /// Name of the list box entry as shown in the UI
        /// </summary>
        public RichText Text
        {
            get { return new RichText(GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.Name) as List<RichStringMembers>); }
            set { GetOrSetMemberFunc(value.apiData, (int)ListBoxEntryAccessors.Name); }
        }

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.Enabled); }
        }

        /// <summary>
        /// Object paired with the entry
        /// </summary>
        public T AssocObject
        {
            get { return (T)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.AssocObject); }
            set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.AssocObject); }
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public object ID => GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.ID);

        private readonly ApiMemberAccessor GetOrSetMemberFunc;

        public EntryData(ApiMemberAccessor GetOrSetMemberFunc)
        {
            this.GetOrSetMemberFunc = GetOrSetMemberFunc;
        }
    }
}