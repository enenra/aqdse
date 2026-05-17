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
    /// Clickable button. Mimics the appearance of the terminal button in the SE terminal.
    /// </summary>
    public class TerminalButton : TerminalControlBase
    {
        public TerminalButton() : base(MenuControls.TerminalButton)
        { }
    }
}