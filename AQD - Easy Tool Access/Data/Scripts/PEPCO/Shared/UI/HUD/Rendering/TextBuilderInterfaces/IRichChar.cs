using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {

        namespace Rendering
        {
            public enum RichCharAccessors : int
            {
                /// <summary>
                /// out: char
                /// </summary>
                Ch = 1,

                /// <summary>
                /// out: GlyphFormatMembers
                /// </summary>
                Format = 2,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Size = 3,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Offset = 4
            }

            public interface IRichChar
            {
                /// <summary>
                /// Character assocated with the glyph
                /// </summary>
                char Ch { get; }

                /// <summary>
                /// Text format used by the character
                /// </summary>
                GlyphFormat Format { get; }

                /// <summary>
                /// Size of the glyph as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Position of the glyph relative to the center of its parent text element. Does not include the 
                /// parent's TextOffset. Will not be updated if outside its TextBoard's visible line range.
                /// </summary>
                Vector2 Offset { get; }
            }
        }
    }
}