using System;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {

        namespace Rendering
        {
            public enum TextBoardAccessors : int
            {
                /// <summary>
                /// in/out: bool
                /// </summary>
                AutoResize = 129,

                /// <summary>
                /// in/out: bool
                /// </summary>
                VertAlign = 130,

                /// <summary>
                /// in: Vector2I
                /// </summary>
                MoveToChar = 131,

                /// <summary>
                /// out: Vector2I
                /// </summary>
                GetCharAtOffset = 132,

                /// <summary>
                /// Action event
                /// </summary>
                OnTextChanged = 133,

                /// <summary>
                /// in/out: Vector2
                /// </summary>
                TextOffset = 134,

                /// <summary>
                /// out: Vector2I
                /// </summary>
                VisibleLineRange = 135,
            }

            public interface ITextBoard : ITextBuilder
            {
                /// <summary>
                /// Invoked whenever a change is made to the text. Invokes once every 500ms, at most.
                /// </summary>
                event Action TextChanged;

                /// <summary>
                /// Scale of the text board. Applied after scaling specified in GlyphFormat.
                /// </summary>
                float Scale { get; set; }

                /// <summary>
                /// Size of the text box as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Full text size including any text outside the visible range.
                /// </summary>
                Vector2 TextSize { get; }

                /// <summary>
                /// Used to change the position of the text within the text element. AutoResize must be disabled for this to work.
                /// </summary>
                Vector2 TextOffset { get; set; }

                /// <summary>
                /// Returns the range of lines visible.
                /// </summary>
                Vector2I VisibleLineRange { get; }

                /// <summary>
                /// Size of the text box when AutoResize is set to false. Does nothing otherwise.
                /// </summary>
                Vector2 FixedSize { get; set; }

                /// <summary>
                /// If true, the text board will automatically resize to fit the text.
                /// </summary>
                bool AutoResize { get; set; }

                /// <summary>
                /// If true, the text will be vertically aligned to the center of the text board.
                /// </summary>
                bool VertCenterText { get; set; }

                /// <summary>
                /// Calculates and applies the minimum offset needed to ensure that the character at the specified index
                /// is within the visible range.
                /// </summary>
                void MoveToChar(Vector2I index);

                /// <summary>
                /// Returns the index of the character at the given offset.
                /// </summary>
                Vector2I GetCharAtOffset(Vector2 localPos);

                /// <summary>
                /// Draws the text board in screen space with an offset given in pixels.
                /// </summary>
                void Draw(Vector2 origin);

                /// <summary>
                /// Draws the text board in world space on the XY plane of the matrix, facing in the +Z
                /// direction.
                /// </summary>
                void Draw(Vector2 offset, MatrixD matrix);

                /// <summary>
                /// Draws the text board in world space on the XY plane of the matrix, facing in the +Z
                /// direction.
                /// </summary>
                void Draw(BoundingBox2 box, BoundingBox2 mask, MatrixD[] matrix);
            }
        }
    }
}