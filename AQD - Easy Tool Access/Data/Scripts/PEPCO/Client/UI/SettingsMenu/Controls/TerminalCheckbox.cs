using System;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// Labeled checkbox designed to mimic the appearance of checkboxes in the SE terminal.
    /// </summary>
    public class TerminalCheckbox : TerminalValue<bool>
    {
        public TerminalCheckbox() : base(MenuControls.Checkbox)
        { }
    }
}