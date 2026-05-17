using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;

        public enum ControlTileAccessors : int
        {
            /// <summary>
            /// out: MemberAccessor
            /// </summary>
            AddControl = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,
        }

        /// <summary>
        /// Small collection of terminal controls organized into a single block. No more than 1-3
        /// controls should be added to a tile. If a group of controls can't fit on a tile, then they
        /// will be drawn outside its bounds.
        /// </summary>
        public interface IControlTile : IEnumerable<ITerminalControl>
        {
            /// <summary>
            /// Read only collection of <see cref="TerminalControlBase"/>s attached to the tile
            /// </summary>
            IReadOnlyList<TerminalControlBase> Controls { get; }

            /// <summary>
            /// Used to allow the addition of controls to tiles using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            IControlTile ControlContainer { get; }

            /// <summary>
            /// Determines whether or not the tile will be rendered in the list.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds a <see cref="TerminalControlBase"/> to the tile
            /// </summary>
            void Add(TerminalControlBase control);

            /// <summary>
            /// Retrieves information needed by the Framework API 
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}