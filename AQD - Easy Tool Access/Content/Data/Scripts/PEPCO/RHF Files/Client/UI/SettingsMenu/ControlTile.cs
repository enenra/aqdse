using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
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

    /// <summary>
    /// Small collection of terminal controls organized into a single block. No more than 1-3
    /// controls should be added to a tile. If a group of controls can't fit on a tile, then they
    /// will be drawn outside its bounds.
    /// </summary>
    public class ControlTile : IControlTile
    {
        /// <summary>
        /// Read only collection of <see cref="TerminalControlBase"/>s attached to the tile
        /// </summary>
        public IReadOnlyList<TerminalControlBase> Controls { get; }

        public IControlTile ControlContainer => this;

        /// <summary>
        /// Determines whether or not the tile will be rendered in the list.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)ControlTileAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)ControlTileAccessors.Enabled); }
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public object ID => tileMembers.Item3;

        private ApiMemberAccessor GetOrSetMemberFunc => tileMembers.Item1;
        private readonly ControlContainerMembers tileMembers;

        public ControlTile() : this(RichHudTerminal.GetNewMenuTile())
        { }

        public ControlTile(ControlContainerMembers data)
        {
            tileMembers = data;

            var GetControlDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
            Func<int, TerminalControlBase> GetControlFunc = (x => new TerminalControl(GetControlDataFunc(x)));

            Controls = new ReadOnlyApiCollection<TerminalControlBase>(GetControlFunc, data.Item2.Item2);
        }

        IEnumerator<ITerminalControl> IEnumerable<ITerminalControl>.GetEnumerator() =>
            Controls.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Controls.GetEnumerator();

        /// <summary>
        /// Adds a <see cref="TerminalControlBase"/> to the tile
        /// </summary>
        public void Add(TerminalControlBase control) =>
            GetOrSetMemberFunc(control.ID, (int)ControlTileAccessors.AddControl);

        /// <summary>
        /// Retrieves information needed by the Framework API 
        /// </summary>
        public ControlContainerMembers GetApiData() =>
            tileMembers;

        private class TerminalControl : TerminalControlBase
        {
            public TerminalControl(ControlMembers data) : base(data)
            { }
        }
    }
}