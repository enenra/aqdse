using System.Text;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Rendering.Client;
        using Rendering.Server;
        using Rendering;

        /// <summary>
        /// Used to determine text alignment.
        /// </summary>
        public enum TextAlignment : byte
        {
            Left = 0,
            Center = 1,
            Right = 2,
        }

        /// <summary>
        /// Defines the formatting of the characters in rich text types.
        /// </summary>
        public struct GlyphFormat : IEquatable<GlyphFormat>
        {
            public static readonly GlyphFormat 
                Black = new GlyphFormat(),
                White = new GlyphFormat(color: Color.White), 
                Blueish = new GlyphFormat(color: new Color(220, 235, 242)),
                Empty = new GlyphFormat(default(GlyphFormatMembers));

            /// <summary>
            /// Determines the alignment (left, center, right) of a given piece of RichText.
            /// </summary>
            public TextAlignment Alignment => (TextAlignment)Data.Item1;

            /// <summary>
            /// Text size
            /// </summary>
            public float TextSize => Data.Item2;

            /// <summary>
            /// Font specified by the format.
            /// </summary>
            public IFontMin Font => FontManager.GetFont(Data.Item3.X);

            /// <summary>
            /// The font style specifed by the format.
            /// </summary>
            public FontStyles FontStyle => (FontStyles)Data.Item3.Y;

            /// <summary>
            /// The font and style used by the format represented as a pair of integers.
            /// </summary>
            public Vector2I StyleIndex => Data.Item3;

            /// <summary>
            /// Text color
            /// </summary>
            public Color Color => Data.Item4;

            public GlyphFormatMembers Data { get; }

            public GlyphFormat(Color color, TextAlignment alignment, float textSize, Vector2I fontStyle)
            {
                if (color == default(Color))
                    color = Color.Black;

                Data = new GlyphFormatMembers((byte)alignment, textSize, fontStyle, color);
            }

            public GlyphFormat(Color color = default(Color), TextAlignment alignment = TextAlignment.Left, float textSize = 1f, FontStyles style = FontStyles.Regular, IFontMin font = null)
            {
                if (color == default(Color))
                    color = Color.Black;

                if (font == null)
                    font = FontManager.GetFont(FontManager.Default.X);

                Data = new GlyphFormatMembers((byte)alignment, textSize, font.GetStyleIndex(style), color);
            }

            public GlyphFormat(GlyphFormatMembers data)
            {
                this.Data = data;
            }

            public GlyphFormat(GlyphFormat original)
            {
                Data = original.Data;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the specified <see cref="VRageMath.Color"/>.
            /// </summary>
            public GlyphFormat WithColor(Color color) =>
                new GlyphFormat(color, Alignment, TextSize, StyleIndex);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the specified <see cref="TextAlignment"/>.
            /// </summary>
            public GlyphFormat WithAlignment(TextAlignment textAlignment) =>
                new GlyphFormat(Color, textAlignment, TextSize, StyleIndex);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font associated with the given index.
            /// </summary>
            public GlyphFormat WithFont(int font) =>
                new GlyphFormat(Color, Alignment, TextSize, new Vector2I(font, 0));

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given index.
            /// </summary>
            public GlyphFormat WithFont(Vector2I fontStyle) =>
                new GlyphFormat(Color, Alignment, TextSize, fontStyle);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given enum.
            /// </summary>
            public GlyphFormat WithStyle(FontStyles style)
            {
                if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
                    return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, (int)style));
                else
                    return this;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given enum.
            /// </summary>
            public GlyphFormat WithStyle(int style)
            {
                if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
                    return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, style));
                else
                    return this;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the given text size.
            /// </summary>
            public GlyphFormat WithSize(float size) =>
                new GlyphFormat(Color, Alignment, size, StyleIndex);

            /// <summary>
            /// Determines whether or not two given <see cref="GlyphFormat"/>s share the same configuration.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                var format = (GlyphFormat)obj;

                return Data.Item1 == format.Data.Item1
                    && Data.Item2 == format.Data.Item2
                    && Data.Item3 == format.Data.Item3
                    && Data.Item4 == format.Data.Item4;
            }

            /// <summary>
            /// Determines whether or not two given <see cref="GlyphFormat"/>s share the same configuration.
            /// </summary>
            public bool Equals(GlyphFormat format)
            {
                return Data.Item1 == format.Data.Item1
                    && Data.Item2 == format.Data.Item2
                    && Data.Item3 == format.Data.Item3
                    && Data.Item4 == format.Data.Item4;
            }

            public bool DataEqual(GlyphFormatMembers data)
            {
                return Data.Item1 == data.Item1
                    && Data.Item2 == data.Item2
                    && Data.Item3 == data.Item3
                    && Data.Item4 == data.Item4;
            }

            public override int GetHashCode() =>
                Data.GetHashCode();
        }
    }
}