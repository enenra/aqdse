using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using AtlasMembers = VRage.MyTuple<string, VRageMath.Vector2>;
using GlyphMembers = VRage.MyTuple<int, VRageMath.Vector2, VRageMath.Vector2, float, float>;

namespace RichHudFramework
{
    using FontMembers = MyTuple<
        string, // Name
        int, // Index
        float, // PtSize
        float, // BaseScale
        Func<int, bool>, // IsStyleDefined
        ApiMemberAccessor
    >;

    namespace UI
    {
        namespace Rendering.Client
        {
            public sealed partial class FontManager
            {
                private class FontData : IFontMin
                {
                    /// <summary>
                    /// Font name
                    /// </summary>
                    public string Name { get; }

                    /// <summary>
                    /// Index of the font in the font manager
                    /// </summary>
                    public int Index { get; }

                    /// <summary>
                    /// Font size at which the textures were created.
                    /// </summary>
                    public float PtSize { get; }

                    /// <summary>
                    /// Default scaling applied to font. Used to normalize font size.
                    /// </summary>
                    public float BaseScale { get; }

                    /// <summary>
                    /// Returns the index for this font using regular styling
                    /// </summary>
                    public Vector2I Regular => new Vector2I(Index, 0);

                    /// <summary>
                    /// Returns the index for the bolded version of this font
                    /// </summary>
                    public Vector2I Bold => new Vector2I(Index, 1);

                    /// <summary>
                    /// Returns the index for the italicised version of this font
                    /// </summary>
                    public Vector2I Italic => new Vector2I(Index, 2);

                    /// <summary>
                    /// Returns the index for the bold italic version of this font
                    /// </summary>
                    public Vector2I BoldItalic => new Vector2I(Index, 3);

                    private readonly Func<int, bool> IsFontDefinedFunc;

                    public FontData(FontMembers members)
                    {
                        Name = members.Item1;
                        Index = members.Item2;
                        PtSize = members.Item3;
                        BaseScale = members.Item4;
                        IsFontDefinedFunc = members.Item5;
                    }

                    /// <summary>
                    /// Returns true if the font is defined for the given style.
                    /// </summary>
                    public bool IsStyleDefined(FontStyles styleEnum) =>
                        IsFontDefinedFunc((int)styleEnum);

                    /// <summary>
                    /// Returns true if the font is defined for the given style.
                    /// </summary>
                    public bool IsStyleDefined(int style) =>
                        IsFontDefinedFunc(style);

                    /// <summary>
                    /// Retrieves the full index of the font style
                    /// </summary>
                    public Vector2I GetStyleIndex(int style) =>
                        new Vector2I(Index, style);

                    /// <summary>
                    /// Retrieves the full index of the font style
                    /// </summary>
                    public Vector2I GetStyleIndex(FontStyles style) =>
                        new Vector2I(Index, (int)style);

                    public override int GetHashCode()
                    {
                        return Index.GetHashCode();
                    }

                    public override bool Equals(object obj)
                    {
                        var font = obj as FontData;

                        return font != null && font.Index == Index;
                    }
                }
            }
        }
    }
}