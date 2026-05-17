using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;
using EventHandler = RichHudFramework.EventHandler;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox<TContainer, TElement>
        : HudCollection<TContainer, TElement>
        where TContainer : IScrollBoxEntry<TElement>, new()
        where TElement : HudElementBase
    {
        /// <summary>
        /// List of entries in the selection box
        /// </summary>
        public virtual IReadOnlyList<TContainer> EntryList => hudCollectionList;

        /// <summary>
        /// Currently highlighted entry
        /// </summary>
        public virtual TContainer Selection 
        {
            get 
            {
                if (SelectionIndex >= 0 && SelectionIndex < hudCollectionList.Count)
                    return hudCollectionList[SelectionIndex];
                else
                    return default(TContainer);
            }
        }

        /// <summary>
        /// Returns the index of the current selection. Returns -1 if nothing is selected.
        /// </summary>
        public virtual int SelectionIndex { get; protected set; }

        /// <summary>
        /// Maximum number of entries. Used to determine subdivisions in circle. If enabled
        /// elements exceed this value, then the total number of entries will superceed this value.
        /// </summary>
        public virtual int MaxEntryCount { get; set; }

        /// <summary>
        /// Number of entries enabled
        /// </summary>
        public virtual int EnabledCount { get; protected set; }

        /// <summary>
        /// Enables/disables highlighting
        /// </summary>
        public virtual bool IsInputEnabled
        {
            get { return _isInputEnabled; }
            set
            {
                if (_isInputEnabled != value)
                    isStartPosStale = true;

                _isInputEnabled = value;
            }
        }

        /// <summary>
        /// Background color for the polyboard
        /// </summary>
        public virtual Color BackgroundColor { get; set; }

        /// <summary>
        /// Highlight color for the polyboard
        /// </summary>
        public virtual Color HighlightColor { get; set; }

        /// <summary>
        /// Cursor sensitivity for wheel scrolling on a scale from .3 to 2.
        /// </summary>
        public float CursorSensitivity { get; set; }

        public readonly PuncturedPolyBoard polyBoard;

        protected int selectionVisPos, effectiveMaxCount, minPolySize;
        protected bool isStartPosStale;
        protected Vector2 lastCursorPos, cursorNormal;
        private float lastDot;
        private bool _isInputEnabled;

        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        {
            polyBoard = new PuncturedPolyBoard()
            {
                Color = new Color(255, 255, 255, 128),
                Sides = 64
            };

            minPolySize = 64;
            Size = new Vector2(512f);
            MaxEntryCount = 8;
            CursorSensitivity = .5f;
        }

        public void SetSelectionAt(int index)
        {
            SelectionIndex = MathHelper.Clamp(index, 0, hudCollectionList.Count - 1);
        }

        public void SetSelection(TContainer container)
        {
            int index = FindIndex(x => x.Equals(container));

            if (index != -1)
                SelectionIndex = index;
        }

        protected override void Layout()
        {
            // Get enabled elements and effective max count
            EnabledCount = 0;
            SelectionIndex = MathHelper.Clamp(SelectionIndex, 0, hudCollectionList.Count - 1);

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                if (hudCollectionList[i].Enabled)
                {
                    hudCollectionList[i].Element.Visible = true;
                    EnabledCount++;
                }
                else
                    hudCollectionList[i].Element.Visible = false;
            }

            effectiveMaxCount = Math.Max(MaxEntryCount, EnabledCount);

            // Update entry positions
            int entrySize = polyBoard.Sides / effectiveMaxCount;
            Vector2I slice = new Vector2I(0, entrySize - 1);
            Vector2 size = cachedSize - cachedPadding;

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                TContainer container = hudCollectionList[i];
                TElement element = container.Element;

                if (container.Enabled)
                {
                    element.Offset = 1.05f * polyBoard.GetSliceOffset(size, slice);
                    slice += entrySize;
                }
            }

            polyBoard.Sides = Math.Max(effectiveMaxCount * 6, minPolySize);
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsInputEnabled)
            {
                CursorSensitivity = MathHelper.Clamp(CursorSensitivity, 0.3f, 2f);

                if (isStartPosStale)
                {
                    lastDot = 0f;
                    cursorNormal = Vector2.Zero;
                    lastCursorPos = cursorPos;
                    isStartPosStale = false;
                }

                Vector2 cursorOffset = cursorPos - lastCursorPos;

                if (cursorOffset.LengthSquared() > 64f)
                {
                    // Find enabled entry with the offset that most closely matches
                    // the direction of the normal
                    float dot = .5f;
                    int newSelection = -1;
                    Vector2 normalizedOffset = CursorSensitivity * 0.4f * Vector2.Normalize(cursorOffset);
                    cursorNormal = Vector2.Normalize(cursorNormal + normalizedOffset);

                    for (int i = 0; i < hudCollectionList.Count; i++)
                    {
                        TContainer container = hudCollectionList[i];
                        TElement element = container.Element;

                        if (container.Enabled)
                        {
                            float newDot = Vector2.Dot(element.Offset, cursorNormal);

                            if (newDot > dot && Math.Abs(lastDot - newDot) > .1f)
                            {
                                dot = newDot;
                                lastDot = dot;
                                newSelection = i;
                            }
                        }
                    }

                    lastCursorPos = cursorPos;
                    SelectionIndex = newSelection;
                }
            }
            else
            {
                isStartPosStale = true;
            }
        }

        protected void UpdateVisPos()
        {
            selectionVisPos = -1;

            // Find visible offset index
            for (int i = 0; i <= SelectionIndex; i++)
            {
                TContainer container = hudCollectionList[i];

                if (container.Enabled)
                    selectionVisPos++;
            }
        }

        protected override void Draw()
        {
            Vector2 size = cachedSize - cachedPadding;
            int entrySize = polyBoard.Sides / effectiveMaxCount;
            polyBoard.Color = BackgroundColor;
            polyBoard.Draw(size, cachedOrigin, HudSpace.PlaneToWorldRef);

            if (SelectionIndex != -1 && selectionVisPos != -1 && entrySize > 0)
            {
                UpdateVisPos();

                Vector2I slice = new Vector2I(0, entrySize - 1) + (selectionVisPos * entrySize);
                polyBoard.Color = HighlightColor;
                polyBoard.Draw(size, cachedOrigin, slice, HudSpace.PlaneToWorldRef);
            }
        }
    }

    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox : RadialSelectionBox<ScrollBoxEntry>
    {
        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        { }
    }

    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox<TContainer> : RadialSelectionBox<TContainer, HudElementBase>
        where TContainer : IScrollBoxEntry<HudElementBase>, new()
    {
        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        { }
    }
}