using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    public enum ListControlAccessors : int
    {
        ListAccessors = 16,
    }

    /// <summary>
    /// A fixed size list box with a label. Designed to mimic the appearance of the list box in the SE terminal.
    /// </summary>
    public class TerminalList<T> : TerminalValue<EntryData<T>>
    {
        /// <summary>
        /// Currently selected list member.
        /// </summary>
        public override EntryData<T> Value
        {
            get { return List.Selection; }
            set { List.SetSelection(value); }
        }

        public ListBoxData<T> List { get; }

        public TerminalList() : base(MenuControls.ListControl)
        {
            var listData = GetOrSetMember(null, (int)ListControlAccessors.ListAccessors) as ApiMemberAccessor;

            List = new ListBoxData<T>(listData);
        }
    }
}