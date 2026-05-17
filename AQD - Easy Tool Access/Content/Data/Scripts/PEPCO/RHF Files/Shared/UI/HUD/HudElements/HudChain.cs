using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Used to control sizing behavior of HudChain members and the containing chain element itself. The align axis
        /// is the axis chain elements are arranged on; the off axis is the other axis. When vertically aligned, Y is 
        /// the align axis and X is the off axis. Otherwise, it's reversed.
        /// </summary>
        public enum HudChainSizingModes : int
        {
            // Naming: [Clamp/Fit][Chain/Members][OffAxis/AlignAxis/Both]
            // Fit > Clamp

            // Chain Sizing

            /// <summary>
            /// In this mode, the size of the chain on it's off axis will be allowed to vary freely so long as it
            /// is large enough to contain its members on that axis.
            /// </summary>
            ClampChainOffAxis = 0x1,

            /// <summary>
            /// In this mode, the size of the chain on it's align axis will be allowed to vary freely so long as it
            /// is large enough to contain its members on that axis.
            /// </summary>
            ClampChainAlignAxis = 0x2,

            /// <summary>
            /// In this mode, the chain's size will be allowed to vary freely so long as it remains large enough
            /// to contain them.
            /// </summary>
            ClampChainBoth = ClampChainOffAxis | ClampChainAlignAxis,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents on its off axis.
            /// Supercedes ClampChainOffAxis.
            /// </summary>
            FitChainOffAxis = 0x4,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents on its align axis.
            /// Supercedes ClampChainAlignAxis.
            /// </summary>
            FitChainAlignAxis = 0x8,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents.
            /// Supercedes ClampChainBoth.
            /// </summary>
            FitChainBoth = FitChainOffAxis | FitChainAlignAxis,

            // Member Sizing

            /// <summary>
            /// If this flag is set, then the size of chain members on the off axis will be clamped. 
            /// </summary>
            ClampMembersOffAxis = 0x10,

            /// <summary>
            /// If this flag is set, then the size of chain members on the align axis will be clamped. 
            /// </summary>
            ClampMembersAlignAxis = 0x20,

            /// <summary>
            /// In this mode, chain members will be clamped between the set min/max size on both axes. Superceeds FitToMembers.
            /// </summary>
            ClampMembersBoth = ClampMembersOffAxis | ClampMembersAlignAxis,

            /// <summary>
            /// If this flag is set, chain members will be automatically resized to fill the chain along the off axis. 
            /// Superceeds ClampMembersOffAxis.
            /// </summary>
            FitMembersOffAxis = 0x40,

            /// <summary>
            /// If this flag is set, then the size of chain members on the align axis will be set to the maximum size. 
            /// Superceeds ClampMembersAlignAxis.
            /// </summary>
            FitMembersAlignAxis = 0x80,

            /// <summary>
            /// In this mode, chain members will be set to the maximum size on both axes. Superceeds ClampMembersBoth.
            /// </summary>
            FitMembersBoth = FitMembersOffAxis | FitMembersAlignAxis,
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        /*
         Rules:
            1) Chain members must fit inside the chain. How this is accomplished depends on the sizing mode.
            2) Members must be positioned within the chain's bounds.
            3) Members are assumed to be compatible with the specified sizing mode. Otherwise the behavior is undefined
            and incorrect positioning and sizing will occur.
        */
        public class HudChain<TElementContainer, TElement> : HudCollection<TElementContainer, TElement>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            protected const HudElementStates nodeSetVisible = HudElementStates.IsVisible | HudElementStates.IsRegistered;

            /// <summary>
            /// Width of the chain
            /// </summary>
            public override float Width
            {
                set
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    _size.X = value;

                    if (value > 0f && offAxis == 0 && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.X = _size.X;
                }
            }

            /// <summary>
            /// Height of the chain
            /// </summary>
            public override float Height
            {
                set
                {
                    if (value > Padding.Y)
                        value -= Padding.Y;

                    _size.Y = value;

                    if (value > 0f && offAxis == 1 && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.Y = _size.Y;
                }
            }

            /// <summary>
            /// Maximum chain member size. If no maximum is set, then the currently set size will be used as the maximum.
            /// </summary>
            public Vector2 MemberMaxSize { get { return _absMaxSize; } set { _absMaxSize = value; } }

            /// <summary>
            /// Minimum allowable member size.
            /// </summary>
            public Vector2 MemberMinSize { get { return _absMinSize; } set { _absMinSize = value; } }

            /// <summary>
            /// Distance between chain elements along their axis of alignment.
            /// </summary>
            public float Spacing { get { return _spacing; } set { _spacing = value; } }

            /// <summary>
            /// Determines how/if the chain will attempt to resize member elements. Default sizing mode is 
            /// HudChainSizingModes.FitChainBoth.
            /// </summary>
            public HudChainSizingModes SizingMode { get; set; }

            /// <summary>
            /// Determines whether or not chain elements will be aligned vertically.
            /// </summary>
            public virtual bool AlignVertical 
            { 
                get { return _alignVertical; }
                set 
                {
                    if (value)
                    {
                        alignAxis = 1;
                        offAxis = 0;
                    }
                    else
                    {
                        alignAxis = 0;
                        offAxis = 1;
                    }

                    _alignVertical = value;
                }
            }

            protected bool _alignVertical;
            protected float _spacing;
            protected int alignAxis, offAxis;
            protected Vector2 _absMaxSize, _absMinSize;

            public HudChain(bool alignVertical, HudParentBase parent = null) : base(parent)
            {
                Init();

                Spacing = 0f;
                SizingMode = HudChainSizingModes.FitChainBoth;
                AlignVertical = alignVertical;
            }

            public HudChain(HudParentBase parent) : this(false, parent)
            { }

            public HudChain() : this(false, null)
            { }

            /// <summary>
            /// Initialzer called before the constructor.
            /// </summary>
            protected virtual void Init() { }

            protected override void Layout()
            {
                UpdateMemberSizes();

                Vector2 visibleTotalSize = GetVisibleTotalSize(),
                    listSize = GetListSize(cachedSize - cachedPadding, visibleTotalSize);

                _size = listSize;

                // Calculate member start offset
                Vector2 startOffset = new Vector2();

                if (alignAxis == 1)
                    startOffset.Y = listSize.Y * .5f;
                else
                    startOffset.X = -listSize.X * .5f;

                UpdateMemberOffsets(startOffset, cachedPadding);
            }

            /// <summary>
            /// Updates chain member sizes to conform to sizing rules.
            /// </summary>
            protected void UpdateMemberSizes()
            {
                Vector2 newMax;
                _absMinSize = Vector2.Max(Vector2.Zero, _absMinSize);
                newMax = Vector2.Max(_absMinSize, _absMaxSize);
                _absMaxSize = newMax;

                Vector2 minSize = MemberMinSize,
                    maxSize = MemberMaxSize;

                for (int n = 0; n < hudCollectionList.Count; n++)
                {
                    TElement element = hudCollectionList[n].Element;
                    Vector2 elementSize = element.Size;

                    // Adjust element size based on sizing mode
                    if ((SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0)
                        elementSize[offAxis] = maxSize[offAxis];
                    else if ((SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0)
                        elementSize[offAxis] = MathHelper.Clamp(elementSize[offAxis], minSize[offAxis], maxSize[offAxis]);

                    if ((SizingMode & HudChainSizingModes.FitMembersAlignAxis) > 0)
                        elementSize[alignAxis] = maxSize[alignAxis];
                    else if ((SizingMode & HudChainSizingModes.ClampMembersAlignAxis) > 0)
                        elementSize[alignAxis] = MathHelper.Clamp(elementSize[alignAxis], minSize[alignAxis], maxSize[alignAxis]);

                    element.Size = elementSize;
                }
            }

            /// <summary>
            /// Calculates the chain's current size based on its sizing mode and the total
            /// size of its members (less padding).
            /// </summary>
            protected Vector2 GetListSize(Vector2 lastSize, Vector2 totalSize)
            {
                if ((SizingMode & HudChainSizingModes.FitChainAlignAxis) > 0)
                {
                    lastSize[alignAxis] = totalSize[alignAxis];
                }
                else // if ClampChainAlignAxis
                {
                    lastSize[alignAxis] = Math.Max(lastSize[alignAxis], totalSize[alignAxis]);
                }

                if ((SizingMode & HudChainSizingModes.FitChainOffAxis) > 0)
                {
                    lastSize[offAxis] = totalSize[offAxis];
                }
                else // if ClampChainOffAxis
                {
                    lastSize[offAxis] = Math.Max(lastSize[offAxis], totalSize[offAxis]);
                }

                return lastSize;
            }

            /// <summary>
            /// Updates chain member offsets to ensure that they're in a straight line.
            /// </summary>
            protected void UpdateMemberOffsets(Vector2 offset, Vector2 padding)
            {
                Vector2 alignMask = new Vector2(offAxis, -alignAxis), offMask = new Vector2(alignAxis, -offAxis);
                ParentAlignments left = (ParentAlignments)((int)ParentAlignments.Left * (2 - alignAxis)),
                    right = (ParentAlignments)((int)ParentAlignments.Right * (2 - alignAxis)),
                    bitmask = left | right;
                float spacing = Spacing;

                for (int n = 0; n < hudCollectionList.Count; n++)
                {
                    TElement element = hudCollectionList[n].Element;

                    // Calculate element size
                    Vector2 elementSize = element.Size;

                    // Enforce alignment restrictions
                    element.ParentAlignment &= bitmask;
                    element.ParentAlignment |= ParentAlignments.Inner;

                    // Calculate element offset
                    Vector2 newOffset = offset + (elementSize * alignMask * .5f);

                    if ((element.ParentAlignment & left) == left)
                    {
                        newOffset += padding * offMask * .5f;
                    }
                    else if ((element.ParentAlignment & right) == right)
                    {
                        newOffset -= padding * offMask * .5f;
                    }

                    // Apply changes
                    element.Offset = newOffset;

                    if ((element.State & (nodeSetVisible)) == nodeSetVisible)
                    {
                        // Move offset down for the next element
                        elementSize[alignAxis] += spacing;
                        offset += elementSize * alignMask;
                    }
                }
            }

            /// <summary>
            /// Calculates the total size of all visible elements in the chain, including spacing and
            /// any resizing that might be required.
            /// </summary>
            protected virtual Vector2 GetVisibleTotalSize()
            {
                Vector2 newSize = new Vector2();
                float spacing = Spacing;

                for (int n = 0; n < hudCollectionList.Count; n++)
                {
                    TElement element = hudCollectionList[n].Element;
                    
                    if ((element.State & (nodeSetVisible)) == nodeSetVisible)
                    {
                        Vector2 elementSize = element.Size;

                        // Total up the size of elements on the axis of alignment
                        newSize[alignAxis] += elementSize[alignAxis];

                        // Find largest element on the off axis
                        if (elementSize[offAxis] > newSize[offAxis])
                            newSize[offAxis] = elementSize[offAxis];

                        newSize[alignAxis] += spacing;
                    }
                }

                newSize[alignAxis] -= spacing;
                return Vector2.Max(newSize, Vector2.Zero);
            }
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        public class HudChain<TElementContainer> : HudChain<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        {
            public HudChain(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
            { }
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        public class HudChain : HudChain<HudElementContainer<HudElementBase>, HudElementBase>
        {
            public HudChain(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
            { }
        }
    }
}
