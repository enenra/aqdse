using System;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage;

namespace RichHudFramework.UI
{
    using Rendering;

    /// <summary>
    /// Indented, collapsable list. Designed to fit in with SE UI elements.
    /// </summary>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class TreeList<TValue> : TreeList<ListBoxEntry<TValue>, Label, TValue>
    {
        public TreeList(HudParentBase parent) : base(parent)
        { }

        public TreeList() : base(null)
        { }
    }

    /// <summary>
    /// Indented, collapsable list. Designed to fit in with SE UI elements.
    /// </summary>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class TreeList<TElement, TValue> : TreeList<ListBoxEntry<TElement, TValue>, TElement, TValue>
        where TElement : HudElementBase, IMinLabelElement, new()
    {
        public TreeList(HudParentBase parent) : base(parent)
        { }

        public TreeList() : base(null)
        { }
    }

    /// <summary>
    /// Generic indented collapsable list of pooled, uniformly-sized entries. Allows use of custom entry element types. 
    /// Designed to fit in with SE UI elements.
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class TreeList<TContainer, TElement, TValue> 
        : TreeBoxBase<
            ChainSelectionBox<TContainer, TElement, TValue>,
            HudChain<TContainer, TElement>,
            TContainer,
            TElement
        >
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
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
                    selectionBox.LineHeight = value;
                }
            }
        }

        public TreeList(HudParentBase parent) : base(parent)
        {
            selectionBox.border.Visible = false;
            selectionBox.hudChain.SizingMode = 
                HudChainSizingModes.FitMembersBoth | 
                HudChainSizingModes.ClampChainOffAxis | 
                HudChainSizingModes.FitChainAlignAxis;
        }

        public TreeList() : this(null)
        { }

        /// <summary>
        /// Adds a new member to the tree box with the given name and associated
        /// object.
        /// </summary>
        public TContainer Add(RichText name, TValue assocMember, bool enabled = true) =>
            selectionBox.Add(name, assocMember, enabled);

        /// <summary>
        /// Adds the given range of entries to the tree box.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, TValue, bool>> entries) =>
            selectionBox.AddRange(entries);

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText name, TValue assocMember, bool enabled = true) =>
            selectionBox.Insert(index, name, assocMember, enabled);

        /// <summary>
        /// Removes the member at the given index from the tree box.
        /// </summary>
        public void RemoveAt(int index) =>
            selectionBox.RemoveAt(index);

        /// <summary>
        /// Removes the specified range of indices from the tree box.
        /// </summary>
        public void RemoveRange(int index, int count) =>
            selectionBox.RemoveRange(index, count);

        /// <summary>
        /// Clears the current selection
        /// </summary>
        public void ClearEntries() =>
            selectionBox.ClearEntries();

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(TValue assocMember)
        {
            int index = selectionBox.hudChain.FindIndex(x => assocMember.Equals(x.AssocMember));

            if (index != -1)
            {
                selectionBox.SetSelectionAt(index);
            }
        }
    }
}