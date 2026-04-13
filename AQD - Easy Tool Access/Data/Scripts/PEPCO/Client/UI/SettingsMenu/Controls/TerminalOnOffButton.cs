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
    /// On/Off toggle designed to mimic the appearance of the On/Off button in the SE Terminal.
    /// </summary>
    public class TerminalOnOffButton : TerminalValue<bool>
    {
        public TerminalOnOffButton() : base(MenuControls.OnOffButton)
        { }
    }
}