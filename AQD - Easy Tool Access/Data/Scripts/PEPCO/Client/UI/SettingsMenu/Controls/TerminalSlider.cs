using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public enum SliderSettingsAccessors : int
    {
        /// <summary>
        /// Float
        /// </summary>
        Min = 16,

        /// <summary>
        /// Float
        /// </summary>
        Max = 17,

        /// <summary>
        /// Float
        /// </summary>
        Percent = 18,

        /// <summary>
        /// RichStringMembers[]
        /// </summary>
        ValueText = 19,
    }

    /// <summary>
    /// Labeled slider used to set float values in the settings menu. Mimics the appearance of the slider in the
    /// SE terminal.
    /// </summary>
    public class TerminalSlider : TerminalValue<float>
    {
        /// <summary>
        /// Minimum configurable value for the slider.
        /// </summary>
        public float Min
        {
            get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Min); }
            set { GetOrSetMember(value, (int)SliderSettingsAccessors.Min); }
        }

        /// <summary>
        /// Maximum configurable value for the slider.
        /// </summary>
        public float Max
        {
            get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Max); }
            set { GetOrSetMember(value, (int)SliderSettingsAccessors.Max); }
        }

        /// <summary>
        /// Current slider value expreseed as a percentage between the min and maximum values.
        /// </summary>
        public float Percent
        {
            get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Percent); }
            set { GetOrSetMember(value, (int)SliderSettingsAccessors.Percent); }
        }

        /// <summary>
        /// Text indicating the current value of the slider. Does not automatically reflect changes to the slider value.
        /// </summary>
        public string ValueText
        {
            get { return GetOrSetMember(null, (int)SliderSettingsAccessors.ValueText) as string; }
            set { GetOrSetMember(value, (int)SliderSettingsAccessors.ValueText); }
        }

        public TerminalSlider() : base(MenuControls.SliderSetting)
        { }
    }
}