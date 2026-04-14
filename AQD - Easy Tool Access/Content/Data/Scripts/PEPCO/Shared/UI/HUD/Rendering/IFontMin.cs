using System;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
    [Flags]
    public enum FontStyles : int
    {
        Regular = 0,

        Bold = 1,

        /// <summary>
        /// Font effect, always available
        /// </summary>
        Italic = 2,

        BoldItalic = 3,

        /// <summary>
        /// Font effect, always available
        /// </summary>
        Underline = 4
    }

    /// <summary>
    /// Simplified Font interface for use by HUD API clients.
    /// </summary>
    public interface IFontMin
    {
        /// <summary>
        /// Font name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Index of the font in the font manager
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Font size at which the textures were created.
        /// </summary>
        float PtSize { get; }

        /// <summary>
        /// Default scaling applied to font. Used to normalize font size.
        /// </summary>
        float BaseScale { get; }

        /// <summary>
        /// Returns the index for this font using regular styling
        /// </summary>
        Vector2I Regular { get; }

        /// <summary>
        /// Returns the index for the bolded version of this font
        /// </summary>
        Vector2I Bold { get; }

        /// <summary>
        /// Returns the index for the italicised version of this font
        /// </summary>
        Vector2I Italic { get; }

        /// <summary>
        /// Returns the index for the bold italic version of this font
        /// </summary>
        Vector2I BoldItalic { get; }

        /// <summary>
        /// Returns true if the font is defined for the given style.
        /// </summary>
        bool IsStyleDefined(FontStyles styleEnum);

        /// <summary>
        /// Returns true if the font is defined for the given style.
        /// </summary>
        bool IsStyleDefined(int style);

        /// <summary>
        /// Retrieves the full index of the font style
        /// </summary>
        Vector2I GetStyleIndex(int style);

        /// <summary>
        /// Retrieves the full index of the font style
        /// </summary>
        Vector2I GetStyleIndex(FontStyles style);
    }
}