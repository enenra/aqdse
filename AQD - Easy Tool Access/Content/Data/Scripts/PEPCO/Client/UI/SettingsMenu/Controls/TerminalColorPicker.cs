using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// An RGB color picker using sliders for each channel. Designed to mimic the appearance of the color picker
    /// in the SE terminal.
    /// </summary>
    public class TerminalColorPicker : TerminalValue<Color>
    {
        public TerminalColorPicker() : base(MenuControls.ColorPicker)
        { }
    }
}