using System;
using System.Text;
using VRage;
using VRageMath;
using System.Collections.Generic;
using RichHudFramework.UI.Rendering;
using System.Collections;
using System.Reflection;

namespace RichHudFramework.UI
{
    /// <summary>
    /// MouseInputElement subtype designed to manage selection, highlighting of vertically or horizontally scrolling 
    /// lists.
    /// </summary>
    public class ListInputElement<TElementContainer, TElement> : MouseInputElement
        where TElement : HudElementBase, IMinLabelElement
        where TElementContainer : class, IScrollBoxEntry<TElement>, new()
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Read-only collection of list entries
        /// </summary>
        public IReadOnlyHudCollection<TElementContainer, TElement> Entries { get; }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public TElementContainer Selection
        {
            get
            {
                if (Entries.Count == 0 || SelectionIndex < 0 || SelectionIndex >= Entries.Count)
                {
                    return default(TElementContainer);
                }
                else
                {
                    return Entries[SelectionIndex];
                }
            }
        }

        /// <summary>
        /// Index of the current selection. -1 if empty.
        /// </summary>
        public int SelectionIndex => MathHelper.Clamp(_selectionIndex, -1, Entries.Count - 1);

        /// <summary>
        /// Index of the highlighted entry
        /// </summary>
        public int HighlightIndex => MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);

        /// <summary>
        /// Index of the entry with input focus 
        /// </summary>
        public int FocusIndex => MathHelper.Clamp(_focusIndex, 0, Entries.Count - 1);

        /// <summary>
        /// If true, then the element is using the keyboard for scrolling
        /// </summary>
        public bool KeyboardScroll { get; protected set; }

        /// <summary>
        /// Range of entries visible to the input element
        /// </summary>
        public Vector2I ListRange { get; set; }

        /// <summary>
        /// Visible size of list entries
        /// </summary>
        public Vector2 ListSize { get; set; }

        /// <summary>
        /// Position of list entries
        /// </summary>
        public Vector2 ListPos { get; set; }

        protected Vector2 lastCursorPos;
        private int _selectionIndex;
        private int _highlightIndex;
        private int _focusIndex;

        public ListInputElement(IReadOnlyHudCollection<TElementContainer, TElement> entries, HudElementBase parent = null) : base(parent)
        {
            Entries = entries;
            _selectionIndex = -1;
        }

        public ListInputElement(HudChain<TElementContainer, TElement> parent = null) : this(parent, parent)
        { }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelectionAt(int index)
        {
            if (index != _selectionIndex)
            {
                _selectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
                Selection.Enabled = true;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(TElementContainer member)
        {
            int index = Entries.FindIndex(x => member.Equals(x));

            if (index != -1 && index != _selectionIndex)
            {
                _selectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
                Selection.Enabled = true;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Offsets selection index in the direction of the offset. If wrap == true, the index will wrap around
        /// if the offset places it out of range.
        /// </summary>
        public void OffsetSelectionIndex(int offset, bool wrap = false)
        {
            int index = _selectionIndex,
                dir = offset > 0 ? 1 : -1,
                absOffset = Math.Abs(offset);

            if (dir > 0)
            {
                for (int i = 0; i < absOffset; i++)
                {
                    if (wrap)
                        index = (index + dir) % Entries.Count;
                    else
                        index = Math.Min(index + dir, Entries.Count - 1);

                    index = FindFirstEnabled(index, wrap);
                }
            }
            else
            {
                for (int i = 0; i < absOffset; i++)
                {
                    if (wrap)
                        index = (index + dir) % Entries.Count;
                    else
                        index = Math.Max(index + dir, 0);

                    if (index < 0)
                        index += Entries.Count;

                    index = FindLastEnabled(index, wrap);
                }
            }

            SetSelectionAt(index);
        }

        /// <summary>
        /// Clears the current selection
        /// </summary>
        public void ClearSelection()
        {
            _selectionIndex = -1;
            _highlightIndex = 0;
            _focusIndex = 0;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (Entries.Count > 0)
            {
                base.HandleInput(cursorPos);
                UpdateSelectionInput(cursorPos);
            }
        }

        /// <summary>
        /// Update selection and highlight based on user input
        /// </summary>
        protected virtual void UpdateSelectionInput(Vector2 cursorPos)
        {
            _selectionIndex = MathHelper.Clamp(_selectionIndex, -1, Entries.Count - 1);
            _highlightIndex = MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);

            // If using arrow keys to scroll, adjust the scrollbox's start/end indices
            if (KeyboardScroll)
                // If using arrow keys to scroll, then the focus should follow the highlight
                _focusIndex = _highlightIndex;
            else // Otherwise, focus index should follow the selection
                _focusIndex = _selectionIndex;

            // Keyboard input
            if (HasFocus)
            {
                if (SharedBinds.UpArrow.IsNewPressed || SharedBinds.UpArrow.IsPressedAndHeld)
                {
                    for (int i = _highlightIndex - 1; i >= 0; i--)
                    {
                        if (Entries[i].Enabled)
                        {
                            _highlightIndex = i;
                            break;
                        }
                    }

                    KeyboardScroll = true;
                    lastCursorPos = cursorPos;
                }
                else if (SharedBinds.DownArrow.IsNewPressed || SharedBinds.DownArrow.IsPressedAndHeld)
                {
                    for (int i = _highlightIndex + 1; i < Entries.Count; i++)
                    {
                        if (Entries[i].Enabled)
                        {
                            _highlightIndex = i;
                            break;
                        }
                    }

                    KeyboardScroll = true;
                    lastCursorPos = cursorPos;
                }
            }
            else
            {
                KeyboardScroll = false;
                lastCursorPos = new Vector2(float.MinValue);
            }

            bool listMousedOver = false;

            // Mouse input
            if (IsMousedOver)
            {
                // If the user moves the cursor after using arrow keys for selection then moves the
                // mouse, disable arrow selection
                if ((cursorPos - lastCursorPos).LengthSquared() > 4f)
                    KeyboardScroll = false;

                if (!KeyboardScroll)
                {
                    Vector2 cursorOffset = cursorPos - ListPos;
                    BoundingBox2 listBounds = new BoundingBox2(-ListSize * .5f, ListSize * .5f);

                    // If the list is moused over, then calculate highlight index based on cursor position.
                    if (listBounds.Contains(cursorOffset) == ContainmentType.Contains)
                    {
                        int newIndex = ListRange.X;

                        for (int i = ListRange.X; i <= ListRange.Y; i++)
                        {
                            if (Entries[i].Enabled)
                            {
                                TElement element = Entries[i].Element;
                                Vector2 halfSize = element.Size * .5f,
                                    offset = element.Offset;
                                BoundingBox2 bb = new BoundingBox2(offset - halfSize, offset + halfSize);

                                if (bb.Contains(cursorOffset) == ContainmentType.Contains)
                                    break;
                            }

                            newIndex++;
                        }

                        if (newIndex >= 0 && newIndex < Entries.Count)
                        {
                            _highlightIndex = newIndex;
                            listMousedOver = true;
                        }
                    }
                }
            }

            if ((listMousedOver && SharedBinds.LeftButton.IsNewPressed) || (HasFocus && SharedBinds.Space.IsNewPressed))
            {
                _selectionIndex = _highlightIndex;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                KeyboardScroll = false;
            }

            _highlightIndex = MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);
        }

        /// <summary>
        /// Returns first enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindFirstEnabled(int index, bool wrap)
        {
            if (wrap)
            {
                int j = index;

                for (int n = 0; n < 2 * Entries.Count; n++)
                {
                    if (Entries[j].Enabled)
                        return j;

                    j++;
                    j %= Entries.Count;
                }
            }
            else
            {
                for (int n = index; n < Entries.Count; n++)
                {
                    if (Entries[n].Enabled)
                        return n;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns preceeding enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindLastEnabled(int index, bool wrap)
        {
            if (wrap)
            {
                int j = index;

                for (int n = 0; n < 2 * Entries.Count; n++)
                {
                    if (Entries[j].Enabled)
                        return j;

                    j++;
                    j %= Entries.Count;
                }
            }
            else
            {
                for (int n = index; n >= 0; n--)
                {
                    if (Entries[n].Enabled)
                        return n;
                }
            }

            return -1;
        }
    }
}
