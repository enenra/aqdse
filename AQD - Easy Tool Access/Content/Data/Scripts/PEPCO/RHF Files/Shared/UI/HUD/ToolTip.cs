using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>>;

namespace RichHudFramework
{
    namespace UI
    {
        using ToolTipMembers = MyTuple<
            List<RichStringMembers>, // Text
            Color? // BgColor
        >;

        /// <summary>
        /// Class used to define tooltips attached to the RHF cursor. Tooltips must be
        /// registered in HandleInput() every input tick. The first tooltip registered
        /// takes precedence.
        /// </summary>
        public class ToolTip
        {
            public static readonly GlyphFormat DefaultText = GlyphFormat.Blueish.WithSize(.75f);
            public static readonly Color DefaultBG = new Color(73, 86, 95),
                orangeWarningBG = new Color(180, 110, 0),
                redWarningBG = new Color(126, 39, 44);

            /// <summary>
            /// Text to be assigned to the tooltip. Multiline tooltips are allowed, but
            /// are not wrapped.
            /// </summary>
            public RichText text;

            /// <summary>
            /// Color of the text background
            /// </summary>
            public Color? bgColor;

            /// <summary>
            /// Callback delegate used by the API to retrieve tooltip information
            /// </summary>
            public readonly Func<ToolTipMembers> GetToolTipFunc;

            public ToolTip()
            {
                bgColor = DefaultBG;

                GetToolTipFunc = () => new ToolTipMembers()
                {
                    Item1 = text?.apiData,
                    Item2 = bgColor,
                };
            }

            public ToolTip(Func<ToolTipMembers> GetToolTipFunc)
            {
                bgColor = DefaultBG;
                this.GetToolTipFunc = GetToolTipFunc;
            }

            /// <summary>
            /// Implicitly converts <see cref="RichText"/> to <see cref="ToolTip"/>
            /// </summary>
            public static implicit operator ToolTip(RichText text) =>
                new ToolTip() { text = text };

            /// <summary>
            /// Implicitly converts <see cref="string"/> to <see cref="ToolTip"/>
            /// </summary>
            public static implicit operator ToolTip(string text) =>
                new ToolTip() { text = new RichText(text, DefaultText) };
        }
    }
}
