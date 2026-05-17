using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Contains colors and text formatting used in styling the Rich HUD Terminal
    /// </summary>
    public static class TerminalFormatting
    {
        public static readonly GlyphFormat HeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1.15f);
        public static readonly GlyphFormat ControlFormat = GlyphFormat.Blueish.WithSize(1.08f);
        public static readonly GlyphFormat InvControlFormat = ControlFormat.WithColor(Charcoal);
        public static readonly GlyphFormat WarningFormat = new GlyphFormat(new Color(200, 55, 55));

        /// <summary>
        /// A dark blue grey commonly used for background colors for buttons and lists in the SE
        /// terminal.
        /// </summary>
        public static readonly Color OuterSpace = new Color(42, 55, 62);

        /// <summary>
        /// Dark slate grey used for list backgrounds
        /// </summary>
        public static readonly Color DarkSlateGrey = new Color(41, 54, 62);

        public static readonly Color Gunmetal = new Color(39, 50, 57);

        public static readonly Color Dark = new Color(32, 39, 45);

        /// <summary>
        /// Darker medium blue-grey used for borders around buttons, sliders, etc in the SE terminal.
        /// </summary>
        public static readonly Color LimedSpruce = new Color(61, 70, 78);

        /// <summary>
        /// Dark medium grey used for background highlights for moused-over elements
        /// </summary>
        public static readonly Color Atomic = new Color(60, 76, 82);

        /// <summary>
        /// Medium blue-grey used for scroll bar slider buttons
        /// </summary>
        public static readonly Color MistBlue = new Color(103, 109, 123);

        /// <summary>
        /// Medium blue-grey used for tick boxes
        /// </summary>
        public static readonly Color StormGrey = new Color(114, 121, 136);

        /// <summary>
        /// Darker medium blue-grey used for scroll bars (the actual bar, not the slider)
        /// </summary>
        public static readonly Color MidGrey = new Color(86, 93, 104);

        public static readonly Color EbonyClay = new Color(34, 44, 53);

        /// <summary>
        /// White with a hint of blue. Used for selection tabs in the SE terminal.
        /// </summary>
        public static readonly Color Mercury = new Color(225, 230, 236);

        /// <summary>
        /// A very dark blue-grey used for sliders that have input focus
        /// </summary>
        public static readonly Color BlackPerl = new Color(29, 37, 40);

        /// <summary>
        /// A slightly-different very dark blue-grey used for sliders and check boxes that have input focus
        /// </summary>
        public static readonly Color Cinder = new Color(33, 41, 45);

        /// <summary>
        /// Dark slightly-blue color used for buttons that have focus (were last clicked). Used in combination in SE with a 
        /// bright blue background.
        /// </summary>
        public static readonly Color Charcoal = new Color(39, 49, 55);

        /// <summary>
        /// Bright blue background use for controls that have focus (last clicked). Usually used in combinatination in SE
        /// with Charcoal as the text color.
        /// </summary>
        public static readonly Color Mint = new Color(142, 188, 206);

        /// <summary>
        /// A light blue-grey used to indicate selections for things like on/off buttons in the SE terminal.
        /// </summary>
        public static readonly Color DullMint = new Color(91, 115, 123);
    }
}
