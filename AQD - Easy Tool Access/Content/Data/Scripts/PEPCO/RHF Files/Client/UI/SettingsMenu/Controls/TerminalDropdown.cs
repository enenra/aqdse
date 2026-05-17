using System;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    /// <summary>
    /// A dropdown list with a label. Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TerminalDropdown<T> : TerminalValue<EntryData<T>>
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

        public TerminalDropdown() : base(MenuControls.DropdownControl)
        {
            var listData = GetOrSetMember(null, (int)ListControlAccessors.ListAccessors) as ApiMemberAccessor;
            
            List = new ListBoxData<T>(listData);
        }
    }
}