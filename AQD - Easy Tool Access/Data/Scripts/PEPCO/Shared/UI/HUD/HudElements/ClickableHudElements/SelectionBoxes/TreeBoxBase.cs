using System;
using System.Collections;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;

    /// <summary>
    /// Abstract, generic base for tree boxes/lists
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    public class TreeBoxBase<TContainer, TElement> 
        : TreeBoxBase<
            ChainSelectionBoxBase<TContainer, TElement>,
            HudChain<TContainer, TElement>,
            TContainer,
            TElement
        >
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        public TreeBoxBase(HudParentBase parent) : base(parent)
        { }

        public TreeBoxBase() : base(null)
        { }
    }

    /// <summary>
    /// Abstract, generic base for tree boxes/lists
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TChain">HudChain type used by the SelectionBox as the list container</typeparam>
    /// <typeparam name="TSelectionBox">SelectionBox type</typeparam>
    public abstract class TreeBoxBase<TSelectionBox, TChain, TContainer, TElement>
        : LabelElementBase, IEntryBox<TContainer, TElement>, IClickableElement
        where TElement : HudElementBase, IMinLabelElement
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
        where TChain : HudChain<TContainer, TElement>, new()
        where TSelectionBox : SelectionBoxBase<TChain, TContainer, TElement>, new()
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add { selectionBox.SelectionChanged += value; }
            remove { selectionBox.SelectionChanged -= value; }
        }

        /// <summary>
        /// List of entries in the treebox.
        /// </summary>
        public IReadOnlyList<TContainer> EntryList => selectionBox.EntryList;

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public TreeBoxBase<TSelectionBox, TChain, TContainer, TElement> ListContainer => this;

        /// <summary>
        /// If true, then the dropdown list will be open
        /// </summary>
        public bool ListOpen { get; protected set; }

        /// <summary>
        /// Height of the treebox in pixels.
        /// </summary>
        public override float Height
        {
            get
            {
                if (!ListOpen)
                    return display.Height + Padding.Y;
                else
                    return display.Height + selectionBox.Height + Padding.Y;
            }
            set
            {
                if (Padding.Y < value)
                    value -= Padding.Y;

                if (!ListOpen)
                {
                    display.Height = value;
                }
            }
        }

        /// <summary>
        /// Name of the element as rendered on the display
        /// </summary>
        public RichText Name { get { return display.Name; } set { display.Name = value; } }

        /// <summary>
        /// TextBoard backing the name label
        /// </summary>
        public override ITextBoard TextBoard => display.name.TextBoard;

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return display.Format; } set { display.Format = value; selectionBox.Format = value; } }

        /// <summary>
        /// Text color used for entries that have input focus
        /// </summary>
        public Color FocusTextColor { get { return selectionBox.FocusTextColor; } set { selectionBox.FocusTextColor = value; } }

        /// <summary>
        /// Determines the color of the header's background/
        /// </summary>
        public Color HeaderColor { get { return display.Color; } set { display.Color = value; } }

        /// <summary>
        /// Default background color of the highlight box
        /// </summary>
        public Color HighlightColor { get { return selectionBox.HighlightColor; } set { selectionBox.HighlightColor = value; } }

        /// <summary>
        /// Background color used for selection/highlighting when the list has input focus
        /// </summary>
        public Color FocusColor { get { return selectionBox.FocusColor; } set { selectionBox.FocusColor = value; } }

        /// <summary>
        /// Background color used for selection/highlighting when the list has input focus
        /// </summary>
        public Color TabColor { get { return selectionBox.TabColor; } set { selectionBox.TabColor = value; } }

        /// <summary>
        /// Padding applied to the highlight box.
        /// </summary>
        public Vector2 HighlightPadding { get { return selectionBox.HighlightPadding; } set { selectionBox.HighlightPadding = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public TContainer Selection => selectionBox.Selection;

        /// <summary>
        /// Size of the entry collection.
        /// </summary>
        public int Count => selectionBox.Count;

        /// <summary>
        /// Determines how far to the right list members should be offset from the position of the header.
        /// </summary>
        public float IndentSize { get { return _indentSize; } set { _indentSize = value; } }

        /// <summary>
        /// Sizing mode used by the chain containing the tree box's member list
        /// </summary>
        public HudChainSizingModes MemberSizingModes
        {
            get { return selectionBox.hudChain.SizingMode; }
            set { selectionBox.hudChain.SizingMode = value; }
        }

        /// <summary>
        /// Member lists' min member size
        /// </summary>
        public Vector2 MemberMinSize
        {
            get { return selectionBox.hudChain.MemberMinSize; }
            set { selectionBox.hudChain.MemberMinSize = value; }
        }

        /// <summary>
        /// Member lists' max member size
        /// </summary>
        public Vector2 MemberMaxSize
        {
            get { return selectionBox.hudChain.MemberMinSize; }
            set { selectionBox.hudChain.MemberMinSize = value; }
        }

        /// <summary>
        /// Handles mouse input for the header.
        /// </summary>
        public IMouseInput MouseInput => display.MouseInput;

        public HudElementBase Display => display;

        public readonly TSelectionBox selectionBox;
        protected readonly TreeBoxDisplay display;
        protected float _indentSize;

        public TreeBoxBase(HudParentBase parent) : base(parent)
        {
            display = new TreeBoxDisplay(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV | ParentAlignments.UsePadding,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding
            };

            selectionBox = new TSelectionBox()
            {
                Visible = false,
                ParentAlignment = ParentAlignments.Bottom,
                HighlightPadding = Vector2.Zero
            };
            selectionBox.Register(display, true);

            selectionBox.hudChain.SizingMode =
                HudChainSizingModes.FitMembersOffAxis |
                HudChainSizingModes.ClampMembersAlignAxis |
                HudChainSizingModes.ClampChainOffAxis |
                HudChainSizingModes.FitChainAlignAxis;

            Size = new Vector2(200f, 34f);
            IndentSize = 20f;

            Format = GlyphFormat.Blueish;

            display.Name = "NewTreeBox";
            display.MouseInput.LeftClicked += ToggleList;
        }

        public TreeBoxBase() : this(null)
        { }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(TContainer member) =>
            selectionBox.SetSelection(member);

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelectionAt(int index) =>
            selectionBox.SetSelectionAt(index);

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection() =>
            selectionBox.ClearSelection();

        protected virtual void ToggleList(object sender, EventArgs args)
        {
            if (!ListOpen)
                OpenList();
            else
                CloseList();
        }

        public void OpenList()
        {
            selectionBox.Visible = true;
            display.Open = true;
            ListOpen = true;
        }

        public void CloseList()
        {
            selectionBox.Visible = false;
            display.Open = false;
            ListOpen = false;
        }

        protected override void Layout()
        {
            selectionBox.Visible = ListOpen;

            if (ListOpen)
            {
                selectionBox.Width = Width - 2f * IndentSize - Padding.X;
                selectionBox.Offset = new Vector2(IndentSize, 0f);
            }
        }

        public IEnumerator<TContainer> GetEnumerator() =>
            selectionBox.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            selectionBox.GetEnumerator();

        /// <summary>
        /// Modified dropdown header with a rotating arrow on the left side indicating
        /// whether the list is open.
        /// </summary>
        protected class TreeBoxDisplay : HudElementBase
        {
            public RichText Name { get { return name.Text; } set { name.Text = value; } }

            public GlyphFormat Format { get { return name.Format; } set { name.Format = value; } }

            public Color Color { get { return background.Color; } set { background.Color = value; } }

            public IMouseInput MouseInput => mouseInput;

            public bool Open
            {
                get { return open; }
                set
                {
                    open = value;

                    if (open)
                        arrow.Material = downArrow;
                    else
                        arrow.Material = rightArrow;
                }
            }

            private bool open;

            public readonly Label name;
            private readonly TexturedBox arrow, divider, background;
            private readonly HudChain layout;
            private readonly MouseInputElement mouseInput;

            private static readonly Material
                downArrow = new Material("RichHudDownArrow", new Vector2(64f, 64f)),
                rightArrow = new Material("RichHudRightArrow", new Vector2(64f, 64f));

            public TreeBoxDisplay(HudParentBase parent = null) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    Color = TerminalFormatting.EbonyClay,
                    DimAlignment = DimAlignments.Both,
                };

                name = new Label()
                {
                    AutoResize = false,
                    Padding = new Vector2(10f, 0f),
                    Format = GlyphFormat.Blueish.WithSize(1.1f),
                };

                divider = new TexturedBox()
                {
                    Padding = new Vector2(2f, 6f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                };

                arrow = new TexturedBox()
                {
                    Width = 20f,
                    Padding = new Vector2(8f, 0f),
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Color = new Color(227, 230, 233),
                    Material = rightArrow,
                };

                layout = new HudChain(false, this)
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    CollectionContainer = { arrow, divider, name }
                };

                mouseInput = new MouseInputElement(this)
                {
                    DimAlignment = DimAlignments.Both
                };
            }

            protected override void Layout()
            {
                name.Width = (Width - Padding.X) - divider.Width - arrow.Width;
            }
        }
    }
}