using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;

    namespace UI.Client
    {
        /// <summary>
        /// Horizontally scrolling list of control tiles.
        /// </summary>
        public class ControlCategory : IControlCategory
        {
            /// <summary>
            /// Category name
            /// </summary>
            public string HeaderText
            {
                get { return GetOrSetMemberFunc(null, (int)ControlCatAccessors.HeaderText) as string; }
                set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.HeaderText); }
            }

            /// <summary>
            /// Category description
            /// </summary>
            public string SubheaderText
            {
                get { return GetOrSetMemberFunc(null, (int)ControlCatAccessors.SubheaderText) as string; }
                set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.SubheaderText); }
            }

            /// <summary>
            /// Read only collection of <see cref="ControlTile"/>s assigned to this category
            /// </summary>
            public IReadOnlyList<ControlTile> Tiles { get; }

            public IControlCategory TileContainer => this;

            /// <summary>
            /// Unique identifier
            /// </summary>
            public object ID => data.Item3;

            /// <summary>
            /// Determines whether or not the element will be drawn.
            /// </summary>
            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)ControlCatAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.Enabled); }
            }

            private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            private readonly ControlContainerMembers data;

            public ControlCategory() : this(RichHudTerminal.GetNewMenuCategory())
            { }

            public ControlCategory(ControlContainerMembers data)
            {
                this.data = data;

                var GetTileDataFunc = data.Item2.Item1 as Func<int, ControlContainerMembers>;
                Func<int, ControlTile> GetTileFunc = x => new ControlTile(GetTileDataFunc(x));

                Tiles = new ReadOnlyApiCollection<ControlTile>(GetTileFunc, data.Item2.Item2);
            }

            IEnumerator<ControlTile> IEnumerable<ControlTile>.GetEnumerator() =>
                Tiles.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Tiles.GetEnumerator();

            /// <summary>
            /// Adds a <see cref="IControlTile"/> to the category
            /// </summary>
            public void Add(ControlTile tile) =>
                GetOrSetMemberFunc(tile.ID, (int)ControlCatAccessors.AddMember);

            public ControlContainerMembers GetApiData() =>
                data;
        }
    }
}