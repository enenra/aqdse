using System;
using System.Collections.Generic;
using VRageMath;
using VRage;

namespace RichHudFramework.UI
{
    using Rendering;
    using Server;
    using System.Collections;

    /// <summary>
    /// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class Dropdown<TValue> : Dropdown<ListBoxEntry<TValue>, Label, TValue>
    {
        public Dropdown(HudParentBase parent) : base(parent)
        { }

        public Dropdown() : base(null)
        { }
    }

    /// <summary>
    /// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class Dropdown<TElement, TValue> : Dropdown<ListBoxEntry<TElement, TValue>, TElement, TValue>
        where TElement : HudElementBase, IMinLabelElement, new()
    {
        public Dropdown(HudParentBase parent) : base(parent)
        { }

        public Dropdown() : base(null)
        { }
    }

    /// <summary>
    /// Generic collapsable list box. Allows use of custom entry element types.
    /// Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class Dropdown<TContainer, TElement, TValue>
        : HudElementBase, IClickableElement, IEntryBox<TContainer, TElement>
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        /// <summary>
        /// Invoked when a member of the list is selected.
        /// </summary>
        public event EventHandler SelectionChanged 
        { 
            add { listBox.SelectionChanged += value; } 
            remove { listBox.SelectionChanged -= value; } 
        }

        /// <summary>
        /// List of entries in the dropdown.
        /// </summary>
        public IReadOnlyList<TContainer> EntryList => listBox.EntryList;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        public IReadOnlyHudCollection<TContainer, TElement> HudCollection => listBox.HudCollection;

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public Dropdown<TContainer, TElement, TValue> ListContainer => this;

        /// <summary>
        /// Height of the dropdown list
        /// </summary>
        public float DropdownHeight { get { return listBox.Height; } set { listBox.Height = value; } }

        /// <summary>
        /// Padding applied to list members.
        /// </summary>
        public Vector2 MemberPadding { get { return listBox.MemberPadding; } set { listBox.MemberPadding = value; } }

        /// <summary>
        /// Height of entries in the dropdown.
        /// </summary>
        public float LineHeight { get { return listBox.LineHeight; } set { listBox.LineHeight = value; } }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return listBox.Format; } set { listBox.Format = value; display.Format = value; } }

        /// <summary>
        /// Background color of the dropdown list
        /// </summary>
        public Color Color { get { return listBox.Color; } set { listBox.Color = value; } }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return listBox.BarColor; } set { listBox.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return listBox.BarHighlight; } set { listBox.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return listBox.SliderColor; } set { listBox.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return listBox.SliderHighlight; } set { listBox.SliderHighlight = value; } }

        /// <summary>
        /// Background color of the highlight box
        /// </summary>
        public Color HighlightColor { get { return listBox.HighlightColor; } set { listBox.HighlightColor = value; } }

        /// <summary>
        /// Color of the highlight box's tab
        /// </summary>
        public Color TabColor { get { return listBox.TabColor; } set { listBox.TabColor = value; } }

        /// <summary>
        /// Padding applied to the highlight box.
        /// </summary>
        public Vector2 HighlightPadding { get { return listBox.HighlightPadding; } set { listBox.HighlightPadding = value; } }

        /// <summary>
        /// Minimum number of elements visible in the list at any given time.
        /// </summary>
        public int MinVisibleCount { get { return listBox.MinVisibleCount; } set { listBox.MinVisibleCount = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public TContainer Selection => listBox.Selection;

        /// <summary>
        /// Index of the current selection. -1 if empty.
        /// </summary>
        public int SelectionIndex => listBox.SelectionIndex;

        /// <summary>
        /// Mouse input for the dropdown display.
        /// </summary>
        public IMouseInput MouseInput => display.MouseInput;

        /// <summary>
        /// Indicates whether or not the dropdown is moused over.
        /// </summary>
        public override bool IsMousedOver => display.IsMousedOver || listBox.IsMousedOver;

        /// <summary>
        /// Indicates whether or not the list is open.
        /// </summary>
        public bool Open => listBox.Visible;

        public HudElementBase Display => display;

        public readonly ListBox<TContainer, TElement, TValue> listBox;
        protected readonly DropdownDisplay display;
        protected bool getDispFocus;

        public Dropdown(HudParentBase parent) : base(parent)
        {
            display = new DropdownDisplay(this)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Text = "None"
            };

            listBox = new ListBox<TContainer, TElement, TValue>()
            {
                Visible = false,
                CanIgnoreMasking = true,
                ZOffset = 3,
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
                TabColor = new Color(0, 0, 0, 0),
            };
            listBox.Register(display, true);
            
            Size = new Vector2(331f, 43f);

            display.MouseInput.LeftClicked += ClickDisplay;
            SelectionChanged += UpdateDisplay;
        }

        public Dropdown() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (SharedBinds.LeftButton.IsNewPressed && !(display.IsMousedOver || listBox.IsMousedOver))
                CloseList();

            if (getDispFocus)
            {
                display.MouseInput.GetInputFocus();
                getDispFocus = false;
            }
        }

        protected virtual void UpdateDisplay(object sender, EventArgs args)
        {
            if (Selection != null)
            {
                var fmt = display.MouseInput.HasFocus ? Format.WithColor(listBox.FocusTextColor) : Format;
                display.name.TextBoard.SetText(Selection.Element.TextBoard.ToString(), fmt);
                CloseList();
            }
        }

        protected virtual void ClickDisplay(object sender, EventArgs args)
        {
            if (!listBox.Visible)
            {
                OpenList();
            }
            else
            {
                CloseList();
            }
        }

        public void OpenList()
        {
            if (!listBox.Visible)
            {
                listBox.Visible = true;
                listBox.MouseInput.GetInputFocus();
            }
        }

        public void CloseList()
        {
            if (listBox.Visible)
            {
                listBox.Visible = false;
                getDispFocus = true;
            }
        }

        /// <summary>
        /// Adds a new member to the dropdown with the given name and associated
        /// object.
        /// </summary>
        public TContainer Add(RichText name, TValue assocMember, bool enabled = true) =>
            listBox.Add(name, assocMember, enabled);

        /// <summary>
        /// Adds the given range of entries to the dropdown.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, TValue, bool>> entries) =>
            listBox.AddRange(entries);

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText name, TValue assocMember, bool enabled = true) =>
            listBox.Insert(index, name, assocMember, enabled);

        /// <summary>
        /// Removes the given member from the dropdown.
        /// </summary>
        public void RemoveAt(int index) =>
            listBox.RemoveAt(index);

        /// <summary>
        /// Removes the member at the given index from the dropdown.
        /// </summary>
        public bool Remove(TContainer entry) =>
            listBox.Remove(entry);

        /// <summary>
        /// Removes the specified range of indices from the dropdown.
        /// </summary>
        public void RemoveRange(int index, int count) =>
            listBox.RemoveRange(index, count);

        /// <summary>
        /// Clears the current contents of the dropdown.
        /// </summary>
        public void ClearEntries() =>
            listBox.ClearEntries();

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelectionAt(int index) =>
            listBox.SetSelectionAt(index);

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(TValue assocMember) =>
            listBox.SetSelection(assocMember);

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(TContainer member) =>
            listBox.SetSelection(member);

        public object GetOrSetMember(object data, int memberEnum) =>
         listBox.GetOrSetMember(data, memberEnum);

        public IEnumerator<TContainer> GetEnumerator() =>
            listBox.EntryList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        protected class DropdownDisplay : Button
        {
            private static readonly Material arrowMat = new Material("RichHudDownArrow", new Vector2(64f, 64f));

            public RichText Text { get { return name.Text; } set { name.Text = value; } }

            public GlyphFormat Format 
            { 
                get { return name.Format; } 
                set { name.Format = value; } 
            }

            /// <summary>
            /// Color of the border surrounding the button
            /// </summary>
            public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

            /// <summary>
            /// Thickness of the border surrounding the button
            /// </summary>
            public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

            /// <summary>
            /// Text color used when the control gains focus.
            /// </summary>
            public Color FocusTextColor { get; set; }

            /// <summary>
            /// Background color used when the control gains focus.
            /// </summary>
            public Color FocusColor { get; set; }

            /// <summary>
            /// If true, then the button will change formatting when it takes focus.
            /// </summary>
            public bool UseFocusFormatting { get; set; }

            public readonly Label name;
            public readonly TexturedBox arrow, divider;

            private readonly HudChain layout;
            private readonly BorderBox border;
            private Color lastTextColor;

            public DropdownDisplay(HudParentBase parent = null) : base(parent)
            {
                border = new BorderBox(this)
                {
                    Thickness = 1f,
                    DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                };

                name = new Label()
                {
                    AutoResize = false,   
                    Padding = new Vector2(10f, 0f)
                };

                divider = new TexturedBox()
                {
                    Padding = new Vector2(4f, 17f),
                    Width = 2f,
                    Color = new Color(104, 113, 120),
                };

                arrow = new TexturedBox()
                {
                    Width = 38f,
                    MatAlignment = MaterialAlignment.FitVertical,
                    Material = arrowMat,
                };

                layout = new HudChain(false, this)
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    CollectionContainer = { name, divider, arrow }
                };

                Format = TerminalFormatting.ControlFormat;
                FocusTextColor = TerminalFormatting.Charcoal;

                Color = TerminalFormatting.OuterSpace;
                HighlightColor = TerminalFormatting.Atomic;
                FocusColor = TerminalFormatting.Mint;
                BorderColor = TerminalFormatting.LimedSpruce;

                HighlightEnabled = true;
                UseFocusFormatting = true;

                _mouseInput.GainedInputFocus += GainFocus;
                _mouseInput.LostInputFocus += LoseFocus;
            }

            protected override void Layout()
            {
                base.Layout();
                name.Width = (Width - Padding.X) - divider.Width - arrow.Width;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (MouseInput.HasFocus)
                {
                    if (SharedBinds.Space.IsNewPressed)
                    {
                        _mouseInput.OnLeftClick();
                    }
                }
                else if (!MouseInput.IsMousedOver)
                {
                    lastBackgroundColor = Color;
                    lastTextColor = name.Format.Color;
                }
            }

            protected override void CursorEnter(object sender, EventArgs args)
            {
                if (HighlightEnabled)
                {
                    if (!(UseFocusFormatting && MouseInput.HasFocus))
                    {
                        lastBackgroundColor = Color;
                        lastTextColor = name.Format.Color;
                    }

                    Color = HighlightColor;
                    name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));

                    divider.Color = lastTextColor.SetAlphaPct(0.8f);
                    arrow.Color = lastTextColor;
                }
            }

            protected override void CursorExit(object sender, EventArgs args)
            {
                if (HighlightEnabled)
                {
                    if (UseFocusFormatting && MouseInput.HasFocus)
                    {
                        Color = FocusColor;
                        name.TextBoard.SetFormatting(name.Format.WithColor(FocusTextColor));

                        divider.Color = FocusTextColor.SetAlphaPct(0.8f);
                        arrow.Color = FocusTextColor;
                    }
                    else
                    {
                        Color = lastBackgroundColor;
                        name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));

                        divider.Color = lastTextColor.SetAlphaPct(0.8f);
                        arrow.Color = lastTextColor;
                    }
                }
            }

            private void GainFocus(object sender, EventArgs args)
            {
                if (UseFocusFormatting)
                {
                    if (!MouseInput.IsMousedOver)
                    {
                        lastBackgroundColor = Color;
                        lastTextColor = name.Format.Color;
                    }

                    Color = FocusColor;
                    name.TextBoard.SetFormatting(name.Format.WithColor(FocusTextColor));

                    divider.Color = FocusTextColor.SetAlphaPct(0.8f);
                    arrow.Color = FocusTextColor;
                }
            }

            private void LoseFocus(object sender, EventArgs args)
            {
                if (UseFocusFormatting)
                {
                    Color = lastBackgroundColor;
                    name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));

                    divider.Color = lastTextColor.SetAlphaPct(0.8f);
                    arrow.Color = lastTextColor;
                }
            }
        }
    }
}