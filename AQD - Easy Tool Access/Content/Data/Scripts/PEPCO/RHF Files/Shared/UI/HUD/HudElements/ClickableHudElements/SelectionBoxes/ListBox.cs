using System;
using System.Text;
using VRage;
using VRageMath;
using System.Collections.Generic;
using RichHudFramework.UI.Rendering;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using System.Collections;

namespace RichHudFramework.UI
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// Scrollable list of text elements. Each list entry is associated with a value of type T.
    /// </summary>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class ListBox<TValue> : ListBox<ListBoxEntry<TValue>, Label, TValue>
    {
        public ListBox(HudParentBase parent) : base(parent)
        { }

        public ListBox() : base(null)
        { }
    }

    /// <summary>
    /// Generic scrollable list of text elements. Allows use of custom entry element types.
    /// Each list entry is associated with a value of type T.
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class ListBox<TContainer, TElement, TValue>
        : ScrollSelectionBox<TContainer, TElement, TValue>, IClickableElement
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return hudChain.BarColor; } set { hudChain.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return hudChain.BarHighlight; } set { hudChain.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return hudChain.SliderColor; } set { hudChain.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return hudChain.SliderHighlight; } set { hudChain.SliderHighlight = value; } }

        protected override Vector2I ListRange => hudChain.ClipRange;

        protected override Vector2 ListSize
        {
            get
            {
                Vector2 listSize = hudChain.Size;
                listSize.X -= hudChain.ScrollBar.Width;

                return listSize;
            }
        }

        protected override Vector2 ListPos
        {
            get
            {
                Vector2 listPos = hudChain.Position;
                listPos.X -= hudChain.ScrollBar.Width;

                return listPos;
            }
        }

        public ListBox(HudParentBase parent) : base(parent)
        {
            hudChain.MinVisibleCount = 5;
            hudChain.Padding = new Vector2(0f, 8f);
        }

        public ListBox() : this(null)
        { }

        protected override void Draw()
        {
            Size = hudChain.Size + Padding;
        }
    }

}