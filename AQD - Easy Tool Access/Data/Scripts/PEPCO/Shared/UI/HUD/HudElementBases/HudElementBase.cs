using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;
        using Internal;

        /// <summary>
        /// Base type for all hud elements with definite size and position. Inherits from HudParentBase and HudNodeBase.
        /// </summary>
        public abstract class HudElementBase : HudNodeBase, IReadOnlyHudElement
        {
            protected const float minMouseBounds = 8f;
            protected const HudElementStates elementNotVisible = ~(HudElementStates.IsVisible | HudElementStates.IsMousedOver | HudElementStates.IsMouseInBounds);

            public override bool Visible
            {
                set
                {
                    if (value)
                        State |= HudElementStates.IsVisible;
                    else
                        State &= elementNotVisible;
                }
            }

            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public sealed override HudParentBase Parent
            {
                protected set
                {
                    _parent = value;
                    _parentFull = value as HudElementBase;
                }
            }

            /// <summary>
            /// Size of the element. Units in pixels by default.
            /// </summary>
            public Vector2 Size
            {
                get { return new Vector2(Width, Height); }
                set { Width = value.X; Height = value.Y; }
            }

            /// <summary>
            /// Width of the hud element. Units in pixels by default.
            /// </summary>
            public virtual float Width
            {
                get { return _size.X + Padding.X; }
                set
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    _size.X = value;
                }
            }

            /// <summary>
            /// Height of the hud element. Units in pixels by default.
            /// </summary>
            public virtual float Height
            {
                get { return _size.Y + Padding.Y; }
                set
                {
                    if (value > Padding.Y)
                        value -= Padding.Y;

                    _size.Y = value;
                }
            }

            /// <summary>
            /// Border size. Included in total element size.
            /// </summary>
            public virtual Vector2 Padding
            {
                get { return _padding; }
                set { _padding = value; }
            }

            /// <summary>
            /// Starting position of the hud element.
            /// </summary>
            public Vector2 Origin => (_parentFull == null) ? Vector2.Zero : _parentFull.cachedPosition + originAlignment;

            /// <summary>
            /// Position of the element relative to its origin.
            /// </summary>
            public Vector2 Offset { get; set; }

            /// <summary>
            /// Current position of the hud element. Origin + Offset.
            /// </summary>
            public Vector2 Position => Origin + Offset;

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            public ParentAlignments ParentAlignment { get; set; }

            /// <summary>
            /// Determines how/if an element will copy its parent's dimensions. 
            /// </summary>
            public DimAlignments DimAlignment { get; set; }

            /// <summary>
            /// If set to true the hud element will be allowed to capture the cursor.
            /// </summary>
            public bool UseCursor
            {
                get { return (State & HudElementStates.CanUseCursor) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanUseCursor;
                    else
                        State &= ~HudElementStates.CanUseCursor;
                }
            }

            /// <summary>
            /// If set to true the hud element will share the cursor with other elements.
            /// </summary>
            public bool ShareCursor
            {
                get { return (State & HudElementStates.CanShareCursor) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanShareCursor;
                    else
                        State &= ~HudElementStates.CanShareCursor;
                }
            }

            /// <summary>
            /// If set to true, the hud element will act as a clipping mask for child elements.
            /// False by default. Masking parent elements can still affect non-masking children.
            /// </summary>
            public bool IsMasking
            {
                get { return (State & HudElementStates.IsMasking) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsMasking;
                    else
                        State &= ~HudElementStates.IsMasking;
                }
            }

            /// <summary>
            /// If set to true, the hud element will treat its parent as a clipping mask, whether
            /// it's configured as a mask or not.
            /// </summary>
            public bool IsSelectivelyMasked
            {
                get { return (State & HudElementStates.IsSelectivelyMasked) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsSelectivelyMasked;
                    else
                        State &= ~HudElementStates.IsSelectivelyMasked;
                }
            }

            /// <summary>
            /// If set to true, then the element can ignore any bounding masks imposed by its parents.
            /// Superceeds selective masking flag.
            /// </summary>
            public bool CanIgnoreMasking
            {
                get { return (State & HudElementStates.CanIgnoreMasking) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanIgnoreMasking;
                    else
                        State &= ~HudElementStates.CanIgnoreMasking;
                }
            }

            /// <summary>
            /// Indicates whether or not the element is capturing the cursor.
            /// </summary>
            public virtual bool IsMousedOver => (State & HudElementStates.IsMousedOver) > 0;

            /// <summary>
            /// Element size
            /// </summary>
            protected Vector2 _size;

            /// <summary>
            /// Element padding
            /// </summary>
            protected Vector2 _padding;

            /// <summary>
            /// Values used internally to minimize property calls. Should be treated as read only.
            /// </summary>
            protected Vector2 cachedOrigin, cachedPosition, cachedSize, cachedPadding;

            protected BoundingBox2? maskingBox;
            protected HudElementBase _parentFull;
            private Vector2 originAlignment;

            /// <summary>
            /// Initializes a new hud element with cursor sharing enabled and scaling set to 1f.
            /// </summary>
            public HudElementBase(HudParentBase parent) : base(parent)
            {
                DimAlignment = DimAlignments.None;
                ParentAlignment = ParentAlignments.Center;
            }

            /// <summary>
            /// Used to check whether the cursor is moused over the element and whether its being
            /// obstructed by another element.
            /// </summary>
            protected override void InputDepth()
            {
                State &= ~HudElementStates.IsMouseInBounds;

                if (HudMain.InputMode != HudInputMode.NoInput && (HudSpace?.IsFacingCamera ?? false))
                {
                    Vector3 cursorPos = HudSpace.CursorPos;
                    Vector2 halfSize = Vector2.Max(cachedSize, new Vector2(minMouseBounds)) * .5f;
                    BoundingBox2 box = new BoundingBox2(cachedPosition - halfSize, cachedPosition + halfSize);
                    bool mouseInBounds;

                    if (maskingBox == null)
                        mouseInBounds = box.Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;
                    else
                        mouseInBounds = box.Intersect(maskingBox.Value).Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;

                    if (mouseInBounds)
                    {
                        State |= HudElementStates.IsMouseInBounds;
                        HudMain.Cursor.TryCaptureHudSpace(cursorPos.Z, HudSpace.GetHudSpaceFunc);
                    }
                }
            }

            /// <summary>
            /// Updates input for the element and its children. Overriding this method is rarely necessary.
            /// If you need to update input, use HandleInput().
            /// </summary>
            public override void BeginInput()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        State &= ~(HudElementStates.IsMousedOver | HudElementStates.WasParentInputEnabled);

                        if (_parent != null)
                            State |= _parent.InputEnabled ? HudElementStates.WasParentInputEnabled : HudElementStates.None;

                        if (Visible && InputEnabled)
                        {
                            Vector3 cursorPos = HudSpace.CursorPos;
                            bool mouseInBounds = (State & HudElementStates.IsMouseInBounds) > 0;

                            if (UseCursor && mouseInBounds && !HudMain.Cursor.IsCaptured && HudMain.Cursor.IsCapturingSpace(HudSpace.GetHudSpaceFunc))
                            {
                                bool isMousedOver = mouseInBounds;

                                if (isMousedOver)
                                    State |= HudElementStates.IsMousedOver;

                                HandleInput(new Vector2(cursorPos.X, cursorPos.Y));

                                if (!ShareCursor)
                                    HudMain.Cursor.Capture(accessorDelegates.Item1);
                            }
                            else
                            {
                                HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Updates layout for the element and its children. Overriding this method is rarely necessary. 
            /// If you need to update layout, use Layout().
            /// </summary>
            public override void BeginLayout(bool refresh)
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData, _parent);

                        if (_parent == null)
                        {
                            ParentVisible = false;
                        }
                        else
                        {
                            ParentVisible = _parent.Visible;
                        }

                        if (Visible || refresh)
                        {
                            UpdateCache();
                            Layout();

                            // Update cached values for use on draw and by child nodes
                            cachedPadding = Padding;
                            cachedSize = new Vector2(Width, Height);
                            cachedPosition = cachedOrigin + Offset;

                            UpdateMasking();
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Used to immediately draw billboards. Overriding this method is rarely necessary. 
            /// If you need to draw something, use Draw().
            /// </summary>
            public override void BeginDraw()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if (Visible)
                        {
                            UpdateCache();
                            Draw();
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Updates cached values as well as parent and dim alignment.
            /// </summary>
            protected void UpdateCache()
            {
                cachedPadding = Padding;

                if (_parentFull != null)
                {
                    GetDimAlignment();
                    originAlignment = GetParentAlignment();
                    cachedOrigin = _parentFull.cachedPosition + originAlignment;
                    cachedPosition = cachedOrigin + Offset;
                }
                else
                {
                    cachedSize = new Vector2(Width, Height);
                    cachedOrigin = Vector2.Zero;
                    cachedPosition = cachedOrigin + Offset;
                }
            }

            /// <summary>
            /// Updates masking state and bounding boxes used to mask billboards
            /// </summary>
            private void UpdateMasking()
            {
                if (_parentFull != null &&
                    (_parentFull.State & HudElementStates.IsMasked) > 0 &&
                    (State & HudElementStates.CanIgnoreMasking) == 0
                )
                    State |= HudElementStates.IsMasked;
                else
                    State &= ~HudElementStates.IsMasked;

                if ((State & HudElementStates.IsMasking) > 0 || (_parentFull != null && (State & HudElementStates.IsSelectivelyMasked) > 0))
                {
                    State |= HudElementStates.IsMasked;
                    BoundingBox2? parentBox, box = null;

                    if ((State & HudElementStates.CanIgnoreMasking) > 0)
                    {
                        parentBox = null;
                    }
                    else if (_parentFull != null && (State & HudElementStates.IsSelectivelyMasked) > 0)
                    {
                        Vector2 halfParent = .5f * _parentFull.cachedSize;
                        parentBox = new BoundingBox2(
                            -halfParent + _parentFull.cachedPosition,
                            halfParent + _parentFull.cachedPosition
                        );

                        if (_parentFull.maskingBox != null)
                            parentBox = parentBox.Value.Intersect(_parentFull.maskingBox.Value);
                    }
                    else
                        parentBox = _parentFull?.maskingBox;

                    if ((State & HudElementStates.IsMasking) > 0)
                    {
                        Vector2 halfSize = .5f * cachedSize;
                        box = new BoundingBox2(
                            -halfSize + cachedPosition,
                            halfSize + cachedPosition
                        );
                    }

                    if (parentBox != null && box != null)
                        box = box.Value.Intersect(parentBox.Value);
                    else if (box == null)
                        box = parentBox;

                    maskingBox = box;
                }
                else if ((State & HudElementStates.IsMasked) > 0)
                {
                    maskingBox = _parentFull?.maskingBox;
                }
                else
                {
                    maskingBox = null;
                }
            }

            /// <summary>
            /// Updates element dimensions to match those of its parent in accordance
            /// with its DimAlignment.
            /// </summary>
            private void GetDimAlignment()
            {
                float width = Width, height = Height;

                if (DimAlignment != DimAlignments.None)
                {
                    float parentWidth = _parentFull.cachedSize.X, parentHeight = _parentFull.cachedSize.Y;

                    if ((DimAlignment & DimAlignments.IgnorePadding) == DimAlignments.IgnorePadding)
                    {
                        Vector2 parentPadding = _parentFull.cachedPadding;

                        if ((DimAlignment & DimAlignments.Width) == DimAlignments.Width)
                            width = parentWidth - parentPadding.X;

                        if ((DimAlignment & DimAlignments.Height) == DimAlignments.Height)
                            height = parentHeight - parentPadding.Y;
                    }
                    else
                    {
                        if ((DimAlignment & DimAlignments.Width) == DimAlignments.Width)
                            width = parentWidth;

                        if ((DimAlignment & DimAlignments.Height) == DimAlignments.Height)
                            height = parentHeight;
                    }

                    Width = width;
                    Height = height;
                }

                cachedSize = new Vector2(width, height);
            }

            /// <summary>
            /// Calculates the offset necessary to achieve the alignment specified by the
            /// ParentAlignment property.
            /// </summary>
            private Vector2 GetParentAlignment()
            {
                Vector2 alignment = Vector2.Zero,
                    max = (_parentFull.cachedSize + cachedSize) * .5f,
                    min = -max;

                if ((ParentAlignment & ParentAlignments.UsePadding) == ParentAlignments.UsePadding)
                {
                    min += _parentFull.cachedPadding * .5f;
                    max -= _parentFull.cachedPadding * .5f;
                }

                if ((ParentAlignment & ParentAlignments.InnerV) == ParentAlignments.InnerV)
                {
                    min.Y += cachedSize.Y;
                    max.Y -= cachedSize.Y;
                }

                if ((ParentAlignment & ParentAlignments.InnerH) == ParentAlignments.InnerH)
                {
                    min.X += cachedSize.X;
                    max.X -= cachedSize.X;
                }

                if ((ParentAlignment & ParentAlignments.Bottom) == ParentAlignments.Bottom)
                    alignment.Y = min.Y;
                else if ((ParentAlignment & ParentAlignments.Top) == ParentAlignments.Top)
                    alignment.Y = max.Y;

                if ((ParentAlignment & ParentAlignments.Left) == ParentAlignments.Left)
                    alignment.X = min.X;
                else if ((ParentAlignment & ParentAlignments.Right) == ParentAlignments.Right)
                    alignment.X = max.X;

                return alignment;
            }

            protected override object GetOrSetApiMember(object data, int memberEnum)
            {
                switch ((HudElementAccessors)memberEnum)
                {
                    case HudElementAccessors.Position:
                        return Position;
                    case HudElementAccessors.Size:
                        return Size;
                }

                return base.GetOrSetApiMember(data, memberEnum);
            }
        }
    }
}