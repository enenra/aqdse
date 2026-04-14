using System;
using System.Collections.Generic;
using VRageMath;
using VRage;
using VRage.Utils;
using AtlasMembers = VRage.MyTuple<string, VRageMath.Vector2>;
using GlyphMembers = VRage.MyTuple<int, VRageMath.Vector2, VRageMath.Vector2, float, float>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using Client;
    using FontMembers = MyTuple<
        string, // Name
        int, // Index
        float, // PtSize
        float, // BaseScale
        Func<int, bool>, // IsStyleDefined
        ApiMemberAccessor
    >;
    using FontStyleDefinition = MyTuple<
        int, // styleID
        float, // height
        float, // baseline
        AtlasMembers[], // atlases
        KeyValuePair<char, GlyphMembers>[], // glyphs
        KeyValuePair<uint, float>[] // kernings
    >;

    namespace UI
    {
        using FontDefinition = MyTuple<
            string, // Name
            float, // PtSize
            FontStyleDefinition[] // styles
        >;

        namespace Rendering.Client
        {
            using FontManagerMembers = MyTuple<
                MyTuple<Func<int, FontMembers>, Func<int>>, // Font List
                Func<FontDefinition, FontMembers?>, // TryAddFont
                Func<string, FontMembers?>, // GetFont
                ApiMemberAccessor
            >;

            /// <summary>
            /// Manages fonts used by the Rich Hud Framework
            /// </summary>
            public sealed partial class FontManager : RichHudClient.ApiModule<FontManagerMembers>
            {
                /// <summary>
                /// Retrieves default font for Space Engineers with regular styling.
                /// </summary>
                public static Vector2I Default => Vector2I.Zero;

                /// <summary>
                /// Read-only collection of all registered fonts.
                /// </summary>
                public static IReadOnlyList<IFontMin> Fonts => Instance.fonts;

                private static FontManager Instance
                {
                    get { Init(); return instance; }
                    set { instance = value; }
                }
                private static FontManager instance;

                private readonly ReadOnlyApiCollection<IFontMin> fonts;
                private readonly Func<FontDefinition, FontMembers?> TryAddFontFunc;
                private readonly Func<string, FontMembers?> GetFontFunc;

                private FontManager() : base(ApiModuleTypes.FontManager, false, true)
                {
                    var members = GetApiData();

                    Func<int, IFontMin> fontGetter = x => new FontData(members.Item1.Item1(x));
                    fonts = new ReadOnlyApiCollection<IFontMin>(fontGetter, members.Item1.Item2);

                    TryAddFontFunc = members.Item2;
                    GetFontFunc = members.Item3;
                }

                private static void Init()
                {
                    if (instance == null)
                        instance = new FontManager();
                }

                public override void Close()
                {
                    instance = null;
                }

                /// <summary>
                /// Attempts to register a new font using API data.
                /// </summary>
                public static bool TryAddFont(FontDefinition fontData) =>
                    Instance.TryAddFontFunc(fontData) != null;

                /// <summary>
                /// Attempts to register a new font using API data. Returns the font created.
                /// </summary>
                public static bool TryAddFont(FontDefinition fontData, out IFontMin font)
                {
                    FontMembers? members = Instance.TryAddFontFunc(fontData);

                    if (members != null)
                    {
                        font = new FontData(members.Value);
                        return true;
                    }
                    else
                    {
                        font = null;
                        return false;
                    }
                }

                /// <summary>
                /// Retrieves the font with the given name.
                /// </summary>
                public static IFontMin GetFont(string name)
                {
                    FontMembers? members = Instance.GetFontFunc(name);
                    IFontMin font = null;

                    if (members != null)
                        font = new FontData(members.Value);

                    return font;
                }

                /// <summary>
                /// Retrieves the font with the given name.
                /// </summary>
                public static IFontMin GetFont(int index) =>
                    Instance.fonts[index];

                /// <summary>
                /// Retrieves the font style index of the font with the given name and style.
                /// </summary>
                public static Vector2I GetStyleIndex(string name, FontStyles style = FontStyles.Regular)
                {
                    IFontMin font = GetFont(name);
                    return new Vector2I(font.Index, (int)style);
                }
            }
        }
    }
}