using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        [Flags]
        public enum DimAlignments : byte
        {
            None = 0x0,

            /// <summary>
            /// Match parent width
            /// </summary>
            Width = 0x1,

            /// <summary>
            /// Match parent height
            /// </summary>
            Height = 0x2,

            /// <summary>
            /// Match parent size
            /// </summary>
            Both = Width | Height,

            /// <summary>
            /// Matches parent size less padding
            /// </summary>
            IgnorePadding = 0x4,
        }

        /// <summary>
        /// Used to determine the default position of an element relative to its parent.
        /// </summary>
        [Flags]
        public enum ParentAlignments : byte
        {
            /// <summary>
            /// The element's origin is at the center of its parent.
            /// </summary>
            Center = 0x0,

            /// <summary>
            /// The element will start with its right edge aligned to its parent's left edge.
            /// If the flag InnerH is set, then its left edge will be aligned to its parent's
            /// left edge.
            /// </summary>
            Left = 0x1,

            /// <summary>
            /// The element will start with its bottom edge aligned to its parent's top edge.
            /// If the flag InnerV is set, then its top edge will be aligned to its parent's
            /// top edge.
            /// </summary>
            Top = 0x2,

            /// <summary>
            /// The element will start with its left edge aligned to its parent's right edge.
            /// If the flag InnerH is set, then its right edge will be aligned to its parent's
            /// right edge.
            /// </summary>
            Right = 0x4,

            /// <summary>
            /// The element will start with its top edge aligned to its parent's bottom edge.
            /// If the flag InnerV is set, then its bottom edge will be aligned to its parent's
            /// bottom edge.
            /// </summary>
            Bottom = 0x8,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Left/Right flags. If this flag is set,
            /// then the element will be horizontally aligned to the interior of its parent.
            /// </summary>
            InnerH = 0x10,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Top/Bottom flags. If this flag is set,
            /// then the element will be vertically aligned to the interior of its parent.
            /// </summary>
            InnerV = 0x20,

            /// <summary>
            /// InnerH + InnerV. If this flag is set then the element will be aligned to the interior of
            /// its parent.
            /// </summary>
            Inner = InnerH | InnerV,

            /// <summary>
            /// If set, this flag will cause the element's alignment to be calculated taking the
            /// parent's padding into account.
            /// </summary>
            UsePadding = 0x40,
        }
    }
}