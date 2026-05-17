using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Read-only interface for hud elements with definite size and position.
        /// </summary>
        public interface IReadOnlyHudElement : IReadOnlyHudNode
        {
            /// <summary>
            /// Size of the element. Units in pixels by default.
            /// </summary>
            Vector2 Size { get; }

            /// <summary>
            /// Height of the hud element. Units in pixels by default.
            /// </summary>
            float Height { get; }

            /// <summary>
            /// Width of the hud element. Units in pixels by default.
            /// </summary>
            float Width { get; }

            /// <summary>
            /// Starting position of the hud element on the screen in pixels.
            /// </summary>
            Vector2 Origin { get; }

            /// <summary>
            /// Position of the hud element relative to its origin.
            /// </summary>
            Vector2 Offset { get; }

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            ParentAlignments ParentAlignment { get; }

            /// <summary>
            /// Determines how/if an element will copy its parent's dimensions. 
            /// </summary>
            DimAlignments DimAlignment { get; }

            /// <summary>
            /// If set to true the hud element will be allowed to capture the cursor.
            /// </summary>
            bool UseCursor { get; }

            /// <summary>
            /// If set to true the hud element will share the cursor with its child elements.
            /// </summary>
            bool ShareCursor { get; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over the element. The element must
            /// be set to capture the cursor for this to work.
            /// </summary>
            bool IsMousedOver { get; }
        }
    }
}